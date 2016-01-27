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
                return FileInfo.Name;
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
                if (parent == null)
                    parent =  new BasicDirectory(storage, FileInfo.Directory);

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
                return FileInfo.LastWriteTime;
            }
            set
            {
                FileInfo.LastWriteTime = value;
            }
        }
        public override ulong Size
        {
            get
            {
                return (ulong)FileInfo.Length;
            }
        }
        public override uint Hash
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public FileInfo FileInfo { get; private set; }
        private BasicDirectory parent;
        private BasicStorage storage;

        public BasicFile(BasicStorage storage, FileInfo fileInfo)
        {
            this.storage = storage;
            FileInfo = fileInfo;
        }
        public BasicFile(BasicStorage storage, BasicDirectory parent, FileInfo fileInfo)
        {
            this.storage = storage;
            this.parent = parent;
            FileInfo = fileInfo;
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

            return FileInfo.Open(FileMode.Open, access, share);
        }
    }
}