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
        private PerforceStorage storage;

        public PerforceDirectory(PerforceStorage storage, DirectoryInfo directoryInfo) : base(storage, directoryInfo)
        {
            this.storage = storage;
        }

        public override Common.File CreateFile(string name)
        {
            Common.File file = base.CreateFile(name);
            if (file == null)
                return null;

            string path = System.IO.Path.Combine(directoryInfo.FullName, name);
            storage.Connection.Client.AddFiles(null, new FileSpec(null, null, new LocalPath(path), VersionSpec.None));

            return file;
        }
        public override void DeleteFile(Common.File file)
        {
            base.DeleteFile(file);

            // TODO: Mark for delete
        }
    }
}