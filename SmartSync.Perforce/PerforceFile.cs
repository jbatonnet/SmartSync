using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartSync.Common;

namespace SmartSync.Perforce
{
    public class PerforceFile : BasicFile
    {
        public override Common.Directory Parent
        {
            get
            {
                BasicDirectory parent = base.Parent as BasicDirectory;
                if (parent == null)
                    return null;

                return new PerforceDirectory(storage, parent.DirectoryInfo);
            }
        }

        private PerforceStorage storage;

        public PerforceFile(PerforceStorage storage, FileInfo fileInfo) : base(storage, fileInfo)
        {
            this.storage = storage;
        }

        public override Stream Open(FileAccess access)
        {
            return new PerforceStream(storage, FileInfo, FileInfo.Open(FileMode.Open, access));
        }
    }
}