using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RpcSourceGenerator
{
    [Generator]
    public class RpcClientGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Find all classes that inherit from Client and have partial RPC methods
            var clientClasses = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => node is ClassDeclarationSyntax,
                    transform: static (ctx, _) => GetClientClassOrNull(ctx))
                .Where(static m => m is not null);

            // Combine with compilation
            var compilationAndClasses = context.CompilationProvider.Combine(clientClasses.Collect());

            // Generate source for each client class
            context.RegisterSourceOutput(compilationAndClasses, static (spc, source) => Execute(source.Left, source.Right, spc));
        }

        private static ClassDeclarationSyntax GetClientClassOrNull(GeneratorSyntaxContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;

            // Must be partial
            if (!classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
                return null;

            // Check if inherits from Client
            var symbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
            if (symbol == null)
                return null;

            var baseType = symbol.BaseType;
            while (baseType != null)
            {
                if (baseType.Name == "Client" && baseType.ContainingNamespace?.ToString() == "Core.Infrastructure.Network")
                    return classDeclaration;
                baseType = baseType.BaseType;
            }

            return null;
        }

        private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
        {
            if (classes.IsDefaultOrEmpty)
                return;

            foreach (var classDeclaration in classes)
            {
                var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
                if (classSymbol == null)
                    continue;

                // Find all partial methods with [Rpc] attribute
                var rpcMethods = new List<RpcClientMethodInfo>();

                foreach (var member in classSymbol.GetMembers().OfType<IMethodSymbol>())
                {
                    // Must be partial
                    if (!member.IsPartialDefinition)
                        continue;

                    var rpcAttribute = member.GetAttributes()
                        .FirstOrDefault(a => a.AttributeClass?.Name == "RpcAttribute" && 
                                           a.AttributeClass.ContainingNamespace?.ToString() == "Core.Infrastructure.Network");

                    if (rpcAttribute == null)
                        continue;

                    // Get request opcode from attribute
                    if (rpcAttribute.ConstructorArguments.Length == 0)
                        continue;

                    var requestOpcode = (byte)rpcAttribute.ConstructorArguments[0].Value;
                    
                    // Get optional response opcode (if specified)
                    byte? responseOpcode = null;
                    if (rpcAttribute.ConstructorArguments.Length > 1)
                    {
                        responseOpcode = (byte)rpcAttribute.ConstructorArguments[1].Value;
                    }

                    // Analyze method signature
                    var methodName = member.Name;
                    var isAsync = member.ReturnType.Name == "Task";
                    var hasReturnValue = false;
                    string returnType = null;
                    string requestType = null;

                    // Determine return type
                    if (isAsync && member.ReturnType is INamedTypeSymbol namedReturnType && namedReturnType.TypeArguments.Length > 0)
                    {
                        // Task<T> - async with return value
                        hasReturnValue = true;
                        returnType = namedReturnType.TypeArguments[0].ToDisplayString();
                    }
                    else if (!isAsync && !member.ReturnsVoid)
                    {
                        // T - sync with return value
                        hasReturnValue = true;
                        returnType = member.ReturnType.ToDisplayString();
                    }

                    // Get request parameter type
                    if (member.Parameters.Length > 0)
                    {
                        requestType = member.Parameters[0].Type.ToDisplayString();
                    }

                    // If response opcode not explicitly set, use request opcode
                    if (!responseOpcode.HasValue && hasReturnValue)
                    {
                        responseOpcode = requestOpcode;
                    }

                    rpcMethods.Add(new RpcClientMethodInfo
                    {
                        MethodAccessibility = GetAccessibilityModifier(member.DeclaredAccessibility),
                        MethodName = methodName,
                        RequestOpcode = requestOpcode,
                        ResponseOpcode = responseOpcode,
                        RequestType = requestType,
                        ReturnType = returnType,
                        IsAsync = isAsync,
                        HasReturnValue = hasReturnValue
                    });
                }

                if (rpcMethods.Count == 0)
                    continue;

                // Generate source
                var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
                var className = classSymbol.Name;

                var source = GenerateSource(namespaceName, className, rpcMethods);
                context.AddSource($"{className}.RpcClient.g.cs", source);
            }
        }

        private static string GenerateSource(string namespaceName, string className, List<RpcClientMethodInfo> methods)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine();
            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine("{");
            sb.AppendLine($"    partial class {className}");
            sb.AppendLine("    {");

            foreach (var method in methods)
            {
                GenerateMethod(sb, method);
                sb.AppendLine();
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static void GenerateMethod(StringBuilder sb, RpcClientMethodInfo method)
        {
            // Build parameter list
            var parameters = method.RequestType != null ? $"{method.RequestType} request" : "";

            // Build method signature
            string returnTypeStr;
            if (method.IsAsync)
            {
                returnTypeStr = method.HasReturnValue ? $"Task<{method.ReturnType}>" : "Task";
            }
            else
            {
                returnTypeStr = method.HasReturnValue ? method.ReturnType : "void";
            }

            sb.AppendLine($"        {method.MethodAccessibility} partial {returnTypeStr} {method.MethodName}({parameters})");
            sb.AppendLine("        {");

            // Generate method body based on pattern
            if (method.HasReturnValue)
            {
                // Request-response pattern
                if (method.IsAsync)
                {
                    // Async: return SendRequestAsync<TReq, TResp>(requestOpcode, responseOpcode, request)
                    sb.AppendLine($"            return SendRequestAsync<{method.RequestType}, {method.ReturnType}>(0x{method.RequestOpcode:X2}, 0x{method.ResponseOpcode:X2}, request);");
                }
                else
                {
                    // Sync: return SendRequest<TReq, TResp>(requestOpcode, responseOpcode, request)
                    sb.AppendLine($"            return SendRequest<{method.RequestType}, {method.ReturnType}>(0x{method.RequestOpcode:X2}, 0x{method.ResponseOpcode:X2}, request);");
                }
            }
            else
            {
                // Fire-and-forget pattern
                if (method.RequestType != null)
                {
                    sb.AppendLine($"            SendRequest(0x{method.RequestOpcode:X2}, request);");
                }
                else
                {
                    // No request parameter - send empty request
                    sb.AppendLine($"            SendRequest(0x{method.RequestOpcode:X2}, new object());");
                }

                if (method.IsAsync)
                {
                    sb.AppendLine("            return Task.CompletedTask;");
                }
            }

            sb.AppendLine("        }");
        }
        
        private static string GetAccessibilityModifier(Accessibility accessibility)
        {
            return accessibility switch
            {
                Accessibility.Public => "public",
                Accessibility.Internal => "internal",
                Accessibility.Protected => "protected",
                Accessibility.ProtectedOrInternal => "protected internal",
                Accessibility.ProtectedAndInternal => "private protected",
                Accessibility.Private => "private",
                _ => ""
            };
        }

        private class RpcClientMethodInfo
        {
            public string MethodAccessibility { get; set; }
            public string MethodName { get; set; }
            public byte RequestOpcode { get; set; }
            public byte? ResponseOpcode { get; set; }
            public string RequestType { get; set; }
            public string ReturnType { get; set; }
            public bool IsAsync { get; set; }
            public bool HasReturnValue { get; set; }
        }
    }
}
