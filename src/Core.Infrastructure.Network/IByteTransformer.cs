namespace Core.Infrastructure.Network
{
    /// <summary>
    /// Transforms raw bytes (e.g., decryption, decompression) before packet processing.
    /// Use ArrayPool&lt;byte&gt; for temporary buffers to minimize allocations.
    /// </summary>
    public interface IByteTransformer
    {
        /// <summary>
        /// Transforms input bytes into output buffer.
        /// </summary>
        /// <param name="input">The input bytes to transform.</param>
        /// <param name="output">The output buffer to write transformed bytes.</param>
        /// <returns>The number of bytes written to output.</returns>
        int Transform(ReadOnlySpan<byte> input, Span<byte> output);
    }
}
