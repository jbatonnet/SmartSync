using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Drive.v2;

namespace SmartSync.GoogleDrive
{
    public class GoogleDriveStream : Stream
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

        private GoogleDriveStorage storage;
        internal Google.Apis.Drive.v2.Data.File file;

        private MemoryStream dataStream = new MemoryStream();
        private bool written = false;

        public GoogleDriveStream(GoogleDriveStorage storage, Google.Apis.Drive.v2.Data.File file)
        {
            this.storage = storage;
            this.file = file;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(file.DownloadUrl));
            request.Headers.Add("Authorization", storage.Credential.Token.TokenType + " " + storage.Credential.Token.AccessToken);
            
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("Could not retrieve file stream");

            using (Stream responseStream = response.GetResponseStream())
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

                FilesResource.UpdateMediaUpload request = storage.Service.Files.Update(file, file.Id, dataStream, "application/octet-stream");
                request.NewRevision = storage.UseVersioning;

                request.Upload();
                
            }

            dataStream.Dispose();
        }
    }
}