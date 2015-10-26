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

        public virtual Directory GetDirectory(string path)
        {
            int separator = path.IndexOf('/');

            if (separator >= 0)
                return Directories.SingleOrDefault(d => d.Name == path.Substring(0, separator))?.GetDirectory(path.Substring(separator + 1));
            else
                return Directories.SingleOrDefault(d => d.Name == path);
        }
        public virtual File GetFile(string path)
        {
            int separator = path.IndexOf('/');

            if (separator >= 0)
                return Directories.SingleOrDefault(d => d.Name == path.Substring(0, separator))?.GetFile(path.Substring(separator + 1));
            else
                return Files.SingleOrDefault(d => d.Name == path);
        }

        public abstract Directory CreateDirectory(string name);
        public abstract void DeleteDirectory(Directory directory);
        public abstract void DeleteFile(File file);
    }

    public class DirectoryDiff
    {
        public Directory Left { get; private set; }
        public Directory Right { get; private set; }

        public DirectoryDiff(Directory left, Directory right)
        {
            Left = left;
            Right = right;
        }

        public override string ToString()
        {
            return (Left?.Path ?? "null") + " - " + (Right?.Path ?? "null");
        }
    }
}