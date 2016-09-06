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
                return DirectoryInfo.Name;
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
                if (storage.Path.FullName == DirectoryInfo.FullName)
                    return null;

                if (parent == null)
                    parent = new BasicDirectory(storage, DirectoryInfo.Parent);

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
        public override string Path
        {
            get
            {
                if (storage.Path.FullName == DirectoryInfo.FullName)
                    return "/";

                return DirectoryInfo.FullName.Substring(storage.Path.FullName.Length).Replace('\\', '/');
            }
        }

        public override IEnumerable<Directory> Directories
        {
            get
            {
                if (storage.UseCache)
                    return DirectoryInfo.GetDirectories().Select(d => new BasicDirectory(storage, this, d)).ToArray();
                else
                    return DirectoryInfo.EnumerateDirectories().Select(d => new BasicDirectory(storage, this, d));
            }
        }
        public override IEnumerable<File> Files
        {
            get
            {
                if (storage.UseCache)
                    return DirectoryInfo.GetFiles().Select(f => new BasicFile(storage, this, f)).ToArray();
                else
                    return DirectoryInfo.EnumerateFiles().Select(f => new BasicFile(storage, this, f));
            }
        }

        public DirectoryInfo DirectoryInfo { get; private set; }
        private BasicDirectory parent;
        private BasicStorage storage;

        public BasicDirectory(BasicStorage storage, DirectoryInfo directoryInfo)
        {
            this.storage = storage;
            DirectoryInfo = directoryInfo;
        }
        public BasicDirectory(BasicStorage storage, BasicDirectory parent, DirectoryInfo directoryInfo)
        {
            this.storage = storage;
            this.parent = parent;
            DirectoryInfo = directoryInfo;
        }

        public override Directory CreateDirectory(string name)
        {
            if (!Storage.IsNameValid(name))
                throw new ArgumentException("The specified name contains invalid characters");

            return new BasicDirectory(storage, DirectoryInfo.CreateSubdirectory(name));
        }
        public override void DeleteDirectory(Directory directory)
        {
            if (!directory.Parent.Equals(this))
                throw new ArgumentException("The specified directory could not be found");

            BasicDirectory basicDirectory = directory as BasicDirectory;
            basicDirectory.DirectoryInfo.Delete(true);
        }

        public override File CreateFile(string name)
        {
            if (!Storage.IsNameValid(name))
                throw new ArgumentException("The specified name contains invalid characters");

            FileInfo fileInfo = new FileInfo(System.IO.Path.Combine(DirectoryInfo.FullName, name));
            fileInfo.Create().Close();
            return new BasicFile(storage, fileInfo);
        }
        public override void DeleteFile(File file)
        {
            if (!file.Parent.Equals(this))
                throw new ArgumentException("The specified file could not be found");

            BasicFile basicFile = file as BasicFile;
            basicFile.FileInfo.Delete();
        }
    }
}