using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartSync.Common;

using Google.Apis.Drive.v2;

namespace SmartSync.GoogleDrive
{
    public class GoogleDriveFile : File
    {
        public override string Name
        {
            get
            {
                return file.Title;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public override Directory Parent
        {
            get
            {
                return parent;
            }
        }
        public override Storage Storage
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public override DateTime Date
        {
            get
            {
                return file.ModifiedDate.Value;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public override ulong Size
        {
            get
            {
                return (ulong)(file.FileSize ?? 0);
            }
        }
        public override uint Hash
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private GoogleDriveStorage storage;
        private GoogleDriveDirectory parent;
        internal Google.Apis.Drive.v2.Data.File file;

        public GoogleDriveFile(GoogleDriveStorage storage, GoogleDriveDirectory parent, Google.Apis.Drive.v2.Data.File file)
        {
            this.storage = storage;
            this.parent = parent;
            this.file = file;
        }

        public override System.IO.Stream Open(System.IO.FileAccess access)
        {
            return new System.IO.MemoryStream();
        }
    }
}