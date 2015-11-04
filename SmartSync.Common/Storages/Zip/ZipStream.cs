using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Common
{
    public class ZipStream : Stream
    {
        private static long totalBytesToFlush = 0;
        private const long autoflushThreshold = 100 * 1024 * 1024;

        private ZipStorage storage;
        private Stream stream;

        private long bytesToFlush = 0;

        public ZipStream(ZipStorage storage, Stream stream)
        {
            this.storage = storage;
            this.stream = stream;
        }

        public override bool CanRead
        {
            get
            {
                return stream.CanRead;
            }
        }
        public override bool CanSeek
        {
            get
            {
                return stream.CanSeek;
            }
        }
        public override bool CanWrite
        {
            get
            {
                return stream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return stream.Length;
            }
        }
        public override long Position
        {
            get
            {
                return stream.Position;
            }
            set
            {
                stream.Position = value;
            }
        }

        public override void Flush()
        {
            stream.Flush();
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            return stream.Read(buffer, offset, count);
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }
        public override void SetLength(long value)
        {
            bytesToFlush += Math.Abs(Length - value);
            stream.SetLength(value);
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            stream.Write(buffer, offset, count);
            bytesToFlush += count;
        }

        protected override void Dispose(bool disposing)
        {
            stream.Dispose();

            totalBytesToFlush += bytesToFlush;
            if (totalBytesToFlush > autoflushThreshold)
            {
                storage.Flush();
                totalBytesToFlush = 0;
            }
        }
    }
}