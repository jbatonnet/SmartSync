using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Common
{
    public class DeleteFileAction : Action
    {
        public File File { get; private set; }

        public DeleteFileAction(File file)
        {
            File = file;
        }

        public override void Process()
        {
            File.Parent.DeleteFile(File);
        }

        public override string ToString()
        {
            return "Delete file " + File;
        }
    }
}