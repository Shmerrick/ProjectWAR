using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using FrameWork.NetWork.SourceGenerators;

namespace Tests.RpcSourceGenerator;

public class RpcClientGeneratorTests
{
    private static readonly string ClientBaseCode = @"
namespace FrameWork.NetWork.V4
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
using FrameWork.NetWork.V4;

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
        
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Single(result.GeneratedTrees);
        var code = result.GeneratedTrees[0].ToString();
        
        Assert.Contains("public partial Task<TestNamespace.LoginResponse> Login(TestNamespace.LoginRequest request)", code);
        Assert.Contains("return SendRequestAsync<TestNamespace.LoginRequest, TestNamespace.LoginResponse>(0x10, 0x11, request);", code);
    }

    [Fact]
    public void GeneratesClientMethod_FireAndForget_WithRequest()
    {
        var source = @"
using System.Threading.Tasks;
using FrameWork.NetWork.V4;

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
        
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        var code = result.GeneratedTrees[0].ToString();
        
        Assert.Contains("public partial Task Ping(TestNamespace.PingRequest request)", code);
        Assert.Contains("SendRequest(0x01, request);", code);
        Assert.Contains("return Task.CompletedTask;", code);
    }

    [Fact]
    public void GeneratesClientMethod_FireAndForget_NoRequest()
    {
        var source = @"
using System.Threading.Tasks;
using FrameWork.NetWork.V4;

namespace TestNamespace
{
    public partial class TestClient : Client
    {
        [Rpc(0x01)]
        public partial Task Ping();
    }
}";

        var result = RunGenerator(source);
        
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        var code = result.GeneratedTrees[0].ToString();
        
        Assert.Contains("public partial Task Ping()", code);
        Assert.Contains("SendRequest(0x01, new object());", code);
        Assert.Contains("return Task.CompletedTask;", code);
    }

    [Fact]
    public void GeneratesClientMethod_Synchronous_WithResponse()
    {
        var source = @"
using FrameWork.NetWork.V4;

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
        
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        var code = result.GeneratedTrees[0].ToString();
        
        Assert.Contains("public partial TestNamespace.LoginResponse Login(TestNamespace.LoginRequest request)", code);
        Assert.Contains("return SendRequest<TestNamespace.LoginRequest, TestNamespace.LoginResponse>(0x10, 0x11, request);", code);
    }

    [Fact]
    public void GeneratesClientMethod_Synchronous_FireAndForget()
    {
        var source = @"
using FrameWork.NetWork.V4;

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
        
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        var code = result.GeneratedTrees[0].ToString();
        
        Assert.Contains("public partial void Notify(TestNamespace.NotifyRequest request)", code);
        Assert.Contains("SendRequest(0x20, request);", code);
        Assert.DoesNotContain("return", code);
    }

    [Fact]
    public void RespectsAccessibilityModifiers()
    {
        var source = @"
using System.Threading.Tasks;
using FrameWork.NetWork.V4;

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
        
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        var code = result.GeneratedTrees[0].ToString();
        
        Assert.Contains("public partial Task<TestNamespace.Response> PublicMethod", code);
        Assert.Contains("internal partial Task<TestNamespace.Response> InternalMethod", code);
        Assert.Contains("protected partial Task<TestNamespace.Response> ProtectedMethod", code);
        Assert.Contains("private partial Task<TestNamespace.Response> PrivateMethod", code);
    }

    [Fact]
    public void IgnoresNonPartialMethods()
    {
        var source = @"
using System.Threading.Tasks;
using FrameWork.NetWork.V4;

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
        Assert.Empty(result.GeneratedTrees);
    }

    [Fact]
    public void IgnoresClassNotInheritingFromClient()
    {
        var source = @"
using System.Threading.Tasks;
using FrameWork.NetWork.V4;

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
        
        Assert.Empty(result.GeneratedTrees);
    }

    [Fact]
    public void GeneratesMultipleMethods()
    {
        var source = @"
using System.Threading.Tasks;
using FrameWork.NetWork.V4;

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
        
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Single(result.GeneratedTrees);
        var code = result.GeneratedTrees[0].ToString();
        
        Assert.Contains("Method1", code);
        Assert.Contains("Method2", code);
        Assert.Contains("Method3", code);
        Assert.Contains("0x10", code);
        Assert.Contains("0x20", code);
        Assert.Contains("0x30", code);
    }

    [Fact]
    public void HandlesDefaultResponseOpcode()
    {
        var source = @"
using System.Threading.Tasks;
using FrameWork.NetWork.V4;

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
        
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        var code = result.GeneratedTrees[0].ToString();
        
        // When response opcode is not specified but method has return value,
        // the request opcode should be used for both
        Assert.Contains("SendRequestAsync<TestNamespace.Request, TestNamespace.Response>(0x10, 0x10, request)", code);
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

        var generator = new FrameWork.NetWork.SourceGenerators.RpcClientGenerator();
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
