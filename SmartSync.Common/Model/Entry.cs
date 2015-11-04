using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Common
{
    public abstract class Entry : IEquatable<Entry>
    {
        public abstract string Name { get; set; }
        public abstract Directory Parent { get; }
        public abstract Storage Storage { get; }

        public virtual string Path
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

        public bool Equals(Entry other)
        {
            if (Storage != other.Storage)
                return false;

            if (Name != other.Name)
                return false;

            return Path == other.Path;
        }
    }
}