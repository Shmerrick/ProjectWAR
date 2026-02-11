using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace FrameWork.NetWork.SourceGenerators
{
    [Generator]
    public class RpcSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Find all classes that inherit from Client
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
                if (baseType.Name == "Client" && baseType.ContainingNamespace?.ToString() == "FrameWork.NetWork.V4")
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

                // Find all methods with [Rpc] attribute
                var rpcMethods = new List<RpcMethodInfo>();
                var opcodes = new HashSet<byte>();

                foreach (var member in classSymbol.GetMembers().OfType<IMethodSymbol>())
                {
                    var rpcAttribute = member.GetAttributes()
                        .FirstOrDefault(a => a.AttributeClass?.Name == "RpcAttribute" && 
                                           a.AttributeClass.ContainingNamespace?.ToString() == "FrameWork.NetWork.V4");

                    if (rpcAttribute == null)
                        continue;

                    // Get opcode from attribute
                    if (rpcAttribute.ConstructorArguments.Length == 0)
                        continue;

                    var opcodeValue = rpcAttribute.ConstructorArguments[0].Value;
                    if (opcodeValue == null)
                        continue;

                    byte opcode = (byte)opcodeValue;

                    // Get optional response opcode (defaults to request opcode if not specified)
                    byte responseOpcode = opcode;
                    if (rpcAttribute.ConstructorArguments.Length > 1)
                    {
                        var responseOpcodeValue = rpcAttribute.ConstructorArguments[1].Value;
                        if (responseOpcodeValue != null)
                            responseOpcode = (byte)responseOpcodeValue;
                    }

                    // Check for duplicate opcodes
                    if (!opcodes.Add(opcode))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            new DiagnosticDescriptor(
                                "RPC001",
                                "Duplicate opcode",
                                $"Opcode 0x{opcode:X2} is already used by another handler in class {classSymbol.Name}",
                                "RpcGenerator",
                                DiagnosticSeverity.Error,
                                isEnabledByDefault: true),
                            member.Locations.FirstOrDefault()));
                        continue;
                    }

                    // Validate method signature (0 or 1 parameters allowed)
                    if (member.Parameters.Length > 1)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            new DiagnosticDescriptor(
                                "RPC002",
                                "Invalid RPC handler signature",
                                $"RPC handler '{member.Name}' must have zero or one parameter",
                                "RpcGenerator",
                                DiagnosticSeverity.Error,
                                isEnabledByDefault: true),
                            member.Locations.FirstOrDefault()));
                        continue;
                    }

                    ITypeSymbol requestType = member.Parameters.Length == 1 ? member.Parameters[0].Type : null;
                    var returnType = member.ReturnType;
                    bool isAsync = returnType.Name == "Task" || returnType.Name == "ValueTask";
                    bool hasResponse = false;
                    ITypeSymbol responseType = null;

                    if (isAsync && returnType is INamedTypeSymbol namedReturnType && namedReturnType.TypeArguments.Length > 0)
                    {
                        hasResponse = true;
                        responseType = namedReturnType.TypeArguments[0];
                    }
                    else if (!isAsync && returnType.SpecialType != SpecialType.System_Void)
                    {
                        hasResponse = true;
                        responseType = returnType;
                    }

                    rpcMethods.Add(new RpcMethodInfo
                    {
                        MethodName = member.Name,
                        Opcode = opcode,
                        ResponseOpcode = responseOpcode,
                        RequestType = requestType?.ToDisplayString(),
                        ResponseType = responseType?.ToDisplayString(),
                        HasResponse = hasResponse,
                        IsAsync = isAsync
                    });
                }

                if (rpcMethods.Count > 0)
                {
                    var source = GenerateSource(classSymbol, rpcMethods);
                    context.AddSource($"{classSymbol.Name}_RpcGenerated.g.cs", source);
                }
            }
        }

        private static string GenerateSource(INamedTypeSymbol classSymbol, List<RpcMethodInfo> methods)
        {
            var namespaceName = classSymbol.ContainingNamespace?.ToDisplayString();
            var className = classSymbol.Name;

            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("#nullable enable");
            sb.AppendLine();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine();

            if (!string.IsNullOrEmpty(namespaceName))
            {
                sb.AppendLine($"namespace {namespaceName}");
                sb.AppendLine("{");
            }

            sb.AppendLine($"    partial class {className}");
            sb.AppendLine("    {");
            sb.AppendLine("        protected override void ProcessPacket(byte opcode, ReadOnlySpan<byte> payload)");
            sb.AppendLine("        {");
            sb.AppendLine("            switch (opcode)");
            sb.AppendLine("            {");

            foreach (var method in methods.OrderBy(m => m.Opcode))
            {
                sb.AppendLine($"                case 0x{method.Opcode:X2}:");
                sb.AppendLine("                {");
                
                if (method.RequestType != null)
                {
                    sb.AppendLine($"                    var request = Serializer.Deserialize<{method.RequestType}>(payload);");
                }

                if (method.IsAsync)
                {
                    string args = method.RequestType != null ? "request" : "";
                    sb.AppendLine($"                    _ = HandleAsync_{method.MethodName}({args});");
                }
                else
                {
                    if (method.HasResponse)
                    {
                        string args = method.RequestType != null ? "request" : "";
                        sb.AppendLine($"                    var response = {method.MethodName}({args});");
                        sb.AppendLine($"                    if (response != null)");
                        sb.AppendLine($"                        SendResponse(0x{method.ResponseOpcode:X2}, response);");
                    }
                    else
                    {
                        string args = method.RequestType != null ? "request" : "";
                        sb.AppendLine($"                    {method.MethodName}({args});");
                    }
                }

                sb.AppendLine("                    break;");
                sb.AppendLine("                }");
            }

            sb.AppendLine("                default:");
            sb.AppendLine("                    OnUnknownOpcode(opcode);");
            sb.AppendLine("                    break;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");

            // Generate async wrapper methods
            foreach (var method in methods.Where(m => m.IsAsync))
            {
                sb.AppendLine();
                string parameters = method.RequestType != null ? $"{method.RequestType} request" : "";
                string args = method.RequestType != null ? "request" : "";
                sb.AppendLine($"        private async Task HandleAsync_{method.MethodName}({parameters})");
                sb.AppendLine("        {");
                sb.AppendLine("            try");
                sb.AppendLine("            {");

                if (method.HasResponse)
                {
                    sb.AppendLine($"                var response = await {method.MethodName}({args});");
                    sb.AppendLine($"                if (response != null)");
                    sb.AppendLine($"                    SendResponse(0x{method.ResponseOpcode:X2}, response);");
                }
                else
                {
                    sb.AppendLine($"                await {method.MethodName}({args});");
                }

                sb.AppendLine("            }");
                sb.AppendLine("            catch (Exception ex)");
                sb.AppendLine("            {");
                sb.AppendLine($"                OnHandlerError(0x{method.Opcode:X2}, ex);");
                sb.AppendLine("            }");
                sb.AppendLine("        }");
            }

            sb.AppendLine("    }");

            if (!string.IsNullOrEmpty(namespaceName))
            {
                sb.AppendLine("}");
            }

            return sb.ToString();
        }

        private class RpcMethodInfo
        {
            public string MethodName { get; set; }
            public byte Opcode { get; set; }
            public byte ResponseOpcode { get; set; }
            public string RequestType { get; set; }
            public string ResponseType { get; set; }
            public bool HasResponse { get; set; }
            public bool IsAsync { get; set; }
        }
    }
}
