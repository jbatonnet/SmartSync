using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Common
{
    public abstract class File : Entry
    {
        public abstract DateTime Date { get; set; }
        public abstract ulong Size { get; }
        public abstract uint Hash { get; }

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

        public abstract Stream Open(FileAccess access);

        public override string ToString()
        {
            return Path;
        }
    }

    public class FileComparer : IEqualityComparer<File>
    {
        public DiffType DiffType { get; private set; }

        public FileComparer(DiffType diffType)
        {
            DiffType = diffType;
        }

        public bool Equals(File left, File right)
        {
            if (left.Path != right.Path)
                return false;

            if (DiffType == DiffType.Paths)
                return true;

            if (DiffType == DiffType.Sizes && left.Size != right.Size)
                return false;
            if (DiffType == DiffType.Dates && left.Date != right.Date)
                return false;
            if (DiffType == DiffType.Hashes && left.Hash != right.Hash)
                return false;

            return true;
        }

        public int GetHashCode(File file)
        {
            return GetFileKey(file, DiffType).GetHashCode();
        }

        public static object GetFileKey(File file, DiffType diffType)
        {
            if (diffType == DiffType.Paths)
                return file.Path;
            if (diffType == DiffType.Sizes)
                return file.Size;
            if (diffType == DiffType.Dates)
                return file.Date;
            if (diffType == DiffType.Hashes)
                return file.Hash;

            return file.Path;
        }
    }

    public class FileDiff : Diff
    {
        public Storage LeftStorage
        {
            get
            {
                return leftStorage;
            }
        }
        public File Left
        {
            get
            {
                return leftFile;
            }
        }
        Entry Diff.Left
        {
            get
            {
                return leftFile;
            }
        }

        public Storage RightStorage
        {
            get
            {
                return rightStorage;
            }
        }
        public File Right
        {
            get
            {
                return rightFile;
            }
        }
        Entry Diff.Right
        {
            get
            {
                return rightFile;
            }
        }

        private Storage leftStorage, rightStorage;
        private File leftFile, rightFile;

        public FileDiff(Storage leftStorage, File leftFile, Storage rightStorage, File rightFile)
        {
            this.leftStorage = leftStorage;
            this.rightStorage = rightStorage;
            this.leftFile = leftFile;
            this.rightFile = rightFile;
        }

        public override string ToString()
        {
            return (Left?.Path ?? "null") + " - " + (Right?.Path ?? "null");
        }

        public Action GetAction(SyncType syncType)
        {
            if (Left == null)
            {
                switch (syncType)
                {
                    case SyncType.LeftToRight:
                        return new DeleteFileAction(rightFile);

                    case SyncType.RightToleft:
                    case SyncType.Sync:
                        return new CopyFileAction(rightFile, leftStorage, rightFile.Parent.Path, rightFile.Name);
                }
            }
            else if (Right == null)
            {
                switch (syncType)
                {
                    case SyncType.RightToleft:
                        return new DeleteFileAction(leftFile);

                    case SyncType.LeftToRight:
                    case SyncType.Sync:
                        return new CopyFileAction(leftFile, rightStorage, leftFile.Parent.Path, leftFile.Name);
                }
            }
            else
            {
                switch (syncType)
                {
                    case SyncType.LeftToRight:
                        return new ReplaceFileAction(leftFile, rightFile);

                    case SyncType.RightToleft:
                        return new ReplaceFileAction(rightFile, leftFile);

                    case SyncType.Sync:
                        throw new NotImplementedException();
                }
            }

            return null;
        }
    }
}