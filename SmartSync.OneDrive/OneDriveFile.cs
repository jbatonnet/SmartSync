using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bedrock.Common;

using Microsoft.OneDrive.Sdk;

namespace SmartSync.OneDrive
{
    using File = Bedrock.Common.File;
    using Directory = Bedrock.Common.Directory;

    public class OneDriveFile : File
    {
        public override string Name
        {
            get
            {
                return file.Name;
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
        public override ulong Size
        {
            get
            {
                return file.Size.HasValue ? (ulong)file.Size : 0;
            }
        }
        public override DateTime Date
        {
            get
            {
                return (file.FileSystemInfo.LastModifiedDateTime?.DateTime ?? DateTime.MinValue).ToLocalTime();
            }
            set
            {
                Item item = new Item();
                item.FileSystemInfo = new Microsoft.OneDrive.Sdk.FileSystemInfo();
                item.FileSystemInfo.LastModifiedDateTime = new DateTimeOffset(value.ToUniversalTime());

                Task<Item> task = storage.Client.Drive.Items[file.Id].Request().UpdateAsync(item);
                task.Wait();

                file = task.Result;
            }
        }
        public override uint Hash
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private OneDriveStorage storage;
        private OneDriveDirectory parent;
        internal Item file;

        public OneDriveFile(OneDriveStorage storage, OneDriveDirectory parent, Item file)
        {
            this.storage = storage;
            this.parent = parent;
            this.file = file;
        }

        public override Stream Open(FileAccess access)
        {
            return new OneDriveStream(storage, file);
        }
    }
}