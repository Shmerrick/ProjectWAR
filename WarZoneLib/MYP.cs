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
        ART2,
        ART3,
        AUDIO,
        DATA,
        INTERFACE,
        MFT,
        PATCH,
        VO_ENGLISH,
        VO_FRENCH,
        WARTEST,
        WORLD
    }

    public class MYP : IDisposable
    {
        public class MYPHeader
        {
            public byte[] FileHeader;//MYP\0
            public uint Unk1;  //5
            public uint Unk2; //0xfd23ec43 constant must contain this value
            public long MFTOffset;
            public uint EntriesPerMFT; //entries count in first mft link (1000)
            public uint FileCount;  //total count of all files in all mft links
            public uint Unk5; //1 or 5
            public uint Unk6; //1 or 5
            public byte[] UnkPadding;

            public void Load(BinaryReader reader)
            {
                FileHeader = reader.ReadBytes(4);
                Unk1 = reader.ReadUInt32();
                Unk2 = reader.ReadUInt32();
                MFTOffset = reader.ReadInt64();
                EntriesPerMFT = reader.ReadUInt32();
                FileCount = reader.ReadUInt32();
                Unk5 = reader.ReadUInt32();
                Unk6 = reader.ReadUInt32();
                UnkPadding = reader.ReadBytes(0x1DC);
            }
        }

        public class MFTHeader
        {
            public long Offset;
            public uint Length;
            public long NextMFTOffset;

            public void Load(BinaryReader reader)
            {
                Length = reader.ReadUInt32();
                NextMFTOffset = reader.ReadInt64();
            }
        }
        public static string GetExtension(byte[] buffer)
        {
            byte[] outbuffer = buffer;

            string file = "";

            file = System.Text.Encoding.ASCII.GetString(outbuffer, 0, outbuffer.Length);
            char[] separators = { ',' };
            int commaNum = file.Split(separators, 10).Length;
            string header = System.Text.Encoding.ASCII.GetString(outbuffer, 0, 4);
            string ext = "txt";

            if (outbuffer[0] == 0 && outbuffer[1] == 1 && outbuffer[2] == 0)
            {
                ext = "ttf";
            }
            else if (outbuffer[0] == 0x0a && outbuffer[1] == 0x05 && outbuffer[2] == 0x01 && outbuffer[3] == 0x08)
            {
                ext = "pcx";
            }
            else if (header.IndexOf("PK") >= 0)
            {
                ext = "zip";
            }
            else if (header.IndexOf("SCPT") >= 0)
            {
                ext = "scpt";
            }
            else if (header.IndexOf("<") >= 0)
            {
                ext = "xml";
            }
            else if (file.IndexOf("lua") >= 0 && file.IndexOf("lua") < 50)
            {
                ext = "lua";
            }
            else if (header.IndexOf("DDS") >= 0)
            {
                ext = "dds";
            }
            else if (header.IndexOf("XSM") >= 0)
            {
                ext = "xsm";
            }
            else if (header.IndexOf("XAC") >= 0)
            {
                ext = "xac";
            }
            else if (header.IndexOf("8BPS") >= 0)
            {
                ext = "8bps";
            }
            else if (header.IndexOf("bdLF") >= 0)
            {
                ext = "db";
            }
            else if (header.IndexOf("gsLF") >= 0)
            {
                ext = "geom";
            }
            else if (header.IndexOf("idLF") >= 0)
            {
                ext = "diffuse";
            }
            else if (header.IndexOf("psLF") >= 0)
            {
                ext = "specular";
            }
            else if (header.IndexOf("amLF") >= 0)
            {
                ext = "mask";
            }
            else if (header.IndexOf("ntLF") >= 0)
            {
                ext = "tint";
            }
            else if (header.IndexOf("lgLF") >= 0)
            {
                ext = "glow";
            }
            else if (file.IndexOf("Gamebry") >= 0)
            {
                ext = "nif";
            }
            else if (file.IndexOf("WMPHOTO") >= 0)
            {
                ext = "lmp";
            }
            else if (header.IndexOf("RIFF") >= 0)
            {
                string data = System.Text.Encoding.ASCII.GetString(outbuffer, 8, 4);
                if (data.IndexOf("WAVE") >= 0)
                {
                    ext = "wav";
                }
                else
                {
                    ext = "riff";
                }
            }
            else if (header.IndexOf("; Zo") >= 0)
            {
                ext = "zone.txt";
            }
            else if (header.IndexOf("\0\0\0\0") >= 0)
            {
                ext = "zero.txt";
            }
            else if (header.IndexOf("PNG") >= 0)
            {
                ext = "png";
            }
            else if (header.IndexOf("AMX") >= 0)
            {
                ext = "amx";
            }
            else if (header.IndexOf("SIDS") >= 0)
            {
                ext = "sids";
            } //SIDS
            else if (commaNum >= 10)
            {
                ext = "csv";
            }

            return ext;
        }

        public class MFTEntry
        {
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
            public byte[] Data;
            public bool Changed { get; set; }
            public string Name { get; set; }
            public string Path { get; set; }
            public MYP Archive { get; set; }

            public override string ToString()
            {
                if (Name != null)
                    return Hash + " (" + Name + ")";
                return Hash.ToString();
            }

            public void Load(MYP archive, BinaryReader reader, int index)
            {
                Archive = archive;
                MFTEntryOffset = reader.BaseStream.Position;
                MFTEntryIndex = index;

                Offset = reader.ReadUInt64();
                HeaderSize = reader.ReadUInt32();
                CompressedSize = reader.ReadUInt32();
                UnCompressedSize = reader.ReadUInt32();
                Hash = reader.ReadInt64();
                CRC32 = reader.ReadUInt32();
                Compressed = reader.ReadByte();
                Unk1 = reader.ReadByte();
            }
        }

        public MYPHeader Header;
        public MFTHeader MFT; //mythic file table

        public Dictionary<long, MFTEntry> Enteries = new Dictionary<long, MFTEntry>();
        public List<MFTHeader> MFTTables = new List<MFTHeader>();
        private Stream _stream;
        public MythicPackage Package;

        public MYP(MythicPackage package, Stream stream)
        {
            _stream = stream;
            Package = package;
            BinaryReader reader = new BinaryReader(stream);
            Header = new MYPHeader();
            Header.Load(reader);

            MFTTables.Add(new MFTHeader()
            {
                Length = Header.EntriesPerMFT,
                NextMFTOffset = Header.MFTOffset
            });

            var mftOffset = Header.MFTOffset;
            var mftEntryCount = Header.EntriesPerMFT;

            reader.BaseStream.Position = mftOffset;

            while (mftOffset > 0)
            {
                reader.BaseStream.Position = mftOffset;
                MFT = new MFTHeader();
                MFT.Offset = reader.BaseStream.Position;
                MFT.Load(reader);

                MFTTables.Add(MFT);

                mftOffset = MFT.NextMFTOffset;
                int entryIndex = 0;

                while (entryIndex < mftEntryCount)
                {
                    var entry = new MFTEntry();
                    entry.Changed = false;
                    entry.Load(this, reader, entryIndex);

                    if (entry.Hash != 0)
                        Enteries[entry.Hash] = entry;

                    entryIndex++;
                }
                mftEntryCount = MFT.Length;
            }
        }

        public void UpdateFile(string name, byte[] data)
        {
            UpdateFile(HashWAR(name), data);
        }

        public byte[] ReadFileHeader(long hash)
        {
            if (Enteries.ContainsKey(hash))
            {
                _stream.Position = (long)Enteries[hash].Offset;
                byte[] data = new byte[Enteries[hash].HeaderSize];
                _stream.Read(data, 0, (int)Enteries[hash].HeaderSize);
                return data;
            }
            return null;
        }

        public void UpdateFile(long hash, byte[] data)
        {
            if (Enteries.ContainsKey(hash))
            {
                Enteries[hash].Changed = true;
                Enteries[hash].Data = data;
            }
            else
            {
                Enteries[hash] = new MFTEntry()
                {
                    Changed = true,
                    Data = data,
                    Compressed = 1,
                    Hash = hash
                };
            }
        }

        public void Save()
        {
            BinaryWriter writer = new BinaryWriter(_stream);

            //update existing record
            foreach (var entry in Enteries.Where(e => e.Value.Changed == true && e.Value.Offset != 0).Select(e => e.Value).ToList())
            {
                var data = entry.Data;

                if (entry.Compressed == 1)
                {
                    MemoryStream ms = new MemoryStream();

                    using (DeflateStream compressStream = new DeflateStream(ms, CompressionMode.Compress, true))
                        compressStream.Write(entry.Data, 0, entry.Data.Length);

                    data = ms.ToArray();
                }

                //file is bigger then original record, move to end and update mft table
                if (data.Length > entry.CompressedSize)
                {
                    //update MFT entry
                    _stream.Position = entry.MFTEntryOffset;
                    writer.Write((ulong)_stream.Length); //position of new record will be starting at end of file
                    writer.Write(entry.HeaderSize);

                    if (entry.Compressed == 1)
                        writer.Write((uint)(data.Length + 2)); //compressed size + DEFLATE header length
                    else
                        writer.Write((uint)(data.Length)); //uncomprssed size

                    writer.Write((uint)(entry.Data.Length));
                    _stream.Position = entry.MFTEntryOffset + 28;
                    writer.Write(CRC32.ComputeChecksum(data)); //bad, but who cares, client will refresh cache based on difference

                    //write contents at end of file
                    _stream.Position = _stream.Length;
                    entry.Offset = (ulong)_stream.Length;
                    writer.Write(ReadFileHeader(entry.Hash));
                    if (entry.Compressed == 1)
                        writer.Write((ushort)0x9C78); //DEFLATE header

                    writer.Write(data);
                }
                else
                {
                    _stream.Position = entry.MFTEntryOffset;
                    writer.Write(entry.Offset);
                    writer.Write(entry.HeaderSize);
                    if (entry.Compressed == 1)
                        writer.Write((uint)(data.Length + 2)); //compressed size + DEFLATE header length
                    else
                        writer.Write((uint)(data.Length)); //uncomprssed size

                    writer.Write((uint)(entry.Data.Length));

                    _stream.Position = entry.MFTEntryOffset + 28;
                    writer.Write(CRC32.ComputeChecksum(data)); //bad, but who cares, client will refresh based difference

                    if (entry.Compressed == 1)
                        _stream.Position = (long)(entry.Offset + entry.HeaderSize + 2);
                    else
                        _stream.Position = (long)(entry.Offset + entry.HeaderSize);
                    //write contents at end of file
                    writer.Write(data);
                }
            }

            var newEntries = Enteries.Where(e => e.Value.Offset == 0).Select(e => e.Value).ToList();
            if (newEntries.Count > 0)
            {
                //update total file count in myp header
                _stream.Position = 0x18;
                writer.Write((uint)Enteries.Count + newEntries.Count);

                //update last MFT entry to point to next
                if (MFTTables.Count > 0)
                {
                    _stream.Position = MFTTables.Last().Offset + 4;
                    writer.Write((ulong)writer.BaseStream.Length);
                }

                //move to end of the archive and add new MFT entry
                writer.BaseStream.Position = writer.BaseStream.Length;

                var packed = 0;
                var toPack = new List<MFTEntry>();
                int remaining = newEntries.Count;
                long nextMftPos = 0;
                while (remaining > 0)
                {

                    writer.Write((uint)1000);//1000 entries per mft
                    if (newEntries.Count - packed > 1000) //another MFT entry after this?
                    {

                        nextMftPos = _stream.Position;
                        writer.Write((long)0); //next mft position
                    }
                    else
                    {
                        writer.Write((long)0);
                    }

                    for (int i = 0; i < 1000; i++)
                    {
                        //every 1000 files, write out MFT header, and dump files after it
                        if (packed < newEntries.Count)
                        {
                            remaining--;
                            var entry = newEntries[packed];

                            MemoryStream ms = new MemoryStream();

                            using (DeflateStream compressStream = new DeflateStream(ms, CompressionMode.Compress, true))
                            {
                                compressStream.Write(entry.Data, 0, (int)entry.Data.Length);
                            }
                            var data = ms.ToArray();

                            //entry.Data[0] = 0x78;
                            //entry.Data[1] = 0x9C;



                            entry.MFTEntryOffset = _stream.Position;
                            writer.Write((long)0); //will determine data offset on second pass
                            writer.Write((uint)entry.HeaderSize);
                            writer.Write((uint)entry.CompressedSize); //compressed size
                            writer.Write((uint)entry.UnCompressedSize); //uncompressed size
                            writer.Write((long)entry.Hash);

                            writer.Write((uint)entry.CRC32); //CRC
                            writer.Write((byte)entry.Compressed);//compress
                            writer.Write((byte)0);//unknown
                            toPack.Add(entry);
                            packed++;
                        }
                        else
                        {
                            writer.Write((long)0); //offset
                            writer.Write((uint)0); //headersize
                            writer.Write((uint)0); //compressed size
                            writer.Write((uint)0); //uncompressed size
                            writer.Write((long)0); //hash
                            writer.Write((uint)0); //CRC
                            writer.Write((byte)0); //compression
                            writer.Write((byte)0); //unknown
                        }
                    }

                    int packing = 0;

                    foreach (var entry in toPack)
                    {

                        var pos = _stream.Position;
                        _stream.Position = entry.MFTEntryOffset;
                        writer.Write((long)pos);
                        _stream.Position = pos;

                        byte[] data = entry.Data;

                        _stream.Write(data, 0, data.Length);

                        if (entry.Compressed == 1)
                        {
                            var output_buffer = new byte[entry.UnCompressedSize];

                            MemoryStream ms = new MemoryStream(data);
                            ms.Position = (long)(entry.HeaderSize + 2);
                            using (DeflateStream decompressionStream = new DeflateStream(ms, CompressionMode.Decompress, true))
                            {
                                decompressionStream.Read(output_buffer, 0, output_buffer.Length);
                            }
                        }
                        packing++;
                    }


                    //if (packed < files.Count)
                    //{
                    //    var lastPos = _stream.Position;
                    //    _stream.Position = nextMftPos;
                    //    writer.Write((long)lastPos); //next mft position
                    //    _stream.Position = lastPos;
                    //}

                    toPack.Clear();
                }
            }
        }

        public void AddEntries(List<Tuple<string, byte[]>> files)
        {
            _stream.Position = 0x18;
            BinaryWriter writer = new BinaryWriter(_stream);
            //update total file count in myp header
            writer.Write((uint)Enteries.Count + files.Count);


            //update last MFT entry to point to next
            if (MFTTables.Count > 0)
            {
                _stream.Position = MFTTables.Last().Offset + 4;
                writer.Write((ulong)writer.BaseStream.Length);
            }

            //move to end of the archive and add new MFT entry
            writer.BaseStream.Position = writer.BaseStream.Length;

            var packed = 0;
            var toPack = new List<MFTEntry>();
            int remaining = files.Count;
            long nextMftPos = 0;
            while (remaining > 0)
            {

                writer.Write((uint)1000);//1000 entries per mft
                if (files.Count - packed > 1000) //another MFT entry after this?
                {

                    nextMftPos = _stream.Position;
                    writer.Write((long)0); //next mft position
                }
                else
                {
                    writer.Write((long)0);
                }

                for (int i = 0; i < 1000; i++)
                {
                    //every 1000 files, write out MFT header, and dump files after it
                    if (packed < files.Count)
                    {
                        remaining--;
                        var entry = new MFTEntry();

                        MemoryStream ms = new MemoryStream();

                        using (DeflateStream compressStream = new DeflateStream(ms, CompressionMode.Compress, true))
                        {
                            compressStream.Write(files[packed].Item2, 0, files[packed].Item2.Length);
                        }

                        entry.Data = new byte[ms.Length + 2];
                        entry.Data[0] = 0x78;
                        entry.Data[1] = 0x9C;
                        Buffer.BlockCopy(ms.ToArray(), 0, entry.Data, 2, (int)ms.Length);
                        entry.CompressedSize = (uint)entry.Data.Length;
                        entry.Hash = HashWAR(files[packed].Item1);
                        entry.CRC32 = CRC32.ComputeChecksum(files[packed].Item2);
                        entry.Compressed = 1;
                        entry.UnCompressedSize = (uint)files[packed].Item2.Length;

                        entry.MFTEntryOffset = _stream.Position;
                        writer.Write((long)0); //will determine data offset on second pass
                        writer.Write((uint)entry.HeaderSize);
                        writer.Write((uint)entry.CompressedSize); //compressed size
                        writer.Write((uint)entry.UnCompressedSize); //uncompressed size
                        writer.Write((long)entry.Hash);

                        writer.Write((uint)entry.CRC32); //CRC
                        writer.Write((byte)entry.Compressed);//compress
                        writer.Write((byte)0);//unknown
                        toPack.Add(entry);
                        packed++;
                    }
                    else
                    {
                        writer.Write((long)0); //offset
                        writer.Write((uint)0); //headersize
                        writer.Write((uint)0); //compressed size
                        writer.Write((uint)0); //uncompressed size
                        writer.Write((long)0); //hash
                        writer.Write((uint)0); //CRC
                        writer.Write((byte)0); //compression
                        writer.Write((byte)0); //unknown
                    }
                }

                int packing = 0;

                foreach (var entry in toPack)
                {

                    var pos = _stream.Position;
                    _stream.Position = entry.MFTEntryOffset;
                    writer.Write((long)pos);
                    _stream.Position = pos;

                    byte[] data = entry.Data;

                    _stream.Write(data, 0, data.Length);

                    if (entry.Compressed == 1)
                    {
                        var output_buffer = new byte[entry.UnCompressedSize];

                        MemoryStream ms = new MemoryStream(data);
                        ms.Position = (long)(entry.HeaderSize + 2);
                        using (DeflateStream decompressionStream = new DeflateStream(ms, CompressionMode.Decompress, true))
                        {
                            decompressionStream.Read(output_buffer, 0, output_buffer.Length);
                        }
                    }
                    packing++;
                }


                if (packed < files.Count)
                {
                    var lastPos = _stream.Position;
                    _stream.Position = nextMftPos;
                    writer.Write((long)lastPos); //next mft position
                    _stream.Position = lastPos;
                }

                toPack.Clear();
            }
        }

        public static long HashWAR(string s)
        {
            uint ph = 0, sh = 0;
            HashWAR(s, 0xDEADBEEF, out ph, out sh);
            return ((long)ph << 32) + sh;
        }

        public static void HashWAR(string s, uint seed, out uint ph, out uint sh)
        {
            uint edx = 0, eax, esi, ebx = 0;
            uint edi, ecx;


            eax = ecx = edx = ebx = esi = edi = 0;
            ebx = edi = esi = (uint)s.Length + seed;

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

                ph = edi;
                sh = eax;
                return;
            }
            ph = esi;
            sh = eax;
            return;
        }

        public byte[] ReadFile(string name)
        {
            var hash = HashWAR(name);
            if (Enteries.ContainsKey(hash))
                return ReadFile(Enteries[hash]);

            return null;
        }


        public byte[] ReadFile(MFTEntry entry)
        {
            if (entry.Compressed == 1)
            {
                byte[] output_buffer = new byte[entry.UnCompressedSize];

                _stream.Position = (long)(entry.Offset + entry.HeaderSize + 2);
                MemoryStream ms = new MemoryStream(output_buffer);
                using (DeflateStream decompressionStream = new DeflateStream(_stream, CompressionMode.Decompress, true))
                    decompressionStream.Read(output_buffer, 0, output_buffer.Length);

                return output_buffer;
            }
            else
            {
                byte[] output_buffer = new byte[entry.CompressedSize];
                _stream.Position = (long)(entry.Offset + entry.HeaderSize);
                _stream.Read(output_buffer, 0, (int)entry.CompressedSize);
                return output_buffer;
            }
        }

        public void Dispose()
        {
            _stream.Flush();
        }
    }
    public class CRC32
    {
        static uint[] table;

        public static uint ComputeChecksum(string text)
        {
            if (text == null || text.Length == 0)
                return 0;

            return ComputeChecksum(System.Text.ASCIIEncoding.ASCII.GetBytes(text));
        }

        public static uint ComputeChecksum(byte[] bytes)
        {
            if (table == null)
            {
                uint poly = 0xedb88320;
                table = new uint[256];
                uint temp = 0;
                for (uint i = 0; i < table.Length; ++i)
                {
                    temp = i;
                    for (int j = 8; j > 0; --j)
                    {
                        if ((temp & 1) == 1)
                        {
                            temp = (uint)((temp >> 1) ^ poly);
                        }
                        else
                        {
                            temp >>= 1;
                        }
                    }
                    table[i] = temp;
                }
            }

            uint crc = 0xffffffff;
            for (int i = 0; i < bytes.Length; ++i)
            {
                byte index = (byte)(((crc) & 0xff) ^ bytes[i]);
                crc = (uint)((crc >> 8) ^ table[index]);
            }
            return ~crc;
        }
    }
}
