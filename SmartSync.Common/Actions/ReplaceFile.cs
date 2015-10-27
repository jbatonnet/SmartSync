using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Common
{
    public class ReplaceFileAction : Action
    {
        public File Source { get; private set; }
        public File Destination { get; private set; }

        public ReplaceFileAction(File source, File destination)
        {
            Source = source;
            Destination = destination;
        }

        public override void Process()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "Replace file " + Destination + " by " + Source;
        }
    }
}