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
            switch (syncType)
            {
                case SyncType.Backup:
                {
                    if (Right == null)
                        return new CreateDirectoryAction(RightStorage, Left.Parent.Path, Left.Name);

                    break;
                }

                case SyncType.Clone:
                {
                    if (Left == null)
                        return new DeleteDirectoryAction(Right);
                    else if (Right == null)
                        return new CreateDirectoryAction(RightStorage, Left.Parent.Path, Left.Name);

                    break;
                }

                case SyncType.Sync:
                {
                    if (Left == null)
                        return new CreateDirectoryAction(LeftStorage, Right.Parent.Path, Right.Name);
                    else if (Right == null)
                        return new CreateDirectoryAction(RightStorage, Left.Parent.Path, Left.Name);

                    break;
                }
            }

            return null;
        }

        public override string ToString()
        {
            return (leftDirectory?.Path ?? "null") + " - " + (rightDirectory?.Path ?? "null");
        }
    }
}