using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpCompress.Archive.Tar;

namespace SmartSync.Common
{
    public class TarFile : File
    {
        public override string Name
        {
            get
            {
                string name = "/" + file.Key;
                return name.Substring(name.LastIndexOf('/') + 1);
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
                return file.LastModifiedTime.GetValueOrDefault();
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
                return (ulong)file.Size;
            }
        }
        public override uint Hash
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        internal TarStorage storage;
        internal Directory parent;
        internal TarArchiveEntry file;

        public TarFile(TarStorage storage, Directory parent, TarArchiveEntry file)
        {
            this.storage = storage;
            this.parent = parent;
            this.file = file;
        }

        public override Stream Open(FileAccess access)
        {
            throw new NotImplementedException();
        }
    }
}