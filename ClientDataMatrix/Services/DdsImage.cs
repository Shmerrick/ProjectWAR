using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace ClientDataMatrix.Services
{
    /// <summary>
    /// Decodes the client's icon textures to a <see cref="Bitmap"/>.
    ///
    /// WHY THIS EXISTS WHEN THE TOOLKIT ALREADY DECODES DXT1. WAR-RE-Toolkit has the block algorithm
    /// twice -- `apps/diffuse2png/MythicTexture.cs` and `apps/diffuse2png2/DXT1Test` -- and this is a
    /// deliberate second copy of about forty lines of it, so say why rather than let it look like an
    /// oversight:
    ///
    ///   - Those read a *different file format*. Their `ReadHeader` takes Magic, FileVersion,
    ///     FileSize, ID, Width, Height, ImageCount and a mipmap table: Mythic's proprietary texture
    ///     container for world and character art. The interface icons under
    ///     `eatemplate_icons/textures` are ordinary `DDS ` files with a 128-byte standard header.
    ///     Only the 4x4 block math is shared; the container parsing is not.
    ///   - The toolkit builds on .NET 10 and its own solution; this project is .NET Framework 4.8.
    ///     There is no assembly either repo can reference from the other, so "reuse" would mean
    ///     copying the same forty lines anyway, with a cross-repo dependency added for nothing.
    ///
    /// If the block math ever needs fixing, fix it in all three places. For anything beyond icons --
    /// world textures, meshes, animations -- use the toolkit; it owns that work and this does not
    /// try to.
    ///
    /// DXT1 stores each 4x4 block in eight bytes: two RGB565 endpoints then sixteen 2-bit indices.
    /// When the first endpoint is not greater than the second the block is in one-bit-alpha mode,
    /// where index 3 is transparent and the third colour is the midpoint rather than a two-thirds
    /// blend. That mode matters here -- item icons are cut-outs, and treating it as opaque puts a
    /// black box behind every one of them.
    /// </summary>
    public static class DdsImage
    {
        private const int HeaderSize = 128;
        private const int OffsetHeight = 12;
        private const int OffsetWidth = 16;
        private const int OffsetFourCc = 84;

        /// <summary>
        /// Reads a DXT1 .dds into a 32bpp bitmap, or returns null with a reason when it cannot.
        /// Never throws for a malformed file: the caller is a UI that must keep working when one
        /// texture out of thousands is bad.
        /// </summary>
        public static Bitmap TryLoad(string path, out string error)
        {
            error = null;

            byte[] raw;
            try
            {
                raw = File.ReadAllBytes(path);
            }
            catch (IOException exception)
            {
                error = exception.Message;
                return null;
            }
            catch (UnauthorizedAccessException exception)
            {
                error = exception.Message;
                return null;
            }

            if (raw.Length < HeaderSize || raw[0] != 'D' || raw[1] != 'D' || raw[2] != 'S' || raw[3] != ' ')
            {
                error = "Not a DDS file";
                return null;
            }

            string fourCc = "" + (char)raw[OffsetFourCc] + (char)raw[OffsetFourCc + 1]
                + (char)raw[OffsetFourCc + 2] + (char)raw[OffsetFourCc + 3];
            if (fourCc != "DXT1")
            {
                error = "Unsupported DDS format " + fourCc + " (only DXT1 appears in this client)";
                return null;
            }

            int height = BitConverter.ToInt32(raw, OffsetHeight);
            int width = BitConverter.ToInt32(raw, OffsetWidth);
            if (width <= 0 || height <= 0 || width > 4096 || height > 4096)
            {
                error = "Implausible dimensions " + width + "x" + height;
                return null;
            }

            int blocksWide = (width + 3) / 4;
            int blocksHigh = (height + 3) / 4;
            if (raw.Length < HeaderSize + blocksWide * blocksHigh * 8)
            {
                error = "Truncated: file is shorter than its own dimensions require";
                return null;
            }

            var pixels = new byte[width * height * 4];

            for (int by = 0; by < blocksHigh; ++by)
            {
                for (int bx = 0; bx < blocksWide; ++bx)
                {
                    int offset = HeaderSize + (by * blocksWide + bx) * 8;
                    DecodeBlock(raw, offset, pixels, bx * 4, by * 4, width, height);
                }
            }

            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData locked = bitmap.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < height; ++y)
                    Marshal.Copy(pixels, y * width * 4, locked.Scan0 + y * locked.Stride, width * 4);
            }
            finally
            {
                bitmap.UnlockBits(locked);
            }

            return bitmap;
        }

        private static void DecodeBlock(byte[] raw, int offset, byte[] pixels,
            int originX, int originY, int width, int height)
        {
            ushort c0 = (ushort)(raw[offset] | (raw[offset + 1] << 8));
            ushort c1 = (ushort)(raw[offset + 2] | (raw[offset + 3] << 8));

            var r = new byte[4];
            var g = new byte[4];
            var b = new byte[4];
            var a = new byte[4];

            Unpack565(c0, out r[0], out g[0], out b[0]);
            Unpack565(c1, out r[1], out g[1], out b[1]);
            a[0] = a[1] = a[2] = a[3] = 255;

            if (c0 > c1)
            {
                r[2] = (byte)((2 * r[0] + r[1]) / 3);
                g[2] = (byte)((2 * g[0] + g[1]) / 3);
                b[2] = (byte)((2 * b[0] + b[1]) / 3);
                r[3] = (byte)((r[0] + 2 * r[1]) / 3);
                g[3] = (byte)((g[0] + 2 * g[1]) / 3);
                b[3] = (byte)((b[0] + 2 * b[1]) / 3);
            }
            else
            {
                // One-bit alpha mode: index 3 is a hole, not a colour.
                r[2] = (byte)((r[0] + r[1]) / 2);
                g[2] = (byte)((g[0] + g[1]) / 2);
                b[2] = (byte)((b[0] + b[1]) / 2);
                r[3] = g[3] = b[3] = 0;
                a[3] = 0;
            }

            uint indices = (uint)(raw[offset + 4] | (raw[offset + 5] << 8)
                | (raw[offset + 6] << 16) | (raw[offset + 7] << 24));

            for (int y = 0; y < 4; ++y)
            {
                for (int x = 0; x < 4; ++x)
                {
                    int px = originX + x;
                    int py = originY + y;
                    if (px >= width || py >= height)
                        continue;

                    int code = (int)((indices >> (2 * (4 * y + x))) & 3);
                    int target = (py * width + px) * 4;

                    // 32bppArgb is BGRA in memory.
                    pixels[target] = b[code];
                    pixels[target + 1] = g[code];
                    pixels[target + 2] = r[code];
                    pixels[target + 3] = a[code];
                }
            }
        }

        private static void Unpack565(ushort value, out byte r, out byte g, out byte b)
        {
            int r5 = (value >> 11) & 0x1F;
            int g6 = (value >> 5) & 0x3F;
            int b5 = value & 0x1F;

            // Replicate the high bits into the low ones so 31 maps to 255, not 248.
            r = (byte)((r5 << 3) | (r5 >> 2));
            g = (byte)((g6 << 2) | (g6 >> 4));
            b = (byte)((b5 << 3) | (b5 >> 2));
        }
    }
}
