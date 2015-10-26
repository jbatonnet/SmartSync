using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Engine
{
    public class BasicDirectory : Directory
    {
        public override string Name
        {
            get
            {
                return directoryInfo.Name;
            }
        }
        public override Directory Parent
        {
            get
            {
                return parent;
            }
        }
        public override IEnumerable<Directory> Directories
        {
            get
            {
                foreach (DirectoryInfo directoryInfo in directoryInfo.EnumerateDirectories())
                    yield return new BasicDirectory(directoryInfo, this);
            }
        }
        public override IEnumerable<File> Files
        {
            get
            {
                foreach (FileInfo fileInfo in directoryInfo.EnumerateFiles())
                    yield return new BasicFile(fileInfo, this);
            }
        }

        private DirectoryInfo directoryInfo;
        private Directory parent;

        public BasicDirectory(DirectoryInfo directoryInfo, Directory parent)
        {
            this.directoryInfo = directoryInfo;
            this.parent = parent;
        }

        public override Directory CreateDirectory(string name)
        {
            return new BasicDirectory(directoryInfo.CreateSubdirectory(name), this);
        }
        public override void DeleteDirectory(Directory directory)
        {
            throw new NotImplementedException();
        }

        public override File CreateFile(string name)
        {
            FileInfo fileInfo = new FileInfo(System.IO.Path.Combine(directoryInfo.FullName, name));
            fileInfo.Create().Close();
            return new BasicFile(fileInfo, this);
        }
        public override void DeleteFile(File file)
        {
            throw new NotImplementedException();
        }
    }
}