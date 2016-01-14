using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Perforce.P4;

using SmartSync.Common;

namespace SmartSync.Perforce
{
    public class PerforceDirectory : BasicDirectory
    {
        public override IEnumerable<Common.Directory> Directories
        {
            get
            {
                foreach (BasicDirectory directory in base.Directories)
                    yield return new PerforceDirectory(storage, directory.DirectoryInfo);
            }
        }
        public override IEnumerable<Common.File> Files
        {
            get
            {
                foreach (BasicFile file in base.Files)
                    yield return new PerforceFile(storage, file.FileInfo);
            }
        }

        private PerforceStorage storage;

        public PerforceDirectory(PerforceStorage storage, DirectoryInfo directoryInfo) : base(storage, directoryInfo)
        {
            this.storage = storage;
        }

        public override Common.Directory GetDirectory(string path)
        {
            BasicDirectory directory = base.GetDirectory(path) as BasicDirectory;
            if (directory == null)
                return null;

            return new PerforceDirectory(storage, directory.DirectoryInfo);
        }
        public override Common.File GetFile(string path)
        {
            BasicFile file = base.GetFile(path) as BasicFile;
            if (file == null)
                return null;

            return new PerforceFile(storage, file.FileInfo);
        }

        public override Common.Directory CreateDirectory(string name)
        {
            BasicDirectory directory = base.CreateDirectory(name) as BasicDirectory;
            if (directory == null)
                return null;

            return new PerforceDirectory(storage, directory.DirectoryInfo);
        }
        public override void DeleteDirectory(Common.Directory directory)
        {
            base.DeleteDirectory(directory);
        }
        public override Common.File CreateFile(string name)
        {
            BasicFile file = base.CreateFile(name) as BasicFile;
            if (file == null)
                return null;

            string path = System.IO.Path.Combine(DirectoryInfo.FullName, name);
            storage.Connection.Client.AddFiles(null, new FileSpec(null, null, new LocalPath(path), null));

            return new PerforceFile(storage, file.FileInfo);
        }
        public override void DeleteFile(Common.File file)
        {
            base.DeleteFile(file);

            string path = System.IO.Path.Combine(DirectoryInfo.FullName, file.Name);
            FileSpec fileSpec = new FileSpec(null, null, new LocalPath(path), null);

            storage.Connection.Client.RevertFiles(null, fileSpec);
            storage.Connection.Client.DeleteFiles(null, fileSpec);
        }
    }
}