using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Common
{
    public class BasicDirectory : Directory
    {
        public override string Name
        {
            get
            {
                return directoryInfo.Name;
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

        public override IEnumerable<Directory> Directories
        {
            get
            {
                foreach (DirectoryInfo directoryInfo in directoryInfo.EnumerateDirectories())
                    yield return new BasicDirectory(storage, this, directoryInfo);
            }
        }
        public override IEnumerable<File> Files
        {
            get
            {
                foreach (FileInfo fileInfo in directoryInfo.EnumerateFiles())
                    yield return new BasicFile(storage, this, fileInfo);
            }
        }

        private BasicStorage storage;
        private BasicDirectory parent;
        private DirectoryInfo directoryInfo;

        public BasicDirectory(BasicStorage storage, BasicDirectory parent, DirectoryInfo directoryInfo)
        {
            this.storage = storage;
            this.parent = parent;
            this.directoryInfo = directoryInfo;
        }

        public override Directory CreateDirectory(string name)
        {
            return new BasicDirectory(storage, this, directoryInfo.CreateSubdirectory(name));
        }
        public override void DeleteDirectory(Directory directory)
        {
            throw new NotImplementedException();
        }

        public override File CreateFile(string name)
        {
            FileInfo fileInfo = new FileInfo(System.IO.Path.Combine(directoryInfo.FullName, name));
            fileInfo.Create().Close();
            return new BasicFile(storage, this, fileInfo);
        }
        public override void DeleteFile(File file)
        {
            FileInfo fileInfo = new FileInfo(System.IO.Path.Combine(directoryInfo.FullName, file.Name));
            fileInfo.Delete();
        }
    }
}