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
        private PerforceStorage storage;

        public PerforceFile(PerforceStorage storage, FileInfo fileInfo) : base(storage, fileInfo)
        {
            this.storage = storage;
        }

        public override Stream Open(FileAccess access)
        {
            return new PerforceStream(storage, fileInfo.Open(FileMode.Open, access));
        }
    }
}