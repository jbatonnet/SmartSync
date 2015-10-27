using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Common
{
    public class BasicStorage : Storage
    {
        public string Path { get; set; }
        public override Directory Root
        {
            get
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(Path);
                return new BasicDirectory(this, null, directoryInfo);
            }
        }

        public BasicStorage() { }
        public BasicStorage(string path)
        {
            Path = path;
        }
    }
}