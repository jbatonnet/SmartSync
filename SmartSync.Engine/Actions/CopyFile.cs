using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Engine
{
    public class CopyFileAction : Action
    {
        public File Source { get; private set; }
        public Directory DestinationParent { get; private set; }
        public string DestinationName { get; private set; }

        public CopyFileAction(File source, Directory destinationParent, string destinationName)
        {
            Source = source;
            DestinationParent = destinationParent;
            DestinationName = destinationName;
        }

        public override void Run()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "Copy " + Source + " to " + DestinationParent + "/" + DestinationName;
        }
    }
}