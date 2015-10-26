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
        public abstract DateTime Date { get; }
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

            if (DiffType == DiffType.Dates && left.Date != right.Date)
                return false;
            if (DiffType == DiffType.Hashes && left.Hash != right.Hash)
                return false;

            return true;
        }

        public int GetHashCode(File file)
        {
            if (DiffType == DiffType.Dates)
                return file.Date.GetHashCode();
            if (DiffType == DiffType.Hashes)
                return unchecked((int)file.Hash);

            return file.Path.GetHashCode();
        }
    }

    public class FileDiff
    {
        public File Left { get; private set; }
        public File Right { get; private set; }

        public FileDiff(File left, File right)
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