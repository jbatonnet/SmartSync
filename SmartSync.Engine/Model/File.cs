using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Engine
{
    public abstract class File
    {
        public abstract string Name { get; }
        public abstract Directory Parent { get; }
        //public abstract DateTime Date { get; }
        //public abstract uint Hash { get; }

        public string Path
        {
            get
            {
                string path = Parent.Path;

                if (path == "/")
                    return "/" + Name;
                else
                    return path + "/" + Name;
            }
        }
    }
}