using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Common
{
    [Serializable]
    public class BasicStorage : Storage
    {
        public DirectoryInfo Path { get; set; }
        public override Directory Root
        {
            get
            {
                return new BasicDirectory(this, Path);
            }
        }
        public bool UseCache { get; set; } = false;

        public BasicStorage() { }
        public BasicStorage(DirectoryInfo path)
        {
            Path = path;
        }

        /*public override IEnumerable<Directory> GetAllDirectories(string[] exclusions = null)
        {
            int length = Path.FullName.Length;

            IEnumerable<DirectoryInfo> directories = Path.EnumerateDirectories("*", SearchOption.AllDirectories)
                .AsParallel()
                .Where(d => exclusions != null && exclusions.Any(e => MatchPattern(d.FullName.Substring(length).Replace('\\', '/'), e)));

            yield return Root;

            foreach (DirectoryInfo directory in directories)
                yield return new BasicDirectory(this, directory);
        }*/
        /*public override IEnumerable<File> GetAllFiles(string[] exclusions = null)
        {
            int length = Path.FullName.Length;

            return Path.EnumerateFiles("*", SearchOption.AllDirectories)
                .AsParallel()
                .Where(f => exclusions != null && exclusions.Any(e => MatchPattern(f.FullName.Substring(length).Replace('\\', '/'), e)))
                .Select(f => new BasicFile(this, f));
        }*/

        public override string ToString()
        {
            return string.Format("Basic {{ Path: {0} }}", Path.FullName);
        }
    }
}