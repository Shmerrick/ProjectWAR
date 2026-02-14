using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RpcSourceGenerator;
using Shouldly;

namespace Core.Infrastructure.Network.RpcSourceGenerator.Tests;

public class RpcClientGeneratorTests
{
    private static readonly string ClientBaseCode = @"
namespace Core.Infrastructure.Network
{
    public abstract class Client
    {
        protected void SendRequest<T>(byte opcode, T request) { }
        protected TResponse SendRequest<TRequest, TResponse>(byte requestOpcode, byte responseOpcode, TRequest request) { return default!; }
        protected System.Threading.Tasks.Task SendRequestAsync<T>(byte opcode, T request) { return System.Threading.Tasks.Task.CompletedTask; }
        protected System.Threading.Tasks.Task<TResponse> SendRequestAsync<TRequest, TResponse>(byte requestOpcode, byte responseOpcode, TRequest request) { return System.Threading.Tasks.Task.FromResult<TResponse>(default!); }
    }
    
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class RpcAttribute : System.Attribute
    {
        public byte Opcode { get; }
        public byte? ResponseOpcode { get; }
        public RpcAttribute(byte opcode) { Opcode = opcode; }
        public RpcAttribute(byte opcode, byte responseOpcode) { Opcode = opcode; ResponseOpcode = responseOpcode; }
    }
}";

    [Fact]
    public void GeneratesClientMethod_WithRequestAndResponse_Async()
    {
        var source = @"
using System.Threading.Tasks;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public class LoginRequest { }
    public class LoginResponse { }
    
    public partial class TestClient : Client
    {
        [Rpc(0x10, 0x11)]
        public partial Task<LoginResponse> Login(LoginRequest request);
    }
}";

        var result = RunGenerator(source);
        
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        result.GeneratedTrees.ShouldHaveSingleItem();
        var code = result.GeneratedTrees[0].ToString();
        
        code.ShouldContain("public partial Task<TestNamespace.LoginResponse> Login(TestNamespace.LoginRequest request)");
        code.ShouldContain("return SendRequestAsync<TestNamespace.LoginRequest, TestNamespace.LoginResponse>(0x10, 0x11, request);");
    }

    [Fact]
    public void GeneratesClientMethod_FireAndForget_WithRequest()
    {
        var source = @"
using System.Threading.Tasks;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public class PingRequest { }
    
    public partial class TestClient : Client
    {
        [Rpc(0x01)]
        public partial Task Ping(TestNamespace.PingRequest request);
    }
}";

        var result = RunGenerator(source);
        
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var code = result.GeneratedTrees[0].ToString();
        
        code.ShouldContain("public partial Task Ping(TestNamespace.PingRequest request)");
        code.ShouldContain("SendRequest(0x01, request);");
        code.ShouldContain("return Task.CompletedTask;");
    }

    [Fact]
    public void GeneratesClientMethod_FireAndForget_NoRequest()
    {
        var source = @"
using System.Threading.Tasks;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public partial class TestClient : Client
    {
        [Rpc(0x01)]
        public partial Task Ping();
    }
}";

        var result = RunGenerator(source);
        
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var code = result.GeneratedTrees[0].ToString();
        
        code.ShouldContain("public partial Task Ping()");
        code.ShouldContain("SendRequest(0x01, new object());");
        code.ShouldContain("return Task.CompletedTask;");
    }

    [Fact]
    public void GeneratesClientMethod_Synchronous_WithResponse()
    {
        var source = @"
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public class LoginRequest { }
    public class LoginResponse { }
    
    public partial class TestClient : Client
    {
        [Rpc(0x10, 0x11)]
        public partial TestNamespace.LoginResponse Login(TestNamespace.LoginRequest request);
    }
}";

        var result = RunGenerator(source);
        
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var code = result.GeneratedTrees[0].ToString();
        
        code.ShouldContain("public partial TestNamespace.LoginResponse Login(TestNamespace.LoginRequest request)");
        code.ShouldContain("return SendRequest<TestNamespace.LoginRequest, TestNamespace.LoginResponse>(0x10, 0x11, request);");
    }

    [Fact]
    public void GeneratesClientMethod_Synchronous_FireAndForget()
    {
        var source = @"
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public class NotifyRequest { }
    
    public partial class TestClient : Client
    {
        [Rpc(0x20)]
        public partial void Notify(TestNamespace.NotifyRequest request);
    }
}";

        var result = RunGenerator(source);
        
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var code = result.GeneratedTrees[0].ToString();
        
        code.ShouldContain("public partial void Notify(TestNamespace.NotifyRequest request)");
        code.ShouldContain("SendRequest(0x20, request);");
        Assert.DoesNotContain("return", code);
    }

    [Fact]
    public void RespectsAccessibilityModifiers()
    {
        var source = @"
using System.Threading.Tasks;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public class Request { }
    public class Response { }
    
    public partial class TestClient : Client
    {
        [Rpc(0x10, 0x11)]
        public partial Task<TestNamespace.Response> PublicMethod(Request request);
        
        [Rpc(0x20, 0x21)]
        internal partial Task<TestNamespace.Response> InternalMethod(Request request);
        
        [Rpc(0x30, 0x31)]
        protected partial Task<TestNamespace.Response> ProtectedMethod(Request request);
        
        [Rpc(0x40, 0x41)]
        private partial Task<TestNamespace.Response> PrivateMethod(Request request);
    }
}";

        var result = RunGenerator(source);
        
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var code = result.GeneratedTrees[0].ToString();
        
        code.ShouldContain("public partial Task<TestNamespace.Response> PublicMethod");
        code.ShouldContain("internal partial Task<TestNamespace.Response> InternalMethod");
        code.ShouldContain("protected partial Task<TestNamespace.Response> ProtectedMethod");
        code.ShouldContain("private partial Task<TestNamespace.Response> PrivateMethod");
    }

    [Fact]
    public void IgnoresNonPartialMethods()
    {
        var source = @"
using System.Threading.Tasks;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public class Request { }
    
    public partial class TestClient : Client
    {
        [Rpc(0x01)]
        public Task Method(Request request)
        {
            return Task.CompletedTask;
        }
    }
}";

        var result = RunGenerator(source);
        
        // Should generate nothing since method is not partial
        result.GeneratedTrees.ShouldBeEmpty();
    }

    [Fact]
    public void IgnoresClassNotInheritingFromClient()
    {
        var source = @"
using System.Threading.Tasks;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public class Request { }
    
    public partial class TestClient
    {
        [Rpc(0x01)]
        public partial Task Method(Request request);
    }
}";

        var result = RunGenerator(source);
        
        result.GeneratedTrees.ShouldBeEmpty();
    }

    [Fact]
    public void GeneratesMultipleMethods()
    {
        var source = @"
using System.Threading.Tasks;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public class Request1 { }
    public class Response1 { }
    public class Request2 { }
    
    public partial class TestClient : Client
    {
        [Rpc(0x10, 0x11)]
        public partial Task<Response1> Method1(Request1 request);
        
        [Rpc(0x20)]
        public partial Task Method2(Request2 request);
        
        [Rpc(0x30)]
        public partial Task Method3();
    }
}";

        var result = RunGenerator(source);
        
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        result.GeneratedTrees.ShouldHaveSingleItem();
        var code = result.GeneratedTrees[0].ToString();
        
        code.ShouldContain("Method1");
        code.ShouldContain("Method2");
        code.ShouldContain("Method3");
        code.ShouldContain("0x10");
        code.ShouldContain("0x20");
        code.ShouldContain("0x30");
    }

    [Fact]
    public void HandlesDefaultResponseOpcode()
    {
        var source = @"
using System.Threading.Tasks;
using Core.Infrastructure.Network;

namespace TestNamespace
{
    public class Request { }
    public class Response { }
    
    public partial class TestClient : Client
    {
        [Rpc(0x10)]
        public partial Task<Response> Method(Request request);
    }
}";

        var result = RunGenerator(source);
        
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var code = result.GeneratedTrees[0].ToString();
        
        // When response opcode is not specified but method has return value,
        // the request opcode should be used for both
        code.ShouldContain("SendRequestAsync<TestNamespace.Request, TestNamespace.Response>(0x10, 0x10, request)");
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
            new[] { syntaxTree, CSharpSyntaxTree.ParseText(ClientBaseCode) },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new RpcClientGenerator();
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
