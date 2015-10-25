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
                return FileInfo.Name;
            }
        }
        public override Directory Parent
        {
            get
            {
                return parent;
            }
        }
        public FileInfo FileInfo { get; private set; }

        private Directory parent;

        public BasicFile(FileInfo fileInfo, Directory parent)
        {
            FileInfo = fileInfo;
            this.parent = parent;
        }

        public override string ToString()
        {
            return Path;
        }
    }
}