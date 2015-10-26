using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ionic.Zip;

namespace SmartSync.Engine
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
        public override DateTime Date
        {
            get
            {
                return file.LastModified;
            }
            set
            {
                file.LastModified = value;
            }
        }
        public override uint Hash
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        internal Ionic.Zip.ZipFile zip;
        internal Directory parent;
        internal ZipEntry file;

        public ZipFile(Ionic.Zip.ZipFile zip, Directory parent, ZipEntry file)
        {
            this.zip = zip;
            this.parent = parent;
            this.file = file;
        }

        public override Stream Open(FileAccess access)
        {
            //return new MemoryStream();
            return new ZipStream(zip, file, access);
        }
    }
}