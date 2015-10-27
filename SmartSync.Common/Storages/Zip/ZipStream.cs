using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ionic.Zip;

namespace SmartSync.Common
{
    public class ZipStream : Stream
    {
        public override bool CanRead
        {
            get
            {
                return access == FileAccess.Read || access == FileAccess.ReadWrite;
            }
        }
        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }
        public override bool CanWrite
        {
            get
            {
                return access == FileAccess.Write || access == FileAccess.ReadWrite;
            }
        }
        public override long Length
        {
            get
            {
                return memoryStream.Length;
            }
        }
        public override long Position
        {
            get
            {
                return memoryStream.Position;
            }
            set
            {
                memoryStream.Position = value;
            }
        }

        private Ionic.Zip.ZipFile zip;
        private ZipEntry entry;
        private FileAccess access;

        private MemoryStream memoryStream;
        
        public ZipStream(Ionic.Zip.ZipFile zip, ZipEntry entry, FileAccess access)
        {
            this.zip = zip;
            this.entry = entry;
            this.access = access;

            memoryStream = new MemoryStream();
            
            if (CanRead)
            {
                using (Stream readerStream = entry.OpenReader())
                    readerStream.CopyTo(memoryStream);
            }
        }

        public override void Flush()
        {
            memoryStream.Flush();
            zip.UpdateEntry(entry.FileName, memoryStream.ToArray());
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            return memoryStream.Seek(offset, origin);
        }
        public override void SetLength(long value)
        {
            memoryStream.SetLength(value);
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            return memoryStream.Read(buffer, offset, count);
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            memoryStream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            zip.UpdateEntry(entry.FileName, memoryStream.ToArray());
            memoryStream.Dispose();
        }
    }
}