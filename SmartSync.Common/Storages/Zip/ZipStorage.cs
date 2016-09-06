using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Common
{
    public class ZipStorage : Storage
    {
        public Storage Storage { get; set; }
        public string Path { get; set; }
        public CompressionLevel Compression { get; set; } = CompressionLevel.Fastest;

        public override Directory Root
        {
            get
            {
                Initialize();
                return root;
            }
        }

        internal ZipArchive Archive { get; private set; }
        internal bool Modified { get; set; } = false;

        private File zipFile;
        private Stream zipStream;
        private ZipRoot root;

        public ZipStorage() { }
        public ZipStorage(Storage storage, string path)
        {
            Storage = storage;
            Path = path;
        }

        public void Initialize()
        {
            if (zipFile != null)
                return;

            zipFile = Storage.GetFile(Path);
            if (zipFile == null)
            {
                int separator = Path.LastIndexOf('/');
                Directory directory = Storage.GetDirectory(Path.Substring(0, separator + 1));
                zipFile = directory.CreateFile(Path.Substring(separator + 1));
            }

            Flush();
        }

        internal void Flush()
        {
            if (Archive != null)
                Archive.Dispose();

            zipStream = zipFile.Open(FileAccess.ReadWrite);
            Archive = new ZipArchive(zipStream, ZipArchiveMode.Update);
            root = new ZipRoot(this);
        }

        public override IEnumerable<Directory> GetAllDirectories(string[] exclusions = null)
        {
            Initialize();

            ZipArchiveEntry[] entries = Archive.Entries.AsParallel()
                                                       .Where(e => e.FullName.EndsWith("/"))
                                                       .OrderBy(e => e.FullName) // TODO: Very costly
                                                       .ToArray();

            // Wrap all folders and rebuild parents
            List<ZipDirectory> directories = new List<ZipDirectory>(entries.Length);
            foreach (ZipArchiveEntry entry in entries)
            {
                if (exclusions != null && exclusions.Any(e => MatchPattern("/" + entry.FullName, e)))
                    continue;

                ZipDirectory parent = directories.Reverse<ZipDirectory>().FirstOrDefault(d => entry.FullName.StartsWith(d.directory.FullName)); // TODO: Optimize StartsWith call with reverse string match test
                directories.Add(new ZipDirectory(this, parent ?? Root, entry));
            }

            // Return each directory
            yield return Root;
            foreach (ZipDirectory directory in directories)
                yield return directory;
        }
        public override IEnumerable<File> GetAllFiles(string[] exclusions = null)
        {
            Initialize();

            Directory[] directories = GetAllDirectories(exclusions).ToArray(); // TODO: Check to replace with Archive.Get(path) foreach item

            ZipArchiveEntry[] entries = Archive.Entries.AsParallel()
                                                       .Where(e => !e.FullName.EndsWith("/"))
                                                       .OrderBy(e => e.FullName) // TODO: Very costly
                                                       .ToArray();

            // Wrap all folders and rebuild parents
            foreach (ZipArchiveEntry entry in entries)
            {
                if (exclusions != null && exclusions.Any(e => MatchPattern("/" + entry.FullName, e)))
                    continue;

                string parentPath = "/" + entry.FullName.Substring(0, Math.Max(0, entry.FullName.LastIndexOf('/')));
                Directory parent = directories.FirstOrDefault(d => d.Path == parentPath); // TODO: Optimize StartsWith call with reverse string match test

                yield return new ZipFile(this, parent ?? Root, entry);
            }
        }

        public override void Dispose()
        {
            if (Archive != null && Modified)
                Archive.Dispose();
        }

        public override string ToString()
        {
            return string.Format("Zip {{ Storage: {0}, Path: {1} }}", Storage, Path);
        }
    }
}