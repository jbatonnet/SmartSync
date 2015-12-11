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
        public PerforceFile(BasicStorage storage, FileInfo fileInfo) : base(storage, fileInfo) { }

        public override Stream Open(FileAccess access)
        {
            // TODO: Return a PerforceFileStream to detect modifications
            return base.Open(access);
        }
    }
}