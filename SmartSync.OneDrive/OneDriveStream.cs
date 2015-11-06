using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Microsoft.OneDrive.Sdk;

namespace SmartSync.OneDrive
{
    public class OneDriveStream : Stream
    {
        public override bool CanRead
        {
            get
            {
                return true;
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
                return true;
            }
        }
        public override long Length
        {
            get
            {
                return dataStream.Length;
            }
        }
        public override long Position
        {
            get
            {
                return dataStream.Position;
            }
            set
            {
                dataStream.Position = value;
            }
        }

        private OneDriveStorage storage;
        internal Item file;

        private MemoryStream dataStream = new MemoryStream();
        private bool written = false;

        public OneDriveStream(OneDriveStorage storage, Item file)
        {
            this.storage = storage;
            this.file = file;

            Task<Stream> task = storage.Client.Drive.Items[file.Id].Content.Request().GetAsync();
            using (Stream responseStream = task.Result)
                responseStream.CopyTo(dataStream);

            dataStream.Seek(0, SeekOrigin.Begin);
        }
        
        public override void Flush()
        {
            dataStream.Flush();
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            return dataStream.Read(buffer, offset, count);
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            return dataStream.Seek(offset, origin);
        }
        public override void SetLength(long value)
        {
            dataStream.SetLength(value);
            written = true;
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            dataStream.Write(buffer, offset, count);
            written = true;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (written)
            {
                dataStream.Seek(0, SeekOrigin.Begin);

                Task<Item> task = storage.Client.Drive.Items[file.Id].Content.Request().PutAsync<Item>(dataStream);
                task.Wait();
            }

            dataStream.Dispose();
        }
    }
}