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
                return DirectoryInfo.Name;
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
                foreach (DirectoryInfo directoryInfo in DirectoryInfo.EnumerateDirectories())
                    yield return new BasicDirectory(directoryInfo, this);
            }
        }
        public override IEnumerable<File> Files
        {
            get
            {
                foreach (FileInfo fileInfo in DirectoryInfo.EnumerateFiles())
                    yield return new BasicFile(fileInfo, this);
            }
        }

        public DirectoryInfo DirectoryInfo { get; private set; }

        private Directory parent;

        public BasicDirectory(DirectoryInfo directoryInfo, Directory parent)
        {
            DirectoryInfo = directoryInfo;
            this.parent = parent;
        }

        public override string ToString()
        {
            return Path;
        }
    }
}