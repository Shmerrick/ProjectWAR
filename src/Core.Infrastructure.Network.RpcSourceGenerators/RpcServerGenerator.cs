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
    public class RpcServerGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var handlerClasses = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => node is ClassDeclarationSyntax,
                    transform: static (ctx, _) => GetHandlerClassOrNull(ctx))
                .Where(static m => m is not null);

            var compilationAndClasses = context.CompilationProvider.Combine(handlerClasses.Collect());

            context.RegisterSourceOutput(compilationAndClasses,
                static (spc, source) => Execute(source.Left, source.Right, spc));
        }

        private static ClassDeclarationSyntax GetHandlerClassOrNull(GeneratorSyntaxContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;

            var symbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
            if (symbol == null)
                return null;

            // Check if the class implements IPacketHandler interface
            foreach (var iface in symbol.AllInterfaces)
            {
                if (iface.Name == "IPacketHandler" &&
                    iface.ContainingNamespace?.ToString() == "Core.Infrastructure.Network")
                    return classDeclaration;
            }

            return null;
        }

        private static string SanitizeGroupName(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
                return "Default";

            var sb = new StringBuilder();

            foreach (var c in groupName)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                }
                else if (c == '_')
                {
                    sb.Append(c);
                }
                // Skip whitespace and other invalid characters
            }

            var sanitized = sb.ToString();

            // If the result is empty or starts with a digit, prepend an underscore
            if (string.IsNullOrEmpty(sanitized) || char.IsDigit(sanitized[0]))
                sanitized = "_" + sanitized;

            // Check if it's a C# keyword and append underscore
            if (SyntaxFacts.GetKeywordKind(sanitized) != SyntaxKind.None ||
                SyntaxFacts.GetContextualKeywordKind(sanitized) != SyntaxKind.None)
            {
                sanitized = sanitized + "_";
            }

            return sanitized;
        }

        private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes,
            SourceProductionContext context)
        {
            if (classes.IsDefaultOrEmpty)
                return;

            // Collect all handler info grouped by packet group
            var groupedMethods = new Dictionary<string, PacketGroupInfo>();

            foreach (var classDeclaration in classes.Distinct())
            {
                var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
                if (classSymbol == null)
                    continue;

                // Determine packet group
                var groupName = "Default";
                var packetGroupAttr = classSymbol.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.Name == "PacketGroupAttribute" &&
                                         a.AttributeClass.ContainingNamespace?.ToString() == "Core.Infrastructure.Network");
                if (packetGroupAttr != null && packetGroupAttr.ConstructorArguments.Length > 0)
                {
                    var nameArg = packetGroupAttr.ConstructorArguments[0].Value as string;
                    if (!string.IsNullOrEmpty(nameArg))
                        groupName = nameArg;
                }

                if (!groupedMethods.TryGetValue(groupName, out var groupInfo))
                {
                    groupInfo = new PacketGroupInfo
                    {
                        GroupName = groupName,
                        SanitizedGroupName = SanitizeGroupName(groupName),
                        Namespace = classSymbol.ContainingNamespace?.ToDisplayString(),
                        Methods = [],
                        HandlerTypes = [],
                        Opcodes = new Dictionary<byte, (string HandlerName, string MethodName, Location Location)>()
                    };
                    groupedMethods[groupName] = groupInfo;
                }

                var className = classSymbol.ToDisplayString();
                var hasRpcMethods = false;

                foreach (var member in classSymbol.GetMembers().OfType<IMethodSymbol>())
                {
                    var rpcAttribute = member.GetAttributes()
                        .FirstOrDefault(a => a.AttributeClass?.Name == "RpcAttribute" &&
                                             a.AttributeClass.ContainingNamespace?.ToString() ==
                                             "Core.Infrastructure.Network");

                    if (rpcAttribute == null)
                        continue;

                    if (rpcAttribute.ConstructorArguments.Length == 0)
                        continue;

                    var opcodeValue = rpcAttribute.ConstructorArguments[0].Value;
                    if (opcodeValue == null)
                        continue;

                    var opcode = (byte)opcodeValue;

                    var responseOpcode = opcode;
                    if (rpcAttribute.ConstructorArguments.Length > 1)
                    {
                        var responseOpcodeValue = rpcAttribute.ConstructorArguments[1].Value;
                        if (responseOpcodeValue != null)
                            responseOpcode = (byte)responseOpcodeValue;
                    }

                    // Per-group duplicate opcode detection
                    if (groupInfo.Opcodes.TryGetValue(opcode, out var existing))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            new DiagnosticDescriptor(
                                "RPC001",
                                "Duplicate opcode",
                                $"Opcode 0x{opcode:X2} is already used by '{existing.HandlerName}.{existing.MethodName}'",
                                "RpcGenerator",
                                DiagnosticSeverity.Error,
                                isEnabledByDefault: true),
                            member.Locations.FirstOrDefault()));
                        continue;
                    }

                    groupInfo.Opcodes[opcode] = (classSymbol.Name, member.Name, member.Locations.FirstOrDefault());

                    // Classify parameters
                    var parameters = new List<RpcParameterInfo>();
                    RpcParameterInfo requestParam = null;
                    var hasValidationError = false;

                    foreach (var param in member.Parameters)
                    {
                        var fromServicesAttr = param.GetAttributes()
                            .FirstOrDefault(a =>
                                a.AttributeClass?.Name == "FromServicesAttribute" &&
                                a.AttributeClass.ContainingNamespace?.ToString() == "Core.Infrastructure.Network");

                        if (fromServicesAttr != null)
                        {
                            parameters.Add(new RpcParameterInfo
                            {
                                Kind = ParameterKind.Service,
                                TypeName = param.Type.ToDisplayString(),
                                ParameterName = param.Name
                            });
                            continue;
                        }

                        if (param.Type.Name == "IConnectionContext" &&
                            param.Type.ContainingNamespace?.ToString() == "Core.Infrastructure.Network")
                        {
                            parameters.Add(new RpcParameterInfo
                            {
                                Kind = ParameterKind.Context,
                                TypeName = param.Type.ToDisplayString(),
                                ParameterName = param.Name
                            });
                            continue;
                        }

                        if (requestParam != null)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                new DiagnosticDescriptor(
                                    "RPC002",
                                    "Invalid RPC handler signature",
                                    $"RPC handler '{member.Name}' must have at most one request parameter (non-[FromServices], non-IConnectionContext)",
                                    "RpcGenerator",
                                    DiagnosticSeverity.Error,
                                    isEnabledByDefault: true),
                                member.Locations.FirstOrDefault()));
                            hasValidationError = true;
                            break;
                        }

                        requestParam = new RpcParameterInfo
                        {
                            Kind = ParameterKind.Request,
                            TypeName = param.Type.ToDisplayString(),
                            ParameterName = param.Name
                        };
                        parameters.Add(requestParam);
                    }

                    if (hasValidationError)
                        continue;

                    // Determine return type
                    var returnType = member.ReturnType;
                    var isAsync = returnType.Name == "Task" || returnType.Name == "ValueTask";
                    var hasResponse = false;
                    ITypeSymbol responseType = null;

                    if (isAsync && returnType is INamedTypeSymbol namedReturnType &&
                        namedReturnType.TypeArguments.Length > 0)
                    {
                        hasResponse = true;
                        responseType = namedReturnType.TypeArguments[0];
                    }
                    else if (!isAsync && returnType.SpecialType != SpecialType.System_Void)
                    {
                        hasResponse = true;
                        responseType = returnType;
                    }

                    hasRpcMethods = true;
                    groupInfo.Methods.Add(new RpcMethodInfo
                    {
                        MethodName = member.Name,
                        HandlerTypeName = className,
                        Opcode = opcode,
                        ResponseOpcode = responseOpcode,
                        ResponseType = responseType?.ToDisplayString(),
                        HasResponse = hasResponse,
                        IsAsync = isAsync,
                        Parameters = parameters,
                        HasServices = parameters.Any(p => p.Kind == ParameterKind.Service)
                    });
                }

                if (hasRpcMethods && !groupInfo.HandlerTypes.Contains(className))
                {
                    groupInfo.HandlerTypes.Add(className);
                }
            }

            // Generate one dispatcher + extension method per group
            foreach (var kvp in groupedMethods)
            {
                var groupInfo = kvp.Value;
                if (groupInfo.Methods.Count == 0)
                    continue;

                var source = GenerateGroupSource(groupInfo);
                context.AddSource($"{groupInfo.SanitizedGroupName}PacketDispatcher.g.cs", source);
            }
        }

        private static string GenerateGroupSource(PacketGroupInfo group)
        {
            var dispatcherName = $"{group.SanitizedGroupName}PacketDispatcher";
            var extensionMethodName = $"Add{group.SanitizedGroupName}PacketHandlers";
            var extensionClassName = $"{group.SanitizedGroupName}PacketHandlersServiceCollectionExtensions";

            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("#nullable enable");
            sb.AppendLine();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using Core.Infrastructure.Network;");
            sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            sb.AppendLine();

            // Dispatcher class in the first handler's namespace
            if (!string.IsNullOrEmpty(group.Namespace))
            {
                sb.AppendLine($"namespace {group.Namespace}");
                sb.AppendLine("{");
            }

            sb.AppendLine($"    public sealed class {dispatcherName} : IPacketDispatcher");
            sb.AppendLine("    {");
            sb.AppendLine("        public void Dispatch(");
            sb.AppendLine("            byte opcode,");
            sb.AppendLine("            ReadOnlyMemory<byte> payload,");
            sb.AppendLine("            IServiceProvider services,");
            sb.AppendLine("            IPacketSerializer serializer,");
            sb.AppendLine("            IConnectionContext connection)");
            sb.AppendLine("        {");
            sb.AppendLine("            switch (opcode)");
            sb.AppendLine("            {");

            foreach (var method in group.Methods.OrderBy(m => m.Opcode))
            {
                GenerateSwitchCase(sb, method);
            }

            sb.AppendLine("                default:");
            sb.AppendLine("                    break;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");

            // Generate async wrapper methods
            foreach (var method in group.Methods.Where(m => m.IsAsync).OrderBy(m => m.Opcode))
            {
                GenerateAsyncWrapper(sb, method);
            }

            sb.AppendLine("    }"); // end dispatcher class

            if (!string.IsNullOrEmpty(group.Namespace))
            {
                sb.AppendLine("}");
            }

            sb.AppendLine();

            // Extension method for IServiceCollection registration
            sb.AppendLine("namespace Microsoft.Extensions.DependencyInjection");
            sb.AppendLine("{");
            sb.AppendLine($"    public static class {extensionClassName}");
            sb.AppendLine("    {");

            var dispatcherFullName = string.IsNullOrEmpty(group.Namespace)
                ? dispatcherName
                : $"{group.Namespace}.{dispatcherName}";

            sb.AppendLine($"        public static IServerNetworkingBuilder {extensionMethodName}(this IServerNetworkingBuilder services)");
            sb.AppendLine("        {");
            sb.AppendLine($"            services.WithPacketDispatcher<{dispatcherFullName}>();");

            foreach (var handler in group.HandlerTypes)
            {
                sb.AppendLine($"            services.AddHandler<{handler}>();");
            }

            sb.AppendLine("            return services;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static void GenerateSwitchCase(StringBuilder sb, RpcMethodInfo method)
        {
            sb.AppendLine($"                case 0x{method.Opcode:X2}:");
            sb.AppendLine("                {");

            // Resolve handler from connection-scoped services
            sb.AppendLine(
                $"                    var handler = services.GetRequiredService<{method.HandlerTypeName}>();");

            var requestParam = method.Parameters.FirstOrDefault(p => p.Kind == ParameterKind.Request);
            var serviceParams = method.Parameters.Where(p => p.Kind == ParameterKind.Service).ToList();

            // Deserialize request if present
            if (requestParam != null)
            {
                sb.AppendLine(
                    $"                    var request = serializer.Deserialize<{requestParam.TypeName}>(payload.Span);");
            }

            // Create scope for [FromServices] if needed
            var needsScope = serviceParams.Count > 0;
            if (needsScope)
            {
                if (method.IsAsync)
                {
                    sb.AppendLine(
                        "                    var __scope = services.CreateScope();");
                }
                else
                {
                    sb.AppendLine(
                        "                    using var __scope = services.CreateScope();");
                }

                foreach (var svc in serviceParams)
                {
                    sb.AppendLine(
                        $"                    var __svc_{svc.ParameterName} = __scope.ServiceProvider.GetRequiredService<{svc.TypeName}>();");
                }
            }

            // Build argument list in declaration order
            var args = BuildArgumentList(method.Parameters);

            if (method.IsAsync)
            {
                var wrapperArgs = new List<string> { "handler" };
                if (requestParam != null) wrapperArgs.Add("request");
                foreach (var svc in serviceParams) wrapperArgs.Add($"__svc_{svc.ParameterName}");
                wrapperArgs.Add("connection");
                if (needsScope) wrapperArgs.Add("__scope");

                sb.AppendLine(
                    $"                    _ = DispatchAsync_{method.MethodName}({string.Join(", ", wrapperArgs)});");
            }
            else
            {
                if (method.HasResponse)
                {
                    sb.AppendLine($"                    var response = handler.{method.MethodName}({args});");
                    sb.AppendLine("                    if (response != null)");
                    sb.AppendLine(
                        $"                        connection.SendResponse(0x{method.ResponseOpcode:X2}, response);");
                }
                else
                {
                    sb.AppendLine($"                    handler.{method.MethodName}({args});");
                }
            }

            sb.AppendLine("                    break;");
            sb.AppendLine("                }");
        }

        private static void GenerateAsyncWrapper(StringBuilder sb, RpcMethodInfo method)
        {
            var requestParam = method.Parameters.FirstOrDefault(p => p.Kind == ParameterKind.Request);
            var serviceParams = method.Parameters.Where(p => p.Kind == ParameterKind.Service).ToList();
            var needsScope = serviceParams.Count > 0;

            sb.AppendLine();

            var wrapperParams = new List<string> { $"{method.HandlerTypeName} handler" };
            if (requestParam != null)
                wrapperParams.Add($"{requestParam.TypeName} request");
            foreach (var svc in serviceParams)
                wrapperParams.Add($"{svc.TypeName} __svc_{svc.ParameterName}");
            wrapperParams.Add("IConnectionContext connection");
            if (needsScope)
                wrapperParams.Add("IServiceScope __scope");

            sb.AppendLine(
                $"        private static async Task DispatchAsync_{method.MethodName}({string.Join(", ", wrapperParams)})");
            sb.AppendLine("        {");
            sb.AppendLine("            try");
            sb.AppendLine("            {");

            var args = BuildArgumentList(method.Parameters);

            if (method.HasResponse)
            {
                sb.AppendLine($"                var response = await handler.{method.MethodName}({args});");
                sb.AppendLine("                if (response != null)");
                sb.AppendLine(
                    $"                    connection.SendResponse(0x{method.ResponseOpcode:X2}, response);");
            }
            else
            {
                sb.AppendLine($"                await handler.{method.MethodName}({args});");
            }

            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception ex)");
            sb.AppendLine("            {");
            sb.AppendLine($"                connection.OnDispatchError(0x{method.Opcode:X2}, ex);");
            sb.AppendLine("            }");

            if (needsScope)
            {
                sb.AppendLine("            finally");
                sb.AppendLine("            {");
                sb.AppendLine("                __scope.Dispose();");
                sb.AppendLine("            }");
            }

            sb.AppendLine("        }");
        }

        private static string BuildArgumentList(List<RpcParameterInfo> parameters)
        {
            var args = new List<string>();
            foreach (var param in parameters)
            {
                switch (param.Kind)
                {
                    case ParameterKind.Request:
                        args.Add("request");
                        break;
                    case ParameterKind.Context:
                        args.Add("connection");
                        break;
                    case ParameterKind.Service:
                        args.Add($"__svc_{param.ParameterName}");
                        break;
                }
            }

            return string.Join(", ", args);
        }

        private enum ParameterKind
        {
            Request,
            Context,
            Service
        }

        private class RpcParameterInfo
        {
            public ParameterKind Kind { get; set; }
            public string TypeName { get; set; }
            public string ParameterName { get; set; }
        }

        private class RpcMethodInfo
        {
            public string MethodName { get; set; }
            public string HandlerTypeName { get; set; }
            public byte Opcode { get; set; }
            public byte ResponseOpcode { get; set; }
            public string ResponseType { get; set; }
            public bool HasResponse { get; set; }
            public bool IsAsync { get; set; }
            public List<RpcParameterInfo> Parameters { get; set; }
            public bool HasServices { get; set; }
        }

        private class PacketGroupInfo
        {
            public string GroupName { get; set; }
            public string SanitizedGroupName { get; set; }
            public string Namespace { get; set; }
            public List<RpcMethodInfo> Methods { get; set; }
            public List<string> HandlerTypes { get; set; }
            public Dictionary<byte, (string HandlerName, string MethodName, Location Location)> Opcodes { get; set; }
        }
    }
}
