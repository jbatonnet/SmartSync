using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Engine
{
    public class DeleteFileAction : Action
    {
        public File File { get; private set; }

        public DeleteFileAction(File file)
        {
            File = file;
        }

        public override void Run()
        {
            File.Parent.DeleteFile(File);
        }

        public override string ToString()
        {
            return "Delete file " + File;
        }
    }
}