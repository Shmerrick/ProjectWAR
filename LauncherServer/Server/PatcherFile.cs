using System;
using System.IO;
using System.IO.Compression;
using FrameWork;

namespace AuthenticationServer.Server
{
    public class PatcherFile : IDisposable
    {
        private UInt32 _fileHash;
        private UInt64 _fileSize;
        private UInt64 _compressedSize;
        private PatchAsset _asset;
        private UInt64 _written;
        private string _filename;
        public uint GmLevel;
        private string _destination;
        private string _compressedTempFileName;
        private byte[] _data;
        private FileStream _file;
        private FileStream _compressedTempFile;
        private FileCompressionMode _compress;
        private FileType _type;
        public Archive Archive;
        private ulong _filenameHash;
        private uint _oldCrc;
        private bool _closed = true;

        public bool Closed
        {
            get
            {
                return _closed;
            }
        }
        public int ArchiveID
        {
            get
            {
                if (_asset != null)
                    return _asset.ArchiveId;
                return 0;
            }
        }

        public ulong FilenameHash
        {
            get
            {
                if (_asset != null)
                    return _asset.Hash;
                return _filenameHash;
            }
            set
            {
                _filenameHash = value;
            }
        }
        public UInt32 OldCrc
        {
            get
            {
                return _oldCrc;
            }
            set
            {
                _oldCrc = value;
            }
        }

        public UInt64 Written
        {
            get
            {
                return _written;
            }
        }

        public uint FileHash
        {
            get
            {
                return _fileHash;
            }
        }

        public ulong FileSize
        {
            get
            {
                return _fileSize;
            }
        }

        public ulong CompressedSize
        {
            get
            {
                return _compressedSize;
            }
        }

        public string Destination
        {
            get
            {
                return _destination;
            }
        }
        
        public string Filename
        {
            get
            {
                return _filename;
            }
        }

        public FileType Type
        {
            get
            {
                return _type;
            }
        }

        public byte[] Data
        {
            get
            {
                return _data;
            }
        }

        public FileCompressionMode Compress
        {
            get
            {
                return _compress;
            }
        }

        public PatcherFile()
        {
        }


        /// <summary>
        /// Creates in memory file that is being uploaded to client
        /// </summary>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <param name="destination"></param>
        /// <param name="compress"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool CreateUpload(PatchAsset asset, byte[] data, FileCompressionMode compress, FileType type)
        {
            Close();
            _fileHash = Utils.Adler32(0, data, (ulong)data.Length);
            _filename = asset.Name;
            _fileSize = (ulong)data.Length;
            _filenameHash = asset.Hash;
            Archive = (Archive)asset.ArchiveId;
            _asset = asset;
            _compressedSize = _fileSize;
            _oldCrc = asset.CRC32;
            _compress = compress;
            _type = type;
            _closed = false;
            if (Compress == FileCompressionMode.WHOLE)
            {
                MemoryStream ms = new MemoryStream();
                BinaryWriter gzipWriter = new BinaryWriter(ms);

                //write DEFLATE header
                gzipWriter.Write((byte)0x78);
                gzipWriter.Write((byte)0x9C);

                using (DeflateStream compressStream = new DeflateStream(ms, CompressionMode.Compress, true))
                    compressStream.Write(data, 0, (int)_fileSize);

                _compressedSize = (ulong)ms.Length;
                _data = ms.ToArray();
            }
            else
            {
                _data = new byte[_fileSize];
                Buffer.BlockCopy(data, 0, _data, 0, (int)_fileSize);
            }
            return true;
        }

        public bool CreateUpload(string destination, byte[] data, FileCompressionMode compress, FileType type)
        {
            Close();
            _fileHash = Utils.Adler32(0, data, (ulong)data.Length);
            _filename = destination;
            _fileSize = (ulong)data.Length;

            _compressedSize = _fileSize;
            _compress = compress;
            _type = type;
            _closed = false;
            if (Compress == FileCompressionMode.WHOLE)
            {
                MemoryStream ms = new MemoryStream();
                BinaryWriter gzipWriter = new BinaryWriter(ms);

                //write DEFLATE header
                gzipWriter.Write((byte)0x78);
                gzipWriter.Write((byte)0x9C);

                using (DeflateStream compressStream = new DeflateStream(ms, CompressionMode.Compress, true))
                    compressStream.Write(data, 0, (int)_fileSize);

                _compressedSize = (ulong)ms.Length;
                _data = ms.ToArray();
            }
            else
            {
                _data = new byte[_fileSize];
                Buffer.BlockCopy(data, 0, _data, 0, (int)_fileSize);
            }
            return true;
        }

        /// <summary>
        /// Creates file that is being uploaded to client from harddrive
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="destination"></param>
        /// <param name="compress"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool CreateUpload(string filename, string destination, FileCompressionMode compress, FileType type, uint hash = 0)
        {
            Close();
            _closed = false;
            _file = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (hash == 0)
                _fileHash = Utils.Adler32(_file, _file.Length);
            else
                _fileHash = hash;
            _filename = destination;
            _fileSize = (ulong)_file.Length;
            _destination = destination;
            _compressedSize = _fileSize;
            _compress = compress;
            _type = type;

            if (Compress == FileCompressionMode.WHOLE)
            {
                _compressedTempFileName = Path.Combine(Program.Config.TempFilesPath, "tempu_" + DateTime.Now.ToFileTimeUtc().ToString() + ".part");
                if (File.Exists(_compressedTempFileName))
                    File.Delete(_compressedTempFileName);

                _compressedTempFile = new FileStream(_compressedTempFileName, FileMode.Create, FileAccess.ReadWrite);

                BinaryWriter gzipWriter = new BinaryWriter(_compressedTempFile);

                //write DEFLATE header
                gzipWriter.Write((byte)0x78);
                gzipWriter.Write((byte)0x9C);

                using (DeflateStream compressStream = new DeflateStream(_compressedTempFile, CompressionMode.Compress, true))
                    _file.CopyTo(compressStream);

                //      gzipWriter.Write(_fileHash); //write adler32
                gzipWriter.Flush();
                _compressedSize = (ulong)_compressedTempFile.Length;

                _file.Close();
                _file = _compressedTempFile;
            }
            return true;
        }

        public bool Write(byte[] data, ulong offset, uint size)
        {
            if (offset + size > _compressedSize) //attempting to write beyond limits of the file
                return false;

            if (_compressedTempFile != null)
            {
                _compressedTempFile.Position = (long)offset;
                _compressedTempFile.Write(data, (int)0, (int)size);
            }
            else if (_data != null)
            {
                Buffer.BlockCopy(data, 0, _data, (int)offset, (int)size);
            }
            else
            {
                return false;
            }

            _written += size;

            return true;
        }

        public long Read(byte[] dest, long offset, long size)
        {
            FileStream sendFile = null;

            if (_file != null)
                sendFile = _file;
            if (_compressedTempFile != null)
                sendFile = _compressedTempFile;

            if (sendFile != null)
                sendFile.Position = (long)offset;


            if (offset + size > (long)_compressedSize)
            {
                size = (long)_compressedSize - offset;
                if (size < 0)
                    return 0;
            }

            if (sendFile != null)
            {
                int r = sendFile.Read(dest, (int)0, (int)size);
                if (r < 0)
                    return 0;
            }
            else
            {
                Buffer.BlockCopy(_data, (int)offset, dest, (int)0, (int)size);
            }
            return size;
        }

        public void Close()
        {
            _closed = true;
            if (_file != null)
            {
                _file.Close();
                _file = null;
            }

            if (_written != 0 && _written < _compressedSize)
            {
                System.Console.WriteLine("Closing unfinished file!");
            }
            if (_data != null)
                _data = null;

            _written = 0;
            if (_compressedTempFile != null)
            {
                _compressedTempFile.Close();
                _compressedTempFile = null;
            }
            if (_compressedTempFileName != "")
            {
                if (File.Exists(_compressedTempFileName))
                    File.Delete(_compressedTempFileName);

                _compressedTempFileName = "";
            }
        }

        public void Dispose()
        {
            Close();
        }

        public string ToText()
        {
            if (_data != null)
                return System.Text.ASCIIEncoding.ASCII.GetString(_data);
            else if (File.Exists(_destination))
                return File.ReadAllText(_destination);
            return "";
        }
    }
}
