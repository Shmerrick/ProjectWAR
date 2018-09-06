using Dapper.Contrib.Extensions;
using System;

namespace PatcherServer.Database
{
    [Table("file")]
    public class Patcher_File
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int CRC32 { get; set; }
        public long Size { get; set; }
        public DateTime ModifyDate { get; set; }
    }
}
