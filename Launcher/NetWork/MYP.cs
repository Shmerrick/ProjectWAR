using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace WarZoneLib
{
    public enum MythicPackage
    {
        ART = 1,
        ART2 = 2,
        ART3 = 3,
        AUDIO = 4,
        DATA = 5,
        INTERFACE = 6,
        MFT = 7,
        PATCH = 8,
        VO_ENGLISH = 9,
        VO_FRENCH = 10, // 0x0000000A
        WARTEST = 11, // 0x0000000B
        WORLD = 12, // 0x0000000C
    }

    public class MYP : IDisposable
    {
        public Dictionary<long, MYP.MFTEntry> Enteries = new Dictionary<long, MYP.MFTEntry>();
        public List<MYP.MFTHeader> MFTTables = new List<MYP.MFTHeader>();
        public MYP.MYPHeader Header;
        public MYP.MFTHeader MFT;
        private Stream _stream;
        public MythicPackage Package;

        public static string GetExtension(byte[] buffer)
        {
            byte[] bytes = buffer;
            string str1 = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
            char[] separator = new char[1] { ',' };
            int length = str1.Split(separator, 10).Length;
            string str2 = Encoding.ASCII.GetString(bytes, 0, 4);
            string str3 = "txt";
            if ((int)bytes[0] == 0 && (int)bytes[1] == 1 && (int)bytes[2] == 0)
                str3 = "ttf";
            else if ((int)bytes[0] == 10 && (int)bytes[1] == 5 && ((int)bytes[2] == 1 && (int)bytes[3] == 8))
                str3 = "pcx";
            else if (str2.IndexOf("PK") >= 0)
                str3 = "zip";
            else if (str2.IndexOf("SCPT") >= 0)
                str3 = "scpt";
            else if (str2.IndexOf("<") >= 0)
                str3 = "xml";
            else if (str1.IndexOf("lua") >= 0 && str1.IndexOf("lua") < 50)
                str3 = "lua";
            else if (str2.IndexOf("DDS") >= 0)
                str3 = "dds";
            else if (str2.IndexOf("XSM") >= 0)
                str3 = "xsm";
            else if (str2.IndexOf("XAC") >= 0)
                str3 = "xac";
            else if (str2.IndexOf("8BPS") >= 0)
                str3 = "8bps";
            else if (str2.IndexOf("bdLF") >= 0)
                str3 = "db";
            else if (str2.IndexOf("gsLF") >= 0)
                str3 = "geom";
            else if (str2.IndexOf("idLF") >= 0)
                str3 = "diffuse";
            else if (str2.IndexOf("psLF") >= 0)
                str3 = "specular";
            else if (str2.IndexOf("amLF") >= 0)
                str3 = "mask";
            else if (str2.IndexOf("ntLF") >= 0)
                str3 = "tint";
            else if (str2.IndexOf("lgLF") >= 0)
                str3 = "glow";
            else if (str1.IndexOf("Gamebry") >= 0)
                str3 = "nif";
            else if (str1.IndexOf("WMPHOTO") >= 0)
                str3 = "lmp";
            else if (str2.IndexOf("RIFF") >= 0)
                str3 = Encoding.ASCII.GetString(bytes, 8, 4).IndexOf("WAVE") < 0 ? "riff" : "wav";
            else if (str2.IndexOf("; Zo") >= 0)
                str3 = "zone.txt";
            else if (str2.IndexOf("\0\0\0\0") >= 0)
                str3 = "zero.txt";
            else if (str2.IndexOf("PNG") >= 0)
                str3 = "png";
            else if (str2.IndexOf("AMX") >= 0)
                str3 = "amx";
            else if (str2.IndexOf("SIDS") >= 0)
                str3 = "sids";
            else if (length >= 10)
                str3 = "csv";
            return str3;
        }

        public MYP(MythicPackage package, Stream stream)
        {
            this._stream = stream;
            this.Package = package;
            BinaryReader reader = new BinaryReader(stream);
            this.Header = new MYP.MYPHeader();
            this.Header.Load(reader);
            this.MFTTables.Add(new MYP.MFTHeader()
            {
                Length = this.Header.EntriesPerMFT,
                NextMFTOffset = this.Header.MFTOffset
            });
            long num1 = this.Header.MFTOffset;
            uint num2 = this.Header.EntriesPerMFT;
            reader.BaseStream.Position = num1;
            while (num1 > 0L)
            {
                reader.BaseStream.Position = num1;
                this.MFT = new MYP.MFTHeader();
                this.MFT.Offset = reader.BaseStream.Position;
                this.MFT.Load(reader);
                this.MFTTables.Add(this.MFT);
                num1 = this.MFT.NextMFTOffset;
                for (int index = 0; (long)index < (long)num2; ++index)
                {
                    MYP.MFTEntry mftEntry = new MYP.MFTEntry();
                    mftEntry.Changed = false;
                    mftEntry.Load(this, reader, index);
                    if (mftEntry.Hash != 0L)
                        this.Enteries[mftEntry.Hash] = mftEntry;
                }
                num2 = this.MFT.Length;
            }
        }

        public void UpdateFile(string name, byte[] data)
        {
            this.UpdateFile(MYP.HashWAR(name), data);
        }

        public byte[] ReadFileHeader(long hash)
        {
            if (!this.Enteries.ContainsKey(hash))
                return (byte[])null;
            this._stream.Position = (long)this.Enteries[hash].Offset;
            byte[] buffer = new byte[(int)this.Enteries[hash].HeaderSize];
            this._stream.Read(buffer, 0, (int)this.Enteries[hash].HeaderSize);
            return buffer;
        }

        public void UpdateFile(long hash, byte[] data)
        {
            if (this.Enteries.ContainsKey(hash))
            {
                this.Enteries[hash].Changed = true;
                this.Enteries[hash].Data = data;
            }
            else
                this.Enteries[hash] = new MYP.MFTEntry()
                {
                    Changed = true,
                    Data = data,
                    Compressed = (byte)1,
                    Hash = hash
                };
        }

        public void Save()
        {
            BinaryWriter binaryWriter = new BinaryWriter(this._stream);
            foreach (MYP.MFTEntry mftEntry in this.Enteries.Where<KeyValuePair<long, MYP.MFTEntry>>((Func<KeyValuePair<long, MYP.MFTEntry>, bool>)(e =>
            {
                if (e.Value.Changed)
                    return e.Value.Offset > 0UL;
                return false;
            })).Select<KeyValuePair<long, MYP.MFTEntry>, MYP.MFTEntry>((Func<KeyValuePair<long, MYP.MFTEntry>, MYP.MFTEntry>)(e => e.Value)).ToList<MYP.MFTEntry>())
            {
                byte[] numArray = mftEntry.Data;
                if ((int)mftEntry.Compressed == 1)
                {
                    MemoryStream memoryStream = new MemoryStream();
                    using (DeflateStream deflateStream = new DeflateStream((Stream)memoryStream, CompressionMode.Compress, true))
                        deflateStream.Write(mftEntry.Data, 0, mftEntry.Data.Length);
                    numArray = memoryStream.ToArray();
                }
                if ((long)numArray.Length > (long)mftEntry.CompressedSize)
                {
                    this._stream.Position = mftEntry.MFTEntryOffset;
                    binaryWriter.Write((ulong)this._stream.Length);
                    binaryWriter.Write(mftEntry.HeaderSize);
                    if ((int)mftEntry.Compressed == 1)
                        binaryWriter.Write((uint)(numArray.Length + 2));
                    else
                        binaryWriter.Write((uint)numArray.Length);
                    binaryWriter.Write((uint)mftEntry.Data.Length);
                    this._stream.Position = mftEntry.MFTEntryOffset + 28L;
                    binaryWriter.Write(CRC32.ComputeChecksum(numArray));
                    this._stream.Position = this._stream.Length;
                    mftEntry.Offset = (ulong)this._stream.Length;
                    binaryWriter.Write(this.ReadFileHeader(mftEntry.Hash));
                    if ((int)mftEntry.Compressed == 1)
                        binaryWriter.Write((ushort)40056);
                    binaryWriter.Write(numArray);
                }
                else
                {
                    this._stream.Position = mftEntry.MFTEntryOffset;
                    binaryWriter.Write(mftEntry.Offset);
                    binaryWriter.Write(mftEntry.HeaderSize);
                    if ((int)mftEntry.Compressed == 1)
                        binaryWriter.Write((uint)(numArray.Length + 2));
                    else
                        binaryWriter.Write((uint)numArray.Length);
                    binaryWriter.Write((uint)mftEntry.Data.Length);
                    this._stream.Position = mftEntry.MFTEntryOffset + 28L;
                    binaryWriter.Write(CRC32.ComputeChecksum(numArray));
                    this._stream.Position = (int)mftEntry.Compressed != 1 ? (long)mftEntry.Offset + (long)mftEntry.HeaderSize : (long)mftEntry.Offset + (long)mftEntry.HeaderSize + 2L;
                    binaryWriter.Write(numArray);
                }
            }
            List<MYP.MFTEntry> list = this.Enteries.Where<KeyValuePair<long, MYP.MFTEntry>>((Func<KeyValuePair<long, MYP.MFTEntry>, bool>)(e => (long)e.Value.Offset == 0L)).Select<KeyValuePair<long, MYP.MFTEntry>, MYP.MFTEntry>((Func<KeyValuePair<long, MYP.MFTEntry>, MYP.MFTEntry>)(e => e.Value)).ToList<MYP.MFTEntry>();
            if (list.Count <= 0)
                return;
            this._stream.Position = 24L;
            binaryWriter.Write((long)(uint)this.Enteries.Count + (long)list.Count);
            if (this.MFTTables.Count > 0)
            {
                this._stream.Position = this.MFTTables.Last<MYP.MFTHeader>().Offset + 4L;
                binaryWriter.Write((ulong)binaryWriter.BaseStream.Length);
            }
            binaryWriter.BaseStream.Position = binaryWriter.BaseStream.Length;
            int index1 = 0;
            List<MYP.MFTEntry> mftEntryList = new List<MYP.MFTEntry>();
            int count = list.Count;
            while (count > 0)
            {
                binaryWriter.Write(1000U);
                if (list.Count - index1 > 1000)
                {
                    long position = this._stream.Position;
                    binaryWriter.Write(0L);
                }
                else
                    binaryWriter.Write(0L);
                for (int index2 = 0; index2 < 1000; ++index2)
                {
                    if (index1 < list.Count)
                    {
                        --count;
                        MYP.MFTEntry mftEntry = list[index1];
                        MemoryStream memoryStream = new MemoryStream();
                        using (DeflateStream deflateStream = new DeflateStream((Stream)memoryStream, CompressionMode.Compress, true))
                            deflateStream.Write(mftEntry.Data, 0, mftEntry.Data.Length);
                        memoryStream.ToArray();
                        mftEntry.MFTEntryOffset = this._stream.Position;
                        binaryWriter.Write(0L);
                        binaryWriter.Write(mftEntry.HeaderSize);
                        binaryWriter.Write(mftEntry.CompressedSize);
                        binaryWriter.Write(mftEntry.UnCompressedSize);
                        binaryWriter.Write(mftEntry.Hash);
                        binaryWriter.Write(mftEntry.CRC32);
                        binaryWriter.Write(mftEntry.Compressed);
                        binaryWriter.Write((byte)0);
                        mftEntryList.Add(mftEntry);
                        ++index1;
                    }
                    else
                    {
                        binaryWriter.Write(0L);
                        binaryWriter.Write(0U);
                        binaryWriter.Write(0U);
                        binaryWriter.Write(0U);
                        binaryWriter.Write(0L);
                        binaryWriter.Write(0U);
                        binaryWriter.Write((byte)0);
                        binaryWriter.Write((byte)0);
                    }
                }
                int num = 0;
                foreach (MYP.MFTEntry mftEntry in mftEntryList)
                {
                    long position = this._stream.Position;
                    this._stream.Position = mftEntry.MFTEntryOffset;
                    binaryWriter.Write(position);
                    this._stream.Position = position;
                    byte[] data = mftEntry.Data;
                    this._stream.Write(data, 0, data.Length);
                    if ((int)mftEntry.Compressed == 1)
                    {
                        byte[] buffer = new byte[(int)mftEntry.UnCompressedSize];
                        MemoryStream memoryStream = new MemoryStream(data);
                        memoryStream.Position = (long)(mftEntry.HeaderSize + 2U);
                        using (DeflateStream deflateStream = new DeflateStream((Stream)memoryStream, CompressionMode.Decompress, true))
                            deflateStream.Read(buffer, 0, buffer.Length);
                    }
                    ++num;
                }
                mftEntryList.Clear();
            }
        }

        public void AddEntries(List<Tuple<string, byte[]>> files)
        {
            this._stream.Position = 24L;
            BinaryWriter binaryWriter = new BinaryWriter(this._stream);
            binaryWriter.Write((long)(uint)this.Enteries.Count + (long)files.Count);
            if (this.MFTTables.Count > 0)
            {
                this._stream.Position = this.MFTTables.Last<MYP.MFTHeader>().Offset + 4L;
                binaryWriter.Write((ulong)binaryWriter.BaseStream.Length);
            }
            binaryWriter.BaseStream.Position = binaryWriter.BaseStream.Length;
            int index1 = 0;
            List<MYP.MFTEntry> mftEntryList = new List<MYP.MFTEntry>();
            int count = files.Count;
            long num1 = 0;
            while (count > 0)
            {
                binaryWriter.Write(1000U);
                if (files.Count - index1 > 1000)
                {
                    num1 = this._stream.Position;
                    binaryWriter.Write(0L);
                }
                else
                    binaryWriter.Write(0L);
                for (int index2 = 0; index2 < 1000; ++index2)
                {
                    if (index1 < files.Count)
                    {
                        --count;
                        MYP.MFTEntry mftEntry = new MYP.MFTEntry();
                        MemoryStream memoryStream = new MemoryStream();
                        using (DeflateStream deflateStream = new DeflateStream((Stream)memoryStream, CompressionMode.Compress, true))
                            deflateStream.Write(files[index1].Item2, 0, files[index1].Item2.Length);
                        mftEntry.Data = new byte[memoryStream.Length + 2L];
                        mftEntry.Data[0] = (byte)120;
                        mftEntry.Data[1] = (byte)156;
                        Buffer.BlockCopy((Array)memoryStream.ToArray(), 0, (Array)mftEntry.Data, 2, (int)memoryStream.Length);
                        mftEntry.CompressedSize = (uint)mftEntry.Data.Length;
                        mftEntry.Hash = HashWAR(files[index1].Item1);
                        mftEntry.CRC32 = CRC32.ComputeChecksum(files[index1].Item2);
                        mftEntry.Compressed = (byte)1;
                        mftEntry.UnCompressedSize = (uint)files[index1].Item2.Length;
                        mftEntry.MFTEntryOffset = this._stream.Position;
                        binaryWriter.Write(0L);
                        binaryWriter.Write(mftEntry.HeaderSize);
                        binaryWriter.Write(mftEntry.CompressedSize);
                        binaryWriter.Write(mftEntry.UnCompressedSize);
                        binaryWriter.Write(mftEntry.Hash);
                        binaryWriter.Write(mftEntry.CRC32);
                        binaryWriter.Write(mftEntry.Compressed);
                        binaryWriter.Write((byte)0);
                        mftEntryList.Add(mftEntry);
                        ++index1;
                    }
                    else
                    {
                        binaryWriter.Write(0L);
                        binaryWriter.Write(0U);
                        binaryWriter.Write(0U);
                        binaryWriter.Write(0U);
                        binaryWriter.Write(0L);
                        binaryWriter.Write(0U);
                        binaryWriter.Write((byte)0);
                        binaryWriter.Write((byte)0);
                    }
                }
                int num2 = 0;
                foreach (MYP.MFTEntry mftEntry in mftEntryList)
                {
                    long position = this._stream.Position;
                    this._stream.Position = mftEntry.MFTEntryOffset;
                    binaryWriter.Write(position);
                    this._stream.Position = position;
                    byte[] data = mftEntry.Data;
                    this._stream.Write(data, 0, data.Length);
                    if ((int)mftEntry.Compressed == 1)
                    {
                        byte[] buffer = new byte[(int)mftEntry.UnCompressedSize];
                        MemoryStream memoryStream = new MemoryStream(data);
                        memoryStream.Position = (long)(mftEntry.HeaderSize + 2U);
                        using (DeflateStream deflateStream = new DeflateStream((Stream)memoryStream, CompressionMode.Decompress, true))
                            deflateStream.Read(buffer, 0, buffer.Length);
                    }
                    ++num2;
                }
                if (index1 < files.Count)
                {
                    long position = this._stream.Position;
                    this._stream.Position = num1;
                    binaryWriter.Write(position);
                    this._stream.Position = position;
                }
                mftEntryList.Clear();
            }
        }

        public static long HashWAR(string s)
        {
            uint eax, ecx, edx, ebx, esi, edi;

            eax = ecx = edx = ebx = esi = edi = 0;
            ebx = edi = esi = (uint)s.Length + 0xDEADBEEF;

            int i = 0;

            for (i = 0; i + 12 < s.Length; i += 12)
            {
                edi = (uint)((s[i + 7] << 24) | (s[i + 6] << 16) | (s[i + 5] << 8) | s[i + 4]) + edi;
                esi = (uint)((s[i + 11] << 24) | (s[i + 10] << 16) | (s[i + 9] << 8) | s[i + 8]) + esi;
                edx = (uint)((s[i + 3] << 24) | (s[i + 2] << 16) | (s[i + 1] << 8) | s[i]) - esi;

                edx = (edx + ebx) ^ (esi >> 28) ^ (esi << 4);
                esi += edi;
                edi = (edi - edx) ^ (edx >> 26) ^ (edx << 6);
                edx += esi;
                esi = (esi - edi) ^ (edi >> 24) ^ (edi << 8);
                edi += edx;
                ebx = (edx - esi) ^ (esi >> 16) ^ (esi << 16);
                esi += edi;
                edi = (edi - ebx) ^ (ebx >> 13) ^ (ebx << 19);
                ebx += esi;
                esi = (esi - edi) ^ (edi >> 28) ^ (edi << 4);
                edi += ebx;
            }

            if (s.Length - i > 0)
            {
                switch (s.Length - i)
                {
                    case 12:
                        esi += (uint)s[i + 11] << 24;
                        goto case 11;
                    case 11:
                        esi += (uint)s[i + 10] << 16;
                        goto case 10;
                    case 10:
                        esi += (uint)s[i + 9] << 8;
                        goto case 9;
                    case 9:
                        esi += (uint)s[i + 8];
                        goto case 8;
                    case 8:
                        edi += (uint)s[i + 7] << 24;
                        goto case 7;
                    case 7:
                        edi += (uint)s[i + 6] << 16;
                        goto case 6;
                    case 6:
                        edi += (uint)s[i + 5] << 8;
                        goto case 5;
                    case 5:
                        edi += (uint)s[i + 4];
                        goto case 4;
                    case 4:
                        ebx += (uint)s[i + 3] << 24;
                        goto case 3;
                    case 3:
                        ebx += (uint)s[i + 2] << 16;
                        goto case 2;
                    case 2:
                        ebx += (uint)s[i + 1] << 8;
                        goto case 1;
                    case 1:
                        ebx += (uint)s[i];
                        break;
                }

                esi = (esi ^ edi) - ((edi >> 18) ^ (edi << 14));
                ecx = (esi ^ ebx) - ((esi >> 21) ^ (esi << 11));
                edi = (edi ^ ecx) - ((ecx >> 7) ^ (ecx << 25));
                esi = (esi ^ edi) - ((edi >> 16) ^ (edi << 16));
                edx = (esi ^ ecx) - ((esi >> 28) ^ (esi << 4));
                edi = (edi ^ edx) - ((edx >> 18) ^ (edx << 14));
                eax = (esi ^ edi) - ((edi >> 8) ^ (edi << 24));

                return ((long)edi << 32) | eax;
            }

            return ((long)esi << 32) | eax;
        }

        public byte[] ReadFile(string name)
        {
            long key = MYP.HashWAR(name);
            if (this.Enteries.ContainsKey(key))
                return this.ReadFile(this.Enteries[key]);
            return (byte[])null;
        }

        public byte[] ReadFile(MYP.MFTEntry entry)
        {
            if ((int)entry.Compressed == 1)
            {
                byte[] buffer = new byte[(int)entry.UnCompressedSize];
                this._stream.Position = (long)entry.Offset + (long)entry.HeaderSize + 2L;
                MemoryStream memoryStream = new MemoryStream(buffer);
                using (DeflateStream deflateStream = new DeflateStream(this._stream, CompressionMode.Decompress, true))
                    deflateStream.Read(buffer, 0, buffer.Length);
                return buffer;
            }
            byte[] buffer1 = new byte[(int)entry.CompressedSize];
            this._stream.Position = (long)entry.Offset + (long)entry.HeaderSize;
            this._stream.Read(buffer1, 0, (int)entry.CompressedSize);
            return buffer1;
        }

        public void Dispose()
        {
            this._stream.Flush();
        }

        public class MYPHeader
        {
            public byte[] FileHeader;
            public uint Unk1;
            public uint Unk2;
            public long MFTOffset;
            public uint EntriesPerMFT;
            public uint FileCount;
            public uint Unk5;
            public uint Unk6;
            public byte[] UnkPadding;

            public void Load(BinaryReader reader)
            {
                this.FileHeader = reader.ReadBytes(4);
                this.Unk1 = reader.ReadUInt32();
                this.Unk2 = reader.ReadUInt32();
                this.MFTOffset = reader.ReadInt64();
                this.EntriesPerMFT = reader.ReadUInt32();
                this.FileCount = reader.ReadUInt32();
                this.Unk5 = reader.ReadUInt32();
                this.Unk6 = reader.ReadUInt32();
                this.UnkPadding = reader.ReadBytes(476);
            }
        }

        public class MFTHeader
        {
            public long Offset;
            public uint Length;
            public long NextMFTOffset;

            public void Load(BinaryReader reader)
            {
                this.Length = reader.ReadUInt32();
                this.NextMFTOffset = reader.ReadInt64();
            }
        }

        public class MFTEntry
        {
            public byte[] Data;

            public ulong Offset { get; set; }

            public uint HeaderSize { get; set; }

            public uint CompressedSize { get; set; }

            public uint UnCompressedSize { get; set; }

            public long Hash { get; set; }

            public uint CRC32 { get; set; }

            public byte Compressed { get; set; }

            public byte Unk1 { get; set; }

            public long MFTEntryOffset { get; set; }

            public int MFTEntryIndex { get; set; }

            public bool Changed { get; set; }

            public string Name { get; set; }

            public string Path { get; set; }

            public MYP Archive { get; set; }

            public override string ToString()
            {
                if (this.Name == null)
                    return this.Hash.ToString();
                return this.Hash.ToString() + " (" + this.Name + ")";
            }

            public void Load(MYP archive, BinaryReader reader, int index)
            {
                this.Archive = archive;
                this.MFTEntryOffset = reader.BaseStream.Position;
                this.MFTEntryIndex = index;
                this.Offset = reader.ReadUInt64();
                this.HeaderSize = reader.ReadUInt32();
                this.CompressedSize = reader.ReadUInt32();
                this.UnCompressedSize = reader.ReadUInt32();
                this.Hash = reader.ReadInt64();
                this.CRC32 = reader.ReadUInt32();
                this.Compressed = reader.ReadByte();
                this.Unk1 = reader.ReadByte();
            }
        }
    }
}