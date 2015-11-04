using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmartSync.Common
{
    public abstract class Storage : IDisposable
    {
        static Storage()
        {
            Bootstrap.Initialize();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public string Name { get; set; }
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public string Description { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract Directory Root { get; }

        public virtual Directory GetDirectory(string path)
        {
            if (path[0] != '/')
                return null;
            if (path == "/")
                return Root;

            return Root.GetDirectory(path.Substring(1));
        }
        public virtual File GetFile(string path)
        {
            if (path[0] != '/')
                return null;

            return Root.GetFile(path.Substring(1));
        }

        public virtual void Dispose() { }

        public static bool IsPathValid(string path)
        {
            if (path.Contains("//"))
                return false;

            return Regex.IsMatch(path, @"[a-zA-Z0-9_\-\.#$~ \/]+");
        }
        public static bool IsNameValid(string name)
        {
            return Regex.IsMatch(name, @"[a-zA-Z0-9_\-\.#$~ ]+");
        }
    }
}