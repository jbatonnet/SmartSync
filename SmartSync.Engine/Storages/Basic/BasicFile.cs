using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Engine
{
    public class BasicFile : File
    {
        public override string Name
        {
            get
            {
                return fileInfo.Name;
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
                return fileInfo.LastWriteTime;
            }
        }
        public override uint Hash
        {
            get
            {
                return unchecked((uint)Path.GetHashCode());
            }
        }

        private FileInfo fileInfo;
        private Directory parent;

        public BasicFile(FileInfo fileInfo, Directory parent)
        {
            this.fileInfo = fileInfo;
            this.parent = parent;
        }

        public override string ToString()
        {
            return Path;
        }
    }
}