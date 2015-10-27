using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Common
{
    public class DeleteDirectoryAction : Action
    {
        public Directory Directory { get; private set; }

        public DeleteDirectoryAction(Directory directory)
        {
            Directory = directory;
        }

        public override void Process()
        {
            Directory.Parent.DeleteDirectory(Directory);
        }

        public override string ToString()
        {
            return "Delete directory " + Directory;
        }
    }
}