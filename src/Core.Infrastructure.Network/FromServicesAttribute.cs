namespace Core.Infrastructure.Network;

/// <summary>
/// Marks a parameter on an RPC handler method for dependency injection from the service provider.
/// Services are resolved from a per-packet scoped service provider.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class FromServicesAttribute : Attribute
{
}
