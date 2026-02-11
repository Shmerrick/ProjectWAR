# RpcSourceGenerator.Tests

Comprehensive unit tests for the RpcSourceGenerator project with 89.4% code coverage.

## Overview

This test project provides extensive testing for three source generators:
- **RpcSourceGenerator**: Server-side RPC handler generation
- **RpcClientGenerator**: Client-side RPC proxy generation
- **PacketSerializerGenerator**: Packet serialization/deserialization generation

## Test Structure

### RpcSourceGenerator Tests (12 tests)
Tests for server-side RPC handler generation:
- Synchronous and asynchronous method handling
- Request/response parameter handling
- Custom response opcodes
- Diagnostic error reporting (RPC001, RPC002)
- Edge cases (non-partial classes, non-Client inheritance, empty classes)

### RpcClientGenerator Tests (10 tests)
Tests for client-side RPC proxy generation:
- Async methods with request/response
- Fire-and-forget patterns (no response expected)
- Methods without request parameters
- Accessibility modifiers (public, internal, protected, private)
- Partial method implementation
- Default response opcode handling

### PacketSerializerGenerator Tests (10 tests)
Tests for packet serialization:
- Primitive types (int, string, bool, byte, short, long, float, double)
- Nested complex types with recursive type discovery
- Collections (List, Array, IEnumerable)
- Nullable properties
- Enums
- PacketLength attributes (1, 2, 4 byte length encoding)
- Multiple root types

### Snapshot Tests (2 tests)
Regression prevention using Verify.NET:
- RpcSourceGenerator complete handler snapshot
- RpcClientGenerator complete proxy snapshot

## Running Tests

```bash
# Run all tests
dotnet test

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "FullyQualifiedName~RpcSourceGeneratorTests"

# Run with detailed output
dotnet test --verbosity normal
```

## Code Coverage

Current coverage: **89.4%**

To view detailed coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
# Open the generated coverage.cobertura.xml file
```

## Key Testing Patterns

### Generator Testing Pattern
All tests follow this pattern:
1. Define test source code with attributes
2. Parse and compile the source
3. Run the generator
4. Assert on generated code and diagnostics

### Snapshot Testing
Snapshot tests use Verify.NET to:
1. Generate code
2. Compare against verified snapshots
3. Flag any changes for review
4. Scrub dynamic data (auto-generated comments, nullable directives)

## Dependencies

- **xUnit**: Test framework
- **Microsoft.CodeAnalysis.CSharp**: Roslyn APIs for compilation
- **Verify.Xunit**: Snapshot testing
- **Verify.SourceGenerators**: Source generator snapshot support
- **coverlet.collector**: Code coverage

## Edge Cases Tested

1. **Non-partial classes**: Should be ignored by generators
2. **Non-Client inheritance**: Should be ignored by RPC generators
3. **Duplicate opcodes**: Should report RPC001 diagnostic error
4. **Invalid signatures**: Should report RPC002 diagnostic error
5. **Multiple methods**: Proper switch case generation
6. **Fully qualified types**: Correct type name handling in generated code
7. **Nullable properties**: Proper null handling in serialization
8. **Collections**: Correct length encoding and deserialization

## Best Practices

- Tests are focused on actual usage patterns, not artificial scenarios
- Each test validates a single concern
- Edge cases are explicitly tested
- Snapshot tests prevent regressions
- Code coverage is maintained above 80%

## Troubleshooting

### Snapshot Test Failures
If snapshot tests fail after legitimate changes:
1. Review the `.received.txt` file in the Snapshots folder
2. If changes are correct, rename it to `.verified.txt`
3. Re-run tests to confirm

### Package Version Warnings
The NU1608 warnings about Microsoft.CodeAnalysis version mismatches are expected and can be safely ignored. They occur because different packages reference different versions of the Roslyn libraries.
