using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bedrock.Common;

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
                return storage;
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
                Google.Apis.Drive.v2.Data.File test = new Google.Apis.Drive.v2.Data.File();
                test.ModifiedDate = value;

                FilesResource.UpdateRequest request = storage.Service.Files.Update(test, file.Id);

                request.ModifiedDateBehavior = FilesResource.UpdateRequest.ModifiedDateBehaviorEnum.FromBody;
                request.SetModifiedDate = true;

                request.Execute();
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
            //return new System.IO.MemoryStream();
            return new GoogleDriveStream(storage, file);
        }
    }
}