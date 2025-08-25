using System;
using System.IO;
using FrameWork;
using Xunit;

namespace LauncherServer.Tests
{
    public class ZOutputStreamTests
    {
        [Fact]
        public void Read_Throws_NotSupported()
        {
            using var stream = new ZOutputStream(new MemoryStream());
            Assert.Throws<NotSupportedException>(() => stream.Read(Array.Empty<byte>(), 0, 0));
        }

        [Fact]
        public void Seek_Throws_NotSupported()
        {
            using var stream = new ZOutputStream(new MemoryStream());
            Assert.Throws<NotSupportedException>(() => stream.Seek(0, SeekOrigin.Begin));
        }

        [Fact]
        public void SetLength_Throws_NotSupported()
        {
            using var stream = new ZOutputStream(new MemoryStream());
            Assert.Throws<NotSupportedException>(() => stream.SetLength(0));
        }

        [Fact]
        public void Length_Throws_NotSupported()
        {
            using var stream = new ZOutputStream(new MemoryStream());
            Assert.Throws<NotSupportedException>(() => { var _ = stream.Length; });
        }

        [Fact]
        public void Position_Get_Throws_NotSupported()
        {
            using var stream = new ZOutputStream(new MemoryStream());
            Assert.Throws<NotSupportedException>(() => { var _ = stream.Position; });
        }

        [Fact]
        public void Position_Set_Throws_NotSupported()
        {
            using var stream = new ZOutputStream(new MemoryStream());
            Assert.Throws<NotSupportedException>(() => stream.Position = 0);
        }

        [Fact]
        public void CanProperties_ReportCorrectCapabilities()
        {
            using var stream = new ZOutputStream(new MemoryStream());
            Assert.False(stream.CanRead);
            Assert.True(stream.CanWrite);
            Assert.False(stream.CanSeek);
        }
    }
}
