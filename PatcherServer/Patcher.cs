using Microsoft.AspNetCore.Http;
using PatcherServer.Config;
using PatcherServer.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PatcherServer
{
    public class Patcher : HttpServerBase
    {
        private Dictionary<int, Patcher_File> _files = new Dictionary<int, Patcher_File>();
        private PatcherConfig Config;
        private PatcherDB _db;

        public Patcher(PatcherConfig config, PatcherDB db)
        {
            Config = config;
            _db = db;
        }

        [HttpRoute("REQUEST_FILE_MANIFEST")]
        public Task REQUEST_FILE_MANIFEST(HttpContext context)
        {
            return ResponseJson(context, new FileManifest()
            {
                TotalFiles = _files.Count,
                Files = _files.Values.Select(e => new FileManifestItem()
                {
                    CRC32 = e.CRC32,
                    Id = e.Id,
                    Name = e.Name,
                    Size = e.Size
                }).ToList()
            });
        }

        [HttpRoute("REQUEST_FILE")]
        public Task REQUEST_FILE(HttpContext context, int id)
        {
            context.Response.Headers.Add("ResponseType", "application/octet-stream");

            if (!_files.ContainsKey(id))
                return ResponseError(context);

            var fileRecord = _files[id];

            string path = Path.Combine(Config.PatcherFilesPath, fileRecord.Name);

            if (!File.Exists(path))
                return ResponseError(context);

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                context.Response.ContentLength = fs.Length;
                fs.CopyTo(context.Response.Body);
            }

            return Task.CompletedTask;
        }

        protected override async Task Initialize()
        {
            await UpdateFileRecords();
        }

        private async Task UpdateFileRecords()
        {
            DirectoryInfo d = new DirectoryInfo(Config.PatcherFilesPath);
#if DEBUG
            if (!d.Exists)
            {
                Console.WriteLine("No patcher files loaded. (Patcher folder not found)");
                return;
            }
#endif
            var dbFiles = (await _db.GetFiles()).ToDictionary(e => e.Id, e => e);
            _files.Clear();

            var files = d.GetFiles("*", SearchOption.AllDirectories).Where(e => !e.FullName.Contains(".git")).ToArray();
            Console.WriteLine("Loading patcher files info");

            foreach (var file in files)
            {
                string folder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).ToLower(), Config.PatcherFilesPath);

                string relativePath = Path.GetRelativePath(folder, file.FullName);
                Patcher_File existing = dbFiles.Values.Where(e => e.Name == relativePath).FirstOrDefault();

                using (var fs = File.OpenRead(file.FullName))
                {
                    if (existing != null)
                    {
                        int crc = (int)Utils.Adler32(fs, fs.Length);
                        if (crc != existing.CRC32)
                        {
                            Console.WriteLine($"Updating patcher file {relativePath}");

                            existing.Size = file.Length;
                            existing.CRC32 = crc;
                            existing.ModifyDate = DateTime.UtcNow;
                            await _db.Save(existing);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Adding patcher file {relativePath}");
                        existing = new Patcher_File()
                        {
                            Name = relativePath,
                            Size = file.Length,
                            CRC32 = (int)Utils.Adler32(fs, fs.Length),
                            ModifyDate = DateTime.UtcNow,

                        };
                        await _db.Add(existing);
                    }

                    _files[existing.Id] = existing;
                }
            }
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
}
