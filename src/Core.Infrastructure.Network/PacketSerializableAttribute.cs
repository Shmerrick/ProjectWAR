namespace Core.Infrastructure.Network
{
    /// <summary>
    /// Marks a class as a packet serializer context for source generation.
    /// Specify types to serialize via the constructor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class PacketSerializerContextAttribute : Attribute
    {
        /// <summary>
        /// Creates a packet serializer context with the specified types
        /// </summary>
        /// <param name="types">Types to generate serializers for</param>
        public PacketSerializerContextAttribute(params Type[] types)
        {
            Types = types;
        }

        /// <summary>
        /// Gets the types to generate serializers for
        /// </summary>
        public Type[] Types { get; }
    }
}
