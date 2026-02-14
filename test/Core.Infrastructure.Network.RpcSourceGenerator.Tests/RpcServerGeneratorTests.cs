using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;

namespace Core.Infrastructure.Network.RpcSourceGenerator.Tests;

public class RpcServerGeneratorTests
{
    private static readonly string HandlerBaseCode = @"
namespace Core.Infrastructure.Network
{
    public interface IPacketHandler { }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class RpcAttribute : System.Attribute
    {
        public byte Opcode { get; }
        public byte? ResponseOpcode { get; }
        public RpcAttribute(byte opcode) { Opcode = opcode; }
        public RpcAttribute(byte opcode, byte responseOpcode) { Opcode = opcode; ResponseOpcode = responseOpcode; }
    }

    [System.AttributeUsage(System.AttributeTargets.Parameter)]
    public class FromServicesAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
    public class PacketGroupAttribute : System.Attribute
    {
        public string GroupName { get; }
        public PacketGroupAttribute(string groupName = ""Default"") { GroupName = groupName; }
    }

    public interface IPacketSerializer
    {
        T Deserialize<T>(System.ReadOnlySpan<byte> data);
        void Serialize<T>(System.Buffers.IBufferWriter<byte> writer, T value);
    }

    public interface IConnectionContext
    {
        string RemoteAddress { get; }
        void SendResponse<T>(byte opcode, T response);
        void Disconnect(object reason);
        System.Collections.Generic.IDictionary<string, object> Items { get; }
        void OnDispatchError(byte opcode, System.Exception exception);
    }

    public interface IPacketDispatcher
    {
        void Dispatch(byte opcode, System.ReadOnlyMemory<byte> payload,
            System.IServiceProvider services, IPacketSerializer serializer, IConnectionContext connection);
    }
}";

    [Fact]
    public void GeneratesDispatcher_WithSynchronousMethod_NoParameters()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public partial class TestHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x01)]
        public void HandlePing()
        {
        }
    }
}";

        var result = RunGenerator(source);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        result.GeneratedTrees.ShouldHaveSingleItem();
        var code = result.GeneratedTrees[0].ToString();
        code.ShouldContain("case 0x01:");
        code.ShouldContain("handler.HandlePing()");
        code.ShouldContain("class DefaultPacketDispatcher : IPacketDispatcher");
    }

    [Fact]
    public void GeneratesDispatcher_WithSynchronousMethod_WithRequest()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public class LoginRequest { }

    public partial class TestHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x10)]
        public void HandleLogin(LoginRequest request)
        {
        }
    }
}";

        var result = RunGenerator(source);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        result.GeneratedTrees.ShouldHaveSingleItem();
        var code = result.GeneratedTrees[0].ToString();
        code.ShouldContain("case 0x10:");
        code.ShouldContain("serializer.Deserialize<TestNamespace.LoginRequest>(payload.Span)");
        code.ShouldContain("handler.HandleLogin(request)");
    }

    [Fact]
    public void GeneratesDispatcher_WithSynchronousMethod_WithResponse()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public class LoginRequest { }
    public class LoginResponse { }

    public partial class TestHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x10, 0x11)]
        public LoginResponse HandleLogin(LoginRequest request)
        {
            return new LoginResponse();
        }
    }
}";

        var result = RunGenerator(source);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        result.GeneratedTrees.ShouldHaveSingleItem();
        var code = result.GeneratedTrees[0].ToString();
        code.ShouldContain("case 0x10:");
        code.ShouldContain("var response = handler.HandleLogin(request);");
        code.ShouldContain("connection.SendResponse(0x11, response)");
    }

    [Fact]
    public void GeneratesDispatcher_WithAsyncMethod_ReturnsTask()
    {
        var source = @"
using System;
using System.Threading.Tasks;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public class PingRequest { }

    public partial class TestHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x01)]
        public async Task HandlePing(PingRequest request)
        {
            await Task.Delay(10);
        }
    }
}";

        var result = RunGenerator(source);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        result.GeneratedTrees.ShouldHaveSingleItem();
        var code = result.GeneratedTrees[0].ToString();
        code.ShouldContain("case 0x01:");
        code.ShouldContain("_ = DispatchAsync_HandlePing(handler, request, connection);");
        code.ShouldContain("private static async Task DispatchAsync_HandlePing(");
    }

    [Fact]
    public void GeneratesDispatcher_WithAsyncMethod_ReturnsTaskWithResponse()
    {
        var source = @"
using System;
using System.Threading.Tasks;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public class LoginRequest { }
    public class LoginResponse { }

    public partial class TestHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x10, 0x11)]
        public async Task<LoginResponse> HandleLogin(LoginRequest request)
        {
            return await Task.FromResult(new LoginResponse());
        }
    }
}";

        var result = RunGenerator(source);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        result.GeneratedTrees.ShouldHaveSingleItem();
        var code = result.GeneratedTrees[0].ToString();
        code.ShouldContain("case 0x10:");
        code.ShouldContain("_ = DispatchAsync_HandleLogin(handler, request, connection);");
        code.ShouldContain("var response = await handler.HandleLogin(request);");
        code.ShouldContain("connection.SendResponse(0x11, response)");
    }

    [Fact]
    public void GeneratesDispatcher_WithFromServicesParameter()
    {
        var source = @"
using System;
using System.Threading.Tasks;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public class MyService { }
    public class LoginResponse { }

    public partial class TestHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x10, 0x11)]
        public async Task<LoginResponse> HandleLogin([FromServices] MyService svc)
        {
            return await Task.FromResult(new LoginResponse());
        }
    }
}";

        var result = RunGenerator(source);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var code = result.GeneratedTrees[0].ToString();
        code.ShouldContain("var __scope = services.CreateScope();");
        code.ShouldContain("GetRequiredService<TestNamespace.MyService>()");
        code.ShouldContain("__scope.Dispose()");
    }

    [Fact]
    public void GeneratesDispatcher_WithConnectionContextParameter()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public partial class TestHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x01)]
        public void HandlePing(IConnectionContext context)
        {
        }
    }
}";

        var result = RunGenerator(source);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var code = result.GeneratedTrees[0].ToString();
        code.ShouldContain("handler.HandlePing(connection)");
    }

    [Fact]
    public void GeneratesDispatcher_WithMixedParameters()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public class LoginRequest { }
    public class LoginResponse { }
    public class MyService { }

    public partial class TestHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x10, 0x11)]
        public LoginResponse HandleLogin(LoginRequest request, IConnectionContext context, [FromServices] MyService svc)
        {
            return new LoginResponse();
        }
    }
}";

        var result = RunGenerator(source);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var code = result.GeneratedTrees[0].ToString();
        code.ShouldContain("serializer.Deserialize<TestNamespace.LoginRequest>(payload.Span)");
        code.ShouldContain("using var __scope = services.CreateScope();");
        code.ShouldContain("GetRequiredService<TestNamespace.MyService>()");
        code.ShouldContain("handler.HandleLogin(request, connection, __svc_svc)");
    }

    [Fact]
    public void ReportsDiagnostic_ForDuplicateOpcodes_WithinSameHandler()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public partial class TestHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x01)]
        public void HandlePing()
        {
        }

        [Rpc(0x01)]
        public void HandlePing2()
        {
        }
    }
}";

        var result = RunGenerator(source);

        var error = result.Diagnostics.FirstOrDefault(d => d.Id == "RPC001");
        error.ShouldNotBeNull();
        error.Severity.ShouldBe(DiagnosticSeverity.Error);
        Assert.Contains("0x01", error.GetMessage());
    }

    [Fact]
    public void ReportsDiagnostic_ForDuplicateOpcodes_AcrossHandlers()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public partial class HandlerA : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x01)]
        public void HandlePing()
        {
        }
    }

    public partial class HandlerB : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x01)]
        public void HandlePing2()
        {
        }
    }
}";

        var result = RunGenerator(source);

        var error = result.Diagnostics.FirstOrDefault(d => d.Id == "RPC001");
        error.ShouldNotBeNull();
        error.Severity.ShouldBe(DiagnosticSeverity.Error);
        Assert.Contains("0x01", error.GetMessage());
        Assert.Contains("HandlerA.HandlePing", error.GetMessage());
    }

    [Fact]
    public void ReportsDiagnostic_ForMultipleRequestParameters()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public class Param1 { }
    public class Param2 { }

    public partial class TestHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x01)]
        public void HandleInvalid(Param1 p1, Param2 p2)
        {
        }
    }
}";

        var result = RunGenerator(source);

        var error = result.Diagnostics.FirstOrDefault(d => d.Id == "RPC002");
        error.ShouldNotBeNull();
        error.Severity.ShouldBe(DiagnosticSeverity.Error);
    }

    [Fact]
    public void IncludesNonPartialHandlerClass()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public class TestHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x01)]
        public void HandlePing()
        {
        }
    }
}";

        var result = RunGenerator(source);
        result.GeneratedTrees.ShouldHaveSingleItem();
        var code = result.GeneratedTrees[0].ToString();
        code.ShouldContain("case 0x01:");
    }

    [Fact]
    public void IgnoresClassNotInheritingFromPacketHandler()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public partial class TestHandler
    {
        [Rpc(0x01)]
        public void HandlePing()
        {
        }
    }
}";

        var result = RunGenerator(source);
        result.GeneratedTrees.ShouldBeEmpty();
    }

    [Fact]
    public void GeneratesNothing_ForClassWithoutRpcMethods()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public partial class TestHandler : Core.Infrastructure.Network.IPacketHandler
    {
        public void RegularMethod()
        {
        }
    }
}";

        var result = RunGenerator(source);
        result.GeneratedTrees.ShouldBeEmpty();
    }

    [Fact]
    public void GeneratesDispatcher_WithMultipleMethods()
    {
        var source = @"
using System;
using System.Threading.Tasks;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public class Request1 { }
    public class Request2 { }
    public class Response2 { }

    public partial class TestHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x01)]
        public void HandleMethod1(Request1 request)
        {
        }

        [Rpc(0x02)]
        public async Task<Response2> HandleMethod2(Request2 request)
        {
            return await Task.FromResult(new Response2());
        }

        [Rpc(0x03)]
        public void HandleMethod3()
        {
        }
    }
}";

        var result = RunGenerator(source);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        result.GeneratedTrees.ShouldHaveSingleItem();
        var code = result.GeneratedTrees[0].ToString();

        code.ShouldContain("case 0x01:");
        code.ShouldContain("case 0x02:");
        code.ShouldContain("case 0x03:");
        code.ShouldContain("handler.HandleMethod1(request)");
        code.ShouldContain("_ = DispatchAsync_HandleMethod2(handler, request, connection)");
        code.ShouldContain("handler.HandleMethod3()");
    }

    [Fact]
    public void GeneratesDispatcher_WithDefaultResponseOpcode()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public class LoginRequest { }
    public class LoginResponse { }

    public partial class TestHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x10)]
        public LoginResponse HandleLogin(LoginRequest request)
        {
            return new LoginResponse();
        }
    }
}";

        var result = RunGenerator(source);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var code = result.GeneratedTrees[0].ToString();
        code.ShouldContain("connection.SendResponse(0x10, response)");
    }

    [Fact]
    public void AllowsMultipleFromServicesParameters()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public class Svc1 { }
    public class Svc2 { }

    public partial class TestHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x01)]
        public void Handle([FromServices] Svc1 svc1, [FromServices] Svc2 svc2)
        {
        }
    }
}";

        var result = RunGenerator(source);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var code = result.GeneratedTrees[0].ToString();
        code.ShouldContain("GetRequiredService<TestNamespace.Svc1>()");
        code.ShouldContain("GetRequiredService<TestNamespace.Svc2>()");
        code.ShouldContain("handler.Handle(__svc_svc1, __svc_svc2)");
    }

    [Fact]
    public void AsyncWithServices_CreatesNonDisposableScope_AndDisposesInFinally()
    {
        var source = @"
using System;
using System.Threading.Tasks;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public class MyService { }

    public partial class TestHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x01)]
        public async Task HandleAsync([FromServices] MyService svc)
        {
            await Task.Delay(10);
        }
    }
}";

        var result = RunGenerator(source);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var code = result.GeneratedTrees[0].ToString();
        // Async handler should NOT use 'using' (scope passed to wrapper)
        code.ShouldContain("var __scope = services.CreateScope();");
        Assert.DoesNotContain("using var __scope", code);
        // Wrapper should dispose scope in finally
        code.ShouldContain("__scope.Dispose()");
    }

    [Fact]
    public void GeneratesServiceCollectionExtensionMethod()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public partial class TestHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x01)]
        public void HandlePing()
        {
        }
    }
}";

        var result = RunGenerator(source);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var code = result.GeneratedTrees[0].ToString();
        code.ShouldContain("AddDefaultPacketHandlers(this IServerNetworkingBuilder services)");
        code.ShouldContain("services.WithPacketDispatcher<TestNamespace.DefaultPacketDispatcher>()");
        code.ShouldContain("services.AddHandler<TestNamespace.TestHandler>()");
    }

    [Fact]
    public void GeneratesNamedPacketGroup()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    [PacketGroup(""Chat"")]
    public partial class ChatHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x01)]
        public void HandleChat()
        {
        }
    }
}";

        var result = RunGenerator(source);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var code = result.GeneratedTrees[0].ToString();
        code.ShouldContain("class ChatPacketDispatcher : IPacketDispatcher");
        code.ShouldContain("AddChatPacketHandlers(this IServerNetworkingBuilder services)");
        code.ShouldContain("services.WithPacketDispatcher<TestNamespace.ChatPacketDispatcher>()");
        code.ShouldContain("services.AddHandler<TestNamespace.ChatHandler>()");
    }

    [Fact]
    public void GeneratesDispatcher_WithMultipleHandlersInSameGroup()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public class Request1 { }
    public class Request2 { }

    public partial class HandlerA : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x01)]
        public void Handle1(Request1 request)
        {
        }
    }

    public partial class HandlerB : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x02)]
        public void Handle2(Request2 request)
        {
        }
    }
}";

        var result = RunGenerator(source);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        result.GeneratedTrees.ShouldHaveSingleItem();
        var code = result.GeneratedTrees[0].ToString();

        // Both opcodes in single dispatcher
        code.ShouldContain("case 0x01:");
        code.ShouldContain("case 0x02:");
        // Handler resolution from services
        code.ShouldContain("services.GetRequiredService<TestNamespace.HandlerA>()");
        code.ShouldContain("services.GetRequiredService<TestNamespace.HandlerB>()");
        // Both handlers registered
        code.ShouldContain("services.AddHandler<TestNamespace.HandlerA>()");
        code.ShouldContain("services.AddHandler<TestNamespace.HandlerB>()");
    }

    [Fact]
    public void ResolvesHandlerFromServices_PerOpcode()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public partial class TestHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x01)]
        public void HandlePing()
        {
        }
    }
}";

        var result = RunGenerator(source);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var code = result.GeneratedTrees[0].ToString();
        code.ShouldContain("var handler = services.GetRequiredService<TestNamespace.TestHandler>();");
    }

    // ──────────────────────────────────────────────
    // Additional edge-case tests
    // ──────────────────────────────────────────────

    [Fact]
    public void GeneratesDispatcher_HandlersInDifferentNamespaces_SameGroup()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace Alpha
{
    public class AlphaRequest { }

    public partial class AlphaHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x01)]
        public void Handle(AlphaRequest req) { }
    }
}

namespace Beta
{
    public class BetaRequest { }

    public partial class BetaHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x02)]
        public void Handle(BetaRequest req) { }
    }
}";

        var result = RunGenerator(source);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        result.GeneratedTrees.ShouldHaveSingleItem();
        var code = result.GeneratedTrees[0].ToString();
        code.ShouldContain("case 0x01:");
        code.ShouldContain("case 0x02:");
        code.ShouldContain("services.AddHandler<Alpha.AlphaHandler>()");
        code.ShouldContain("services.AddHandler<Beta.BetaHandler>()");
    }

    [Fact]
    public void GeneratesDispatcher_BoundaryOpcodeValues()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public partial class TestHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x00)]
        public void HandleMin() { }

        [Rpc(0xFF)]
        public void HandleMax() { }
    }
}";

        var result = RunGenerator(source);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var code = result.GeneratedTrees[0].ToString();
        code.ShouldContain("case 0x00:");
        code.ShouldContain("case 0xFF:");
    }

    [Fact]
    public void GeneratesSeparateDispatchers_ForDifferentGroups()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    [PacketGroup(""Auth"")]
    public partial class AuthHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x01)]
        public void HandleAuth() { }
    }

    [PacketGroup(""Game"")]
    public partial class GameHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x01)]
        public void HandleGame() { }
    }
}";

        var result = RunGenerator(source);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        // Two groups should produce two generated files
        result.GeneratedTrees.Length.ShouldBe(2);

        var allCode = string.Join("\n", result.GeneratedTrees.Select(t => t.ToString()));
        allCode.ShouldContain("class AuthPacketDispatcher : IPacketDispatcher");
        allCode.ShouldContain("class GamePacketDispatcher : IPacketDispatcher");
        allCode.ShouldContain("AddAuthPacketHandlers(this IServerNetworkingBuilder services)");
        allCode.ShouldContain("AddGamePacketHandlers(this IServerNetworkingBuilder services)");
    }

    [Fact]
    public void DuplicateOpcodes_AcrossDifferentGroups_DoesNotReportError()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    [PacketGroup(""Auth"")]
    public partial class AuthHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x01)]
        public void Handle() { }
    }

    [PacketGroup(""Game"")]
    public partial class GameHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x01)]
        public void Handle() { }
    }
}";

        var result = RunGenerator(source);

        // Same opcode in different groups is OK
        result.Diagnostics.Where(d => d.Id == "RPC001").ShouldBeEmpty();
        result.GeneratedTrees.Length.ShouldBe(2);
    }

    [Fact]
    public void GeneratesDispatcher_HandlerWithNoNamespace()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

public partial class GlobalHandler : Core.Infrastructure.Network.IPacketHandler
{
    [Rpc(0x01)]
    public void HandlePing() { }
}";

        var result = RunGenerator(source);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        result.GeneratedTrees.ShouldHaveSingleItem();
        var code = result.GeneratedTrees[0].ToString();
        code.ShouldContain("case 0x01:");
        code.ShouldContain("class DefaultPacketDispatcher : IPacketDispatcher");
    }

    [Fact]
    public void GeneratesDispatcher_OnlyRequestParam_NoResponse_NoServices()
    {
        // Simplest fire-and-forget RPC with just a request
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public class HeartbeatRequest { }

    public partial class TestHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x99)]
        public void HandleHeartbeat(HeartbeatRequest request) { }
    }
}";

        var result = RunGenerator(source);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var code = result.GeneratedTrees[0].ToString();
        code.ShouldContain("case 0x99:");
        code.ShouldContain("serializer.Deserialize<TestNamespace.HeartbeatRequest>(payload.Span)");
        code.ShouldContain("handler.HandleHeartbeat(request)");
        // Should NOT contain scope creation (no services)
        code.ShouldNotContain("CreateScope");
        // Should NOT contain response sending
        code.ShouldNotContain("SendResponse");
    }

    [Fact]
    public void GeneratesDispatcher_ConnectionContextOnly_NoRequestNoResponse()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public partial class TestHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x05)]
        public void HandleDisconnect(IConnectionContext connection) { }
    }
}";

        var result = RunGenerator(source);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var code = result.GeneratedTrees[0].ToString();
        code.ShouldContain("handler.HandleDisconnect(connection)");
        // No deserialization needed
        code.ShouldNotContain("Deserialize");
    }

    [Fact]
    public void ServiceCollectionExtension_RegistersMultipleHandlers()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public partial class HandlerA : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x01)]
        public void Handle1() { }
    }

    public partial class HandlerB : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x02)]
        public void Handle2() { }
    }

    public partial class HandlerC : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x03)]
        public void Handle3() { }
    }
}";

        var result = RunGenerator(source);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var code = result.GeneratedTrees[0].ToString();
        // All three handlers should be registered in the DI extension
        code.ShouldContain("services.AddHandler<TestNamespace.HandlerA>()");
        code.ShouldContain("services.AddHandler<TestNamespace.HandlerB>()");
        code.ShouldContain("services.AddHandler<TestNamespace.HandlerC>()");
        code.ShouldContain("services.WithPacketDispatcher<TestNamespace.DefaultPacketDispatcher>()");
    }

    [Fact]
    public void SanitizesGroupNamesWithSpaces()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    [PacketGroup(""My Group"")]
    public partial class MyHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x01)]
        public void HandleMessage()
        {
        }
    }
}";

        var result = RunGenerator(source);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var code = result.GeneratedTrees[0].ToString();
        // Group name "My Group" should be sanitized to "MyGroup"
        code.ShouldContain("class MyGroupPacketDispatcher : IPacketDispatcher");
        code.ShouldContain("AddMyGroupPacketHandlers(this IServerNetworkingBuilder services)");
    }

    [Fact]
    public void SanitizesGroupNamesWithPunctuation()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    [PacketGroup(""Auth-2.0"")]
    public partial class AuthHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x01)]
        public void HandleAuth()
        {
        }
    }
}";

        var result = RunGenerator(source);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var code = result.GeneratedTrees[0].ToString();
        // Group name "Auth-2.0" should be sanitized to "Auth20"
        code.ShouldContain("class Auth20PacketDispatcher : IPacketDispatcher");
        code.ShouldContain("AddAuth20PacketHandlers(this IServerNetworkingBuilder services)");
    }

    [Fact]
    public void SanitizesGroupNamesWithCSharpKeywords()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    [PacketGroup(""class"")]
    public partial class ClassHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x01)]
        public void HandleClass()
        {
        }
    }
}";

        var result = RunGenerator(source);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var code = result.GeneratedTrees[0].ToString();
        // Group name "class" (keyword) should be sanitized to "class_"
        code.ShouldContain("class class_PacketDispatcher : IPacketDispatcher");
        code.ShouldContain("Addclass_PacketHandlers(this IServerNetworkingBuilder services)");
    }

    [Fact]
    public void SanitizesGroupNamesStartingWithDigit()
    {
        var source = @"
using System;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    [PacketGroup(""123Game"")]
    public partial class GameHandler : Core.Infrastructure.Network.IPacketHandler
    {
        [Rpc(0x01)]
        public void HandleGame()
        {
        }
    }
}";

        var result = RunGenerator(source);

        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var code = result.GeneratedTrees[0].ToString();
        // Group name "123Game" should be sanitized to "_123Game"
        code.ShouldContain("class _123GamePacketDispatcher : IPacketDispatcher");
        code.ShouldContain("Add_123GamePacketHandlers(this IServerNetworkingBuilder services)");
    }

    private GeneratorTestResult RunGenerator(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToList();

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree, CSharpSyntaxTree.ParseText(HandlerBaseCode) },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new global::RpcSourceGenerator.RpcServerGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)driver.RunGenerators(compilation);

        var result = driver.GetRunResult();
        return new GeneratorTestResult
        {
            Diagnostics = result.Diagnostics,
            GeneratedTrees = result.Results[0].GeneratedSources.Select(s => s.SyntaxTree).ToArray()
        };
    }

    private class GeneratorTestResult
    {
        public ImmutableArray<Diagnostic> Diagnostics { get; init; }
        public SyntaxTree[] GeneratedTrees { get; init; } = Array.Empty<SyntaxTree>();
    }
}
