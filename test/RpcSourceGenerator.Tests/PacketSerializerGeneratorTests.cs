using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using FrameWork.NetWork.SourceGenerators;

namespace Tests.RpcSourceGenerator;

public class PacketSerializerGeneratorTests
{
    private static readonly string AttributeCode = @"
namespace FrameWork.NetWork.V4
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class PacketSerializerContextAttribute : System.Attribute
    {
        public PacketSerializerContextAttribute(params System.Type[] types) { }
    }
    
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class PacketLengthAttribute : System.Attribute
    {
        public int ByteCount { get; }
        public PacketLengthAttribute(int byteCount) { ByteCount = byteCount; }
    }
}";

    [Fact]
    public void GeneratesSerializer_ForSimpleType()
    {
        var source = @"
using FrameWork.NetWork.V4;

namespace TestNamespace
{
    public class SimpleMessage
    {
        public int Value { get; set; }
        public string Name { get; set; }
    }

    [PacketSerializerContext(typeof(SimpleMessage))]
    public partial class TestContext
    {
    }
}";

        var result = RunGenerator(source);
        
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Single(result.GeneratedTrees);
        var code = result.GeneratedTrees[0].ToString();
        
        Assert.Contains("partial class TestContext", code);
        Assert.Contains("IPacketSerializerContext", code);
        Assert.Contains("TrySerialize", code);
        Assert.Contains("TryDeserialize", code);
    }

    [Fact]
    public void GeneratesSerializer_ForNestedTypes()
    {
        var source = @"
using FrameWork.NetWork.V4;

namespace TestNamespace
{
    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
    }

    public class Person
    {
        public string Name { get; set; }
        public Address HomeAddress { get; set; }
    }

    [PacketSerializerContext(typeof(Person))]
    public partial class TestContext
    {
    }
}";

        var result = RunGenerator(source);
        
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Single(result.GeneratedTrees);
        var code = result.GeneratedTrees[0].ToString();
        
        // Should generate serializers for both Person and Address
        Assert.Contains("TrySerialize", code);
        Assert.Contains("TryDeserialize", code);
        Assert.Contains("partial class TestContext", code);
    }

    [Fact]
    public void GeneratesSerializer_WithCollections()
    {
        var source = @"
using System.Collections.Generic;
using FrameWork.NetWork.V4;

namespace TestNamespace
{
    public class Item
    {
        public int Id { get; set; }
    }

    public class Inventory
    {
        public List<Item> Items { get; set; }
    }

    [PacketSerializerContext(typeof(Inventory))]
    public partial class TestContext
    {
    }
}";

        var result = RunGenerator(source);
        
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Single(result.GeneratedTrees);
        var code = result.GeneratedTrees[0].ToString();
        
        
        
        
        
    }

    [Fact]
    public void GeneratesSerializer_WithNullableProperties()
    {
        var source = @"
using FrameWork.NetWork.V4;

namespace TestNamespace
{
    public class Message
    {
        public int? OptionalValue { get; set; }
        public string OptionalText { get; set; }
    }

    [PacketSerializerContext(typeof(Message))]
    public partial class TestContext
    {
    }
}";

        var result = RunGenerator(source);
        
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        var code = result.GeneratedTrees[0].ToString();
        
        
        
    }

    [Fact]
    public void GeneratesSerializer_WithEnums()
    {
        var source = @"
using FrameWork.NetWork.V4;

namespace TestNamespace
{
    public enum Status
    {
        Active,
        Inactive,
        Pending
    }

    public class StatusMessage
    {
        public Status CurrentStatus { get; set; }
    }

    [PacketSerializerContext(typeof(StatusMessage))]
    public partial class TestContext
    {
    }
}";

        var result = RunGenerator(source);
        
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        var code = result.GeneratedTrees[0].ToString();
        
        
        
    }

    [Fact]
    public void GeneratesSerializer_WithPrimitiveTypes()
    {
        var source = @"
using FrameWork.NetWork.V4;

namespace TestNamespace
{
    public class AllPrimitives
    {
        public byte ByteValue { get; set; }
        public short ShortValue { get; set; }
        public int IntValue { get; set; }
        public long LongValue { get; set; }
        public bool BoolValue { get; set; }
        public float FloatValue { get; set; }
        public double DoubleValue { get; set; }
        public string StringValue { get; set; }
    }

    [PacketSerializerContext(typeof(AllPrimitives))]
    public partial class TestContext
    {
    }
}";

        var result = RunGenerator(source);
        
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        var code = result.GeneratedTrees[0].ToString();
        
        
        
    }

    [Fact]
    public void IgnoresNonPartialClass()
    {
        var source = @"
using FrameWork.NetWork.V4;

namespace TestNamespace
{
    public class Message
    {
        public int Value { get; set; }
    }

    [PacketSerializerContext(typeof(Message))]
    public class TestContext
    {
    }
}";

        var result = RunGenerator(source);
        
        // Should not generate anything for non-partial classes
        Assert.Empty(result.GeneratedTrees);
    }

    [Fact]
    public void GeneratesSerializer_ForMultipleRootTypes()
    {
        var source = @"
using FrameWork.NetWork.V4;

namespace TestNamespace
{
    public class Message1
    {
        public int Value { get; set; }
    }

    public class Message2
    {
        public string Text { get; set; }
    }

    [PacketSerializerContext(typeof(Message1), typeof(Message2))]
    public partial class TestContext
    {
    }
}";

        var result = RunGenerator(source);
        
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        var code = result.GeneratedTrees[0].ToString();
        
        
        
        
        
    }

    [Fact]
    public void GeneratesSerializer_WithArrays()
    {
        var source = @"
using FrameWork.NetWork.V4;

namespace TestNamespace
{
    public class ArrayMessage
    {
        public int[] Numbers { get; set; }
        public string[] Names { get; set; }
    }

    [PacketSerializerContext(typeof(ArrayMessage))]
    public partial class TestContext
    {
    }
}";

        var result = RunGenerator(source);
        
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        var code = result.GeneratedTrees[0].ToString();
        
        
        
    }

    [Fact]
    public void GeneratesSerializer_WithPacketLengthAttribute()
    {
        var source = @"
using System.Collections.Generic;
using FrameWork.NetWork.V4;

namespace TestNamespace
{
    public class Item
    {
        public int Id { get; set; }
    }

    public class SmallList
    {
        [PacketLength(1)]
        public List<Item> Items { get; set; }
    }

    [PacketSerializerContext(typeof(SmallList))]
    public partial class TestContext
    {
    }
}";

        var result = RunGenerator(source);
        
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        var code = result.GeneratedTrees[0].ToString();
        
        
        
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
            new[] { syntaxTree, CSharpSyntaxTree.ParseText(AttributeCode) },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new FrameWork.NetWork.SourceGenerators.PacketSerializerGenerator();
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

