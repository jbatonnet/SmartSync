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
        public DirectoryInfo Path { get; set; }
        public override Directory Root
        {
            get
            {
                return new BasicDirectory(this, null, Path);
            }
        }

        public BasicStorage() { }
        public BasicStorage(DirectoryInfo path)
        {
            Path = path;
        }

        public override string ToString()
        {
            return string.Format("BasicStorage {{ Path: {0} }}", Path.FullName);
        }
    }
}