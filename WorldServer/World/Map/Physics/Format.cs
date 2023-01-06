using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WorldServer.Physics;

namespace WorldServer.World.Physics
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct NifInfo
    {
        public int ID;
        public float MinAngle;
        public float MaxAngle;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
        public string ModelName;

        public NifInfo(int id, string model, float minAngle, float maxAngle)
        {
            ID = id;
            ModelName = model;
            MinAngle = minAngle;
            MaxAngle = maxAngle;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct FixtureInfo
    {
        public int ID;
        public int NifID;
        public int O;
        public int Scale;
        public double Angle3D;
        public double XAxis;
        public double YAxis;
        public double ZAxis;
        public double X;
        public double Y;
        public double Z;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
        public string Name;

        public FixtureInfo(int id, int nifID, string name, double x, double y, double z, int o, int scale, double angle3D, double xAxis, double yAxis, double zAxis)
        {
            ID = id;
            NifID = nifID;
            Name = name;
            X = x;
            Y = y;
            Z = z;
            O = o;
            Scale = scale;
            Angle3D = angle3D;
            XAxis = xAxis;
            YAxis = yAxis;
            ZAxis = zAxis;
        }
    }

    public class WaterBody
    {
        public float[] Vertices { get; private set; }
        public int[] Polygons { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }
        public string Name { get; private set; }
        public string Type { get; private set; }

        public WaterBody()
        {
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(Type);
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);

            writer.Write(Vertices.Length);
            for (int i = 0; i < Vertices.Length; ++i)
            {
                writer.Write(Vertices[i]);
            }
            writer.Write(Polygons.Length);
            for (int i = 0; i < Polygons.Length; ++i)
            {
                writer.Write(Polygons[i]);
            }
        }

        public void Deserialize(BinaryReader reader)
        {
            Name = reader.ReadString();
            Type = reader.ReadString();
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
            Z = reader.ReadSingle();
            int amt = reader.ReadInt32();
            Vertices = new float[amt];

            byte[] arr = reader.ReadBytes(amt * sizeof(float));
            Buffer.BlockCopy(arr, 0, Vertices, 0, arr.Length);
            amt = reader.ReadInt32();
            Polygons = new int[amt];
            arr = reader.ReadBytes(amt * sizeof(int));
            Buffer.BlockCopy(arr, 0, Polygons, 0, arr.Length);
        }
    }

    public class ObjModel
    {
        public float[] Vertices { get; private set; }
        public int[] Polygons { get; private set; }

        public ObjModel()
        {
        }

        public void Serialize(string path)
        {
            using (BinaryWriter writer = new BinaryWriter(new FileStream(path, FileMode.Create)))
            {
                writer.Write(Vertices.Length);
                for (int i = 0; i < Vertices.Length; ++i)
                {
                    writer.Write(Vertices[i]);
                }
                writer.Write(Polygons.Length);
                for (int i = 0; i < Polygons.Length; ++i)
                {
                    writer.Write(Polygons[i]);
                }
            }
        }

        public void Deserialize(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            using (BufferedBinaryReader reader = new BufferedBinaryReader(fs, (int)fs.Length))
            {
                while (reader.FillBuffer())
                {
                    int amt = reader.ReadInt32();
                    Vertices = new float[amt];
                    for (int i = 0; i < amt; ++i)
                    {
                        Vertices[i] = reader.ReadFloat();
                    }
                    amt = reader.ReadInt32();
                    Polygons = new int[amt];

                    for (int i = 0; i < amt; ++i)
                    {
                        Polygons[i] = reader.ReadInt32();
                    }
                }
            }
            fs.Close();
            fs.Dispose();
        }

        public void Deserialize(byte[] bytes)
        {
            MemoryStream ms = new MemoryStream(bytes);
            using (BinaryReader reader = new BinaryReader(ms))
            {
                int amt = reader.ReadInt32();
                Vertices = new float[amt];
                byte[] arr = reader.ReadBytes(amt * sizeof(float));
                Buffer.BlockCopy(arr, 0, Vertices, 0, arr.Length);

                amt = reader.ReadInt32();
                Polygons = new int[amt];
                arr = reader.ReadBytes(amt * sizeof(int));
                Buffer.BlockCopy(arr, 0, Polygons, 0, arr.Length);
            }
            ms.Close();
            ms.Dispose();
        }
    }

    public class BufferedBinaryReader : IDisposable
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private readonly int bufferSize;
        private int bufferOffset;
        private int numBufferedBytes;
        private byte[] m_charBytes;
        private char[] m_charBuffer;
        private int m_maxCharsSize;
        private Decoder m_decoder;

        public BufferedBinaryReader(Stream stream, int bufferSize)
        {
            this.stream = stream;
            this.bufferSize = bufferSize;
            buffer = new byte[bufferSize];
            bufferOffset = bufferSize;
            Encoding encoding = new ASCIIEncoding();
            m_decoder = encoding.GetDecoder();
            m_maxCharsSize = 1;
        }

        public int NumBytesAvailable
        { get { return Math.Max(0, numBufferedBytes - bufferOffset); } }

        public bool FillBuffer()
        {
            var numBytesUnread = bufferSize - bufferOffset;
            var numBytesToRead = bufferSize - numBytesUnread;
            bufferOffset = 0;
            numBufferedBytes = numBytesUnread;
            if (numBytesUnread > 0)
            {
                Buffer.BlockCopy(buffer, numBytesToRead, buffer, 0, numBytesUnread);
            }
            while (numBytesToRead > 0)
            {
                var numBytesRead = stream.Read(buffer, numBytesUnread, numBytesToRead);
                if (numBytesRead == 0)
                {
                    return false;
                }
                numBufferedBytes += numBytesRead;
                numBytesToRead -= numBytesRead;
                numBytesUnread += numBytesRead;
            }
            return true;
        }

        public ushort ReadUInt16()
        {
            var val = (ushort)((int)buffer[bufferOffset] | (int)buffer[bufferOffset + 1] << 8);
            bufferOffset += 2;
            return val;
        }

        public int ReadInt32()
        {
            int val = ((int)buffer[bufferOffset] |
                       (int)buffer[bufferOffset + 1] << 8 |
                       (int)buffer[bufferOffset + 2] << 16 |
                       (int)buffer[bufferOffset + 3] << 24);
            bufferOffset += 4;
            return val;
        }

        public virtual String ReadString()
        {
            int stringLength;

            // Length of the string in chars
            stringLength = ReadInt32();

            if (stringLength == 0)
            {
                return String.Empty;
            }

            if (m_charBuffer == null)
            {
                m_charBuffer = new char[stringLength];
            }

            m_decoder.GetChars(buffer, bufferOffset, stringLength * m_maxCharsSize, m_charBuffer, 0);
            bufferOffset += stringLength * m_maxCharsSize;
            return new String(m_charBuffer, 0, stringLength);
        }

        public unsafe float ReadFloat()
        {
            // Read four bytes
            uint val = (
                (uint)buffer[bufferOffset] |
                (uint)buffer[bufferOffset + 1] << 8 |
                (uint)buffer[bufferOffset + 2] << 16 |
                (uint)buffer[bufferOffset + 3] << 24);
            bufferOffset += 4;

            // Reinterpret the uint as a float (verbose to show the process)
            uint* pVal = &val;
            float* pFloat = (float*)pVal;
            float floatVal = *pFloat;
            return floatVal;
        }

        public void Dispose()
        {
            stream.Close();
        }
    }
}