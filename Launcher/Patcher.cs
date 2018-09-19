using FrameWork;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Launcher
{
    public class Patcher
    {
        private string _address;
        public long TotalDownloadSize { get; private set; }
        public long Downloaded { get; private set; }
        public string CurrentFile { get; private set; }
        public State CurrentState { get; private set; } = State.RequestManifest;
        private Logger _logger;

        private List<FileManifestItem> _neededAssets = new List<FileManifestItem>();

        public Patcher(Logger logger, string address)
        {
            _address = address;
            _logger = logger;
        }

        public async Task Patch(string patchDirectory)
        {
            string tempFile = "";

            try
            {
                TotalDownloadSize = 0;
                Downloaded = 0;


                CurrentState = State.RequestManifest;

                _logger.Info($"Requesting files to update from {_address}");
                var manifest = await HttpUtil.Request<FileManifest>(_address, "REQUEST_FILE_MANIFEST");

                CurrentState = State.ProcessManifest;
                if (manifest == null || manifest.Files == null)
                {
                    _logger.Info($"Invalid file update list");

                    CurrentState = State.Error;
                    return;
                }
                int hash = 0;

                lock (_neededAssets)
                {
                    _logger.Info($"Processing update files. {manifest.Files.Count} files");

                    foreach (var file in manifest.Files)
                    {
                        string path = Path.Combine(Application.StartupPath+ patchDirectory, file.Name);
                        if (File.Exists(path))
                        {

                            using (var fs = File.OpenRead(path))
                            {
                                hash = (int)Utils.Adler32(fs, fs.Length);
                            }
                            if (hash != file.CRC32)
                            {
                                _logger.Info($"File is out of date (Id:{file.Id} Name:{file.Name})");
                                _neededAssets.Add(file);
                            }
                        }
                        else
                        {
                            _logger.Info($"Adding file (Id:{file.Id} Name:{file.Name})");
                            _neededAssets.Add(file);
                        }
                    }
                }

                TotalDownloadSize = _neededAssets.Sum(e => e.Size);

                if (TotalDownloadSize > 0)
                    _logger.Info($"Total request download size:{TotalDownloadSize} (Files:{_neededAssets.Count})");

                foreach (var file in _neededAssets.ToList())
                {
                    CurrentState = State.Downloading;

                    string path = Path.Combine(Application.StartupPath+ patchDirectory, file.Name);
                    if (File.Exists(path))
                        File.Delete(path);

                    tempFile = Guid.NewGuid().ToString().Replace("-", "") + ".temp";
                    CurrentFile = file.Name;

                    using (var fs = File.Create(tempFile))
                    {
                        _logger.Info($"Begin downloading file (Id:{file.Id} Name:{file.Name} Size:{file.Size})");

                        HttpUtil.RequestStream(_address, $"REQUEST_FILE?id={file.Id}", fs, (current, total, chunkSize) =>
                        {
                            Downloaded += chunkSize;
                        });

                        _logger.Info($"Finished downloading file (Id:{file.Id} Name:{file.Name} Size:{file.Size})");
                    }
                    using (var fs = File.OpenRead(tempFile))
                    {
                        hash = (int)Utils.Adler32(fs, fs.Length);
                    }

                    if (hash == file.CRC32)
                    {
                        File.Move(tempFile, path);
                    }
                    else
                    {
                        _logger.Error($"Invalid file received (Id:{file.Id} Name:{file.Name} Size:{file.Size} LocalHash:{hash} ServerHash:{hash})");
                        CurrentState = State.Error;
                        File.Delete(tempFile);
                        return;
                    }


                    lock (_neededAssets)
                        _neededAssets.Remove(file);
                }

                TotalDownloadSize = 0;
                Downloaded = 0;
                CurrentFile = "";
                CurrentState = State.Done;
                _logger.Info($"Finished downloading all files");
            }
            catch (Exception e)
            {
                _logger.Info($"Error downloading files:{e}");
                CurrentState = State.Error;
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        public int GetRemainingFiles()
        {
            lock (_neededAssets)
            {
                return _neededAssets.Count;
            }
        }

        public class FileManifestItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int CRC32 { get; set; }
            public long Size { get; set; }
        }

        public class FileManifest
        {
            public int TotalFiles { get; set; }
            public List<FileManifestItem> Files { get; set; } = new List<FileManifestItem>();
        }

        public enum State
        {
            RequestManifest,
            ProcessManifest,
            Downloading,
            Done,
            Error,
            ServerOffline,
        }
    }
}
