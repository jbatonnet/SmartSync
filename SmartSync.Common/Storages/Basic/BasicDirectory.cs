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
                if (storage.Path.FullName == directoryInfo.FullName)
                    return null;

                return new BasicDirectory(storage, directoryInfo.Parent);
            }
        }
        public override Storage Storage
        {
            get
            {
                return storage;
            }
        }
        public override string Path
        {
            get
            {
                if (storage.Path.FullName == directoryInfo.FullName)
                    return "/";

                return directoryInfo.FullName.Substring(storage.Path.FullName.Length).Replace('\\', '/');
            }
        }

        public override IEnumerable<Directory> Directories
        {
            get
            {
                foreach (DirectoryInfo directoryInfo in directoryInfo.EnumerateDirectories())
                    yield return new BasicDirectory(storage, directoryInfo);
            }
        }
        public override IEnumerable<File> Files
        {
            get
            {
                foreach (FileInfo fileInfo in directoryInfo.EnumerateFiles())
                    yield return new BasicFile(storage, fileInfo);
            }
        }

        private BasicStorage storage;
        protected internal DirectoryInfo directoryInfo;

        public BasicDirectory(BasicStorage storage, DirectoryInfo directoryInfo)
        {
            this.storage = storage;
            this.directoryInfo = directoryInfo;
        }

        public override Directory CreateDirectory(string name)
        {
            if (!Storage.IsNameValid(name))
                throw new ArgumentException("The specified name contains invalid characters");

            return new BasicDirectory(storage, directoryInfo.CreateSubdirectory(name));
        }
        public override void DeleteDirectory(Directory directory)
        {
            if (!directory.Parent.Equals(this))
                throw new ArgumentException("The specified directory could not be found");

            BasicDirectory basicDirectory = directory as BasicDirectory;
            basicDirectory.directoryInfo.Delete(true);
        }

        public override File CreateFile(string name)
        {
            if (!Storage.IsNameValid(name))
                throw new ArgumentException("The specified name contains invalid characters");

            FileInfo fileInfo = new FileInfo(System.IO.Path.Combine(directoryInfo.FullName, name));
            fileInfo.Create().Close();
            return new BasicFile(storage, fileInfo);
        }
        public override void DeleteFile(File file)
        {
            if (!file.Parent.Equals(this))
                throw new ArgumentException("The specified file could not be found");

            BasicFile basicFile = file as BasicFile;
            basicFile.fileInfo.Delete();
        }
    }
}