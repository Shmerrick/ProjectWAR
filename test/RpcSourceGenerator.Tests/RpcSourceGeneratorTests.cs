using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using FrameWork.NetWork.SourceGenerators;

namespace Tests.RpcSourceGenerator;

public class RpcSourceGeneratorTests
{
    private static readonly string ClientBaseCode = @"
namespace FrameWork.NetWork.V4
{
    public abstract class Client
    {
        protected abstract void ProcessPacket(byte opcode, ReadOnlySpan<byte> payload);
        protected void SendResponse<T>(byte opcode, T response) { }
        protected void OnUnknownOpcode(byte opcode) { }
        protected void OnHandlerError(byte opcode, Exception ex) { }
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
    public void GeneratesRpcHandler_WithSynchronousMethod_NoParameters()
    {
        var source = @"
using System;
using FrameWork.NetWork.V4;

namespace TestNamespace
{
    public partial class TestClient : Client
    {
        [Rpc(0x01)]
        public void HandlePing()
        {
        }
    }
}";

        var result = RunGenerator(source);
        
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Single(result.GeneratedTrees);
        Assert.Contains("case 0x01:", result.GeneratedTrees[0].ToString());
        Assert.Contains("HandlePing()", result.GeneratedTrees[0].ToString());
    }

    [Fact]
    public void GeneratesRpcHandler_WithSynchronousMethod_WithRequest()
    {
        var source = @"
using System;
using FrameWork.NetWork.V4;

namespace TestNamespace
{
    public class LoginRequest { }
    
    public partial class TestClient : Client
    {
        [Rpc(0x10)]
        public void HandleLogin(LoginRequest request)
        {
        }
    }
}";

        var result = RunGenerator(source);
        
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Single(result.GeneratedTrees);
        var code = result.GeneratedTrees[0].ToString();
        Assert.Contains("case 0x10:", code);
        Assert.Contains("var request = Serializer.Deserialize<TestNamespace.LoginRequest>(payload);", code);
        Assert.Contains("HandleLogin(request);", code);
    }

    [Fact]
    public void GeneratesRpcHandler_WithSynchronousMethod_WithResponse()
    {
        var source = @"
using System;
using FrameWork.NetWork.V4;

namespace TestNamespace
{
    public class LoginRequest { }
    public class LoginResponse { }
    
    public partial class TestClient : Client
    {
        [Rpc(0x10, 0x11)]
        public LoginResponse HandleLogin(LoginRequest request)
        {
            return new LoginResponse();
        }
    }
}";

        var result = RunGenerator(source);
        
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Single(result.GeneratedTrees);
        var code = result.GeneratedTrees[0].ToString();
        Assert.Contains("case 0x10:", code);
        Assert.Contains("var response = HandleLogin(request);", code);
        Assert.Contains("SendResponse(0x11, response);", code);
    }

    [Fact]
    public void GeneratesRpcHandler_WithAsyncMethod_ReturnsTask()
    {
        var source = @"
using System;
using System.Threading.Tasks;
using FrameWork.NetWork.V4;

namespace TestNamespace
{
    public class PingRequest { }
    
    public partial class TestClient : Client
    {
        [Rpc(0x01)]
        public async Task HandlePing(PingRequest request)
        {
            await Task.Delay(10);
        }
    }
}";

        var result = RunGenerator(source);
        
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Single(result.GeneratedTrees);
        var code = result.GeneratedTrees[0].ToString();
        Assert.Contains("case 0x01:", code);
        Assert.Contains("_ = HandleAsync_HandlePing(request);", code);
        Assert.Contains("private async Task HandleAsync_HandlePing(TestNamespace.PingRequest request)", code);
    }

    [Fact]
    public void GeneratesRpcHandler_WithAsyncMethod_ReturnsTaskWithResponse()
    {
        var source = @"
using System;
using System.Threading.Tasks;
using FrameWork.NetWork.V4;

namespace TestNamespace
{
    public class LoginRequest { }
    public class LoginResponse { }
    
    public partial class TestClient : Client
    {
        [Rpc(0x10, 0x11)]
        public async Task<LoginResponse> HandleLogin(LoginRequest request)
        {
            return await Task.FromResult(new LoginResponse());
        }
    }
}";

        var result = RunGenerator(source);
        
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Single(result.GeneratedTrees);
        var code = result.GeneratedTrees[0].ToString();
        Assert.Contains("case 0x10:", code);
        Assert.Contains("_ = HandleAsync_HandleLogin(request);", code);
        Assert.Contains("var response = await HandleLogin(request);", code);
        Assert.Contains("SendResponse(0x11, response);", code);
    }

    [Fact]
    public void ReportsDiagnostic_ForDuplicateOpcodes()
    {
        var source = @"
using System;
using FrameWork.NetWork.V4;

namespace TestNamespace
{
    public partial class TestClient : Client
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
        Assert.NotNull(error);
        Assert.Equal(DiagnosticSeverity.Error, error.Severity);
        Assert.Contains("0x01", error.GetMessage());
    }

    [Fact]
    public void ReportsDiagnostic_ForInvalidMethodSignature_TooManyParameters()
    {
        var source = @"
using System;
using FrameWork.NetWork.V4;

namespace TestNamespace
{
    public class Param1 { }
    public class Param2 { }
    
    public partial class TestClient : Client
    {
        [Rpc(0x01)]
        public void HandleInvalid(Param1 p1, Param2 p2)
        {
        }
    }
}";

        var result = RunGenerator(source);
        
        var error = result.Diagnostics.FirstOrDefault(d => d.Id == "RPC002");
        Assert.NotNull(error);
        Assert.Equal(DiagnosticSeverity.Error, error.Severity);
        Assert.Contains("zero or one parameter", error.GetMessage());
    }

    [Fact]
    public void IgnoresNonPartialClass()
    {
        var source = @"
using System;
using FrameWork.NetWork.V4;

namespace TestNamespace
{
    public class TestClient : Client
    {
        [Rpc(0x01)]
        public void HandlePing()
        {
        }
    }
}";

        var result = RunGenerator(source);
        
        Assert.Empty(result.GeneratedTrees);
    }

    [Fact]
    public void IgnoresClassNotInheritingFromClient()
    {
        var source = @"
using System;
using FrameWork.NetWork.V4;

namespace TestNamespace
{
    public partial class TestClient
    {
        [Rpc(0x01)]
        public void HandlePing()
        {
        }
    }
}";

        var result = RunGenerator(source);
        
        Assert.Empty(result.GeneratedTrees);
    }

    [Fact]
    public void GeneratesNothing_ForClassWithoutRpcMethods()
    {
        var source = @"
using System;
using FrameWork.NetWork.V4;

namespace TestNamespace
{
    public partial class TestClient : Client
    {
        public void RegularMethod()
        {
        }
    }
}";

        var result = RunGenerator(source);
        
        Assert.Empty(result.GeneratedTrees);
    }

    [Fact]
    public void GeneratesRpcHandler_WithMultipleMethods()
    {
        var source = @"
using System;
using System.Threading.Tasks;
using FrameWork.NetWork.V4;

namespace TestNamespace
{
    public class Request1 { }
    public class Request2 { }
    public class Response2 { }
    
    public partial class TestClient : Client
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
        
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Single(result.GeneratedTrees);
        var code = result.GeneratedTrees[0].ToString();
        
        // Check all three methods are handled
        Assert.Contains("case 0x01:", code);
        Assert.Contains("case 0x02:", code);
        Assert.Contains("case 0x03:", code);
        
        // Check proper handling
        Assert.Contains("HandleMethod1(request);", code);
        Assert.Contains("_ = HandleAsync_HandleMethod2(request);", code);
        Assert.Contains("HandleMethod3()", code);
    }

    [Fact]
    public void GeneratesRpcHandler_WithDefaultResponseOpcode()
    {
        var source = @"
using System;
using FrameWork.NetWork.V4;

namespace TestNamespace
{
    public class LoginRequest { }
    public class LoginResponse { }
    
    public partial class TestClient : Client
    {
        [Rpc(0x10)]
        public LoginResponse HandleLogin(LoginRequest request)
        {
            return new LoginResponse();
        }
    }
}";

        var result = RunGenerator(source);
        
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        var code = result.GeneratedTrees[0].ToString();
        // When only one opcode is provided, it's used for both request and response
        Assert.Contains("SendResponse(0x10, response);", code);
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

        var generator = new FrameWork.NetWork.SourceGenerators.RpcSourceGenerator();
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
