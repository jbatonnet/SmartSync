using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartSync.Common;

namespace SmartSync.Perforce
{
    public class PerforceDirectory : BasicDirectory
    {
        public PerforceDirectory(BasicStorage storage, DirectoryInfo directoryInfo) : base(storage, directoryInfo) { }

        public override Common.Directory CreateDirectory(string name)
        {
            // TODO: Automatically mark for add
            return base.CreateDirectory(name);
        }
        public override Common.File CreateFile(string name)
        {
            // TODO: Automatically mark for add
            return base.CreateFile(name);
        }
        public override void DeleteDirectory(Common.Directory directory)
        {
            // TODO: Automatically mark for delete
            base.DeleteDirectory(directory);
        }
        public override void DeleteFile(Common.File file)
        {
            // TODO: Automatically mark for delete
            base.DeleteFile(file);
        }
    }
}