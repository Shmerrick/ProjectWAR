namespace Core.Infrastructure.Network
{
    /// <summary>
    /// Marks a method as an RPC packet handler.
    /// Methods must have exactly one parameter (the request) and optionally return a response.
    /// Supports both synchronous and asynchronous (Task/ValueTask) handlers.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class RpcAttribute : Attribute
    {
        /// <summary>
        /// Gets the opcode that this handler processes.
        /// </summary>
        public byte Opcode { get; }

        /// <summary>
        /// Gets the opcode to use for the response, or null to use the same opcode as the request.
        /// </summary>
        public byte? ResponseOpcode { get; }

        /// <summary>
        /// Creates a new RPC attribute for the specified opcode.
        /// </summary>
        /// <param name="opcode">The opcode this handler processes.</param>
        public RpcAttribute(byte opcode)
        {
            Opcode = opcode;
            ResponseOpcode = null;
        }
        
        /// <summary>
        /// Creates a new RPC attribute with different request and response opcodes.
        /// </summary>
        /// <param name="opcode">The opcode this handler processes.</param>
        /// <param name="responseOpcode">The opcode to use for the response.</param>
        public RpcAttribute(byte opcode, byte responseOpcode)
        {
            Opcode = opcode;
            ResponseOpcode = responseOpcode;
        }
    }
}
