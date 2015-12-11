using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Common
{
    public class BasicFile : File
    {
        public override string Name
        {
            get
            {
                return fileInfo.Name;
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
                return new BasicDirectory(storage, fileInfo.Directory);
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
                return fileInfo.LastWriteTime;
            }
            set
            {
                fileInfo.LastWriteTime = value;
            }
        }
        public override ulong Size
        {
            get
            {
                return (ulong)fileInfo.Length;
            }
        }
        public override uint Hash
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private BasicStorage storage;
        protected internal FileInfo fileInfo;

        public BasicFile(BasicStorage storage, FileInfo fileInfo)
        {
            this.storage = storage;
            this.fileInfo = fileInfo;
        }

        public override Stream Open(FileAccess access)
        {
            FileShare share = FileShare.None;

            switch (access)
            {
                case FileAccess.Read: share = FileShare.ReadWrite; break;
                case FileAccess.Write: share = FileShare.Read; break;
                case FileAccess.ReadWrite: share = FileShare.Read; break;
            }

            return fileInfo.Open(FileMode.Open, access, share);
        }
    }
}