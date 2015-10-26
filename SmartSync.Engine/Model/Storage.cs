using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Engine
{
    public abstract class Storage
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public abstract Directory Root { get; }

        public virtual Directory GetDirectory(string path)
        {
            if (path[0] != '/')
                return null;

            return Root.GetDirectory(path.Substring(1));
        }
        public virtual File GetFile(string path)
        {
            if (path[0] != '/')
                return null;

            return Root.GetFile(path.Substring(1));
        }
    }
}