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
            if (DiffType == DiffType.Dates && CompareFileDates(left, right) != 0)
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
        public static int CompareFileDates(File left, File right)
        {
            // FIXME: What an ugly way to do this
            double diff = (left.Date - right.Date).TotalSeconds;

            if (Math.Abs(diff) < 2)
                return 0;

            return (int)diff;
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
        private FileComparer comparer;

        public FileDiff(Storage leftStorage, File leftFile, Storage rightStorage, File rightFile, FileComparer comparer)
        {
            this.leftStorage = leftStorage;
            this.rightStorage = rightStorage;
            this.leftFile = leftFile;
            this.rightFile = rightFile;
            this.comparer = comparer;
        }

        public Action GetAction(SyncType syncType)
        {
            switch (syncType)
            {
                case SyncType.Backup:
                {
                    if (Right == null)
                        return new CopyFileAction(Left, RightStorage, Left.Parent.Path, Left.Name);
                    else if (Left != null)
                        return new ReplaceFileAction(Left, Right);

                    break;
                }

                case SyncType.Clone:
                {
                    if (Left == null)
                        return new DeleteFileAction(Right);
                    else if (Right == null)
                        return new CopyFileAction(Left, RightStorage, Left.Parent.Path, Left.Name);
                    else
                        return new ReplaceFileAction(Left, Right);
                }

                case SyncType.Sync:
                {
                    if (Left == null)
                        return new CopyFileAction(Right, LeftStorage, Right.Parent.Path, Right.Name);
                    else if (Right == null)
                        return new CopyFileAction(Left, RightStorage, Left.Parent.Path, Left.Name);
                    else
                    {
                        int diff = FileComparer.CompareFileDates(Left, Right);

                        if (diff > 0)
                            return new ReplaceFileAction(Left, Right);
                        else if (diff < 0)
                            return new ReplaceFileAction(Right, Left);
                        else
                            throw new NotImplementedException("Files are different, but they have the same date");
                    }
                }
            }

            return null;
        }

        public override string ToString()
        {
            return (Left?.Path ?? "null") + " - " + (Right?.Path ?? "null");
        }
    }
}