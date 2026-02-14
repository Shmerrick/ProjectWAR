namespace Core.Infrastructure.Network;

/// <summary>
/// Assigns a packet handler to a named packet group.
/// Handlers in the same group share a single generated dispatcher and service collection registration.
/// If omitted, the handler belongs to the "Default" group.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class PacketGroupAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the packet group.
    /// </summary>
    public string GroupName { get; }

    /// <summary>
    /// Creates a new <see cref="PacketGroupAttribute"/> with the specified group name.
    /// </summary>
    /// <param name="groupName">The name of the packet group.</param>
    public PacketGroupAttribute(string groupName = "Default")
    {
        GroupName = groupName ?? throw new ArgumentNullException(nameof(groupName));
    }
}
