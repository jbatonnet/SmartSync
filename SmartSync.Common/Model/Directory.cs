using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Common
{
    public abstract class Directory : Entry
    {
        public abstract IEnumerable<Directory> Directories { get; }
        public abstract IEnumerable<File> Files { get; }
        
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

        public abstract File CreateFile(string name);
        public abstract void DeleteFile(File file);

        public override string ToString()
        {
            return Path;
        }
    }

    public class DirectoryDiff : Diff
    {
        public Storage LeftStorage
        {
            get
            {
                return leftStorage;
            }
        }
        public Directory Left
        {
            get
            {
                return leftDirectory;
            }
        }
        Entry Diff.Left
        {
            get
            {
                return leftDirectory;
            }
        }

        public Storage RightStorage
        {
            get
            {
                return rightStorage;
            }
        }
        public Directory Right
        {
            get
            {
                return rightDirectory;
            }
        }
        Entry Diff.Right
        {
            get
            {
                return rightDirectory;
            }
        }

        private Storage leftStorage, rightStorage;
        private Directory leftDirectory, rightDirectory;

        public DirectoryDiff(Storage leftStorage, Directory leftDirectory, Storage rightStorage, Directory rightDirectory)
        {
            this.leftStorage = leftStorage;
            this.rightStorage = rightStorage;
            this.leftDirectory = leftDirectory;
            this.rightDirectory = rightDirectory;
        }

        public Action GetAction(SyncType syncType)
        {
            if (Left == null)
            {
                if (syncType == SyncType.LeftToRight)
                    return new DeleteDirectoryAction(Right);
                else
                    return new CreateDirectoryAction(leftStorage, rightDirectory.Parent.Path, rightDirectory.Name);
            }
            else if (Right == null)
            {
                if (syncType == SyncType.RightToleft)
                    return new DeleteDirectoryAction(Left);
                else
                    return new CreateDirectoryAction(rightStorage, leftDirectory.Parent.Path, leftDirectory.Name);
            }

            return null;
        }

        public override string ToString()
        {
            return (leftDirectory?.Path ?? "null") + " - " + (rightDirectory?.Path ?? "null");
        }
    }
}