using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ionic.Zip;

namespace SmartSync.Common
{
    public class ZipFile : File
    {
        public override string Name
        {
            get
            {
                string name = "/" + file.FileName;
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
                return file.ModifiedTime;
            }
            set
            {
                file.SetEntryTimes(value, value, value);
            }
        }
        public override ulong Size
        {
            get
            {
                return (ulong)file.UncompressedSize;
            }
        }
        public override uint Hash
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        internal ZipStorage storage;
        internal Directory parent;
        internal ZipEntry file;

        public ZipFile(ZipStorage storage, Directory parent, ZipEntry file)
        {
            this.storage = storage;
            this.parent = parent;
            this.file = file;
        }

        public override Stream Open(FileAccess access)
        {
            //return new MemoryStream();
            return new ZipStream(storage.Zip, file, access);
        }
    }
}