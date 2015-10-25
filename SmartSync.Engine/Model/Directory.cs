using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Engine
{
    public abstract class Directory
    {
        public abstract string Name { get; }
        public abstract Directory Parent { get; }
        public abstract IEnumerable<Directory> Directories { get; }
        public abstract IEnumerable<File> Files { get; }

        public string Path
        {
            get
            {
                if (Parent == null)
                    return "/";

                string path = Parent.Path;

                if (path == "/")
                    return "/" + Name;
                else
                    return path + "/" + Name;
            }
        }
    }
}