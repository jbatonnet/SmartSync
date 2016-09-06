using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Common
{
    public abstract class Entry : MarshalByRefObject, IEquatable<Entry>
    {
        public abstract string Name { get; set; }
        public abstract Directory Parent { get; }
        public abstract Storage Storage { get; }

        public virtual string Path
        {
            get
            {
                if (path == null)
                {
                    if (Parent == null)
                        return "/";

                    string parentPath = Parent.Path;

                    if (parentPath == "/")
                        path = "/" + Name;
                    else
                        path = parentPath + "/" + Name;
                }

                return path;
            }
        }

        private string path;

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