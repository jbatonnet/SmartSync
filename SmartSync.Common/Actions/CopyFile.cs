using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Common
{
    public class CopyFileAction : Action
    {
        public File Source { get; private set; }
        public Storage DestinationStorage { get; private set; }
        public string DestinationPath { get; private set; }
        public string DestinationName { get; private set; }

        public CopyFileAction(File source, Storage destinationStorage, string destinationPath, string destinationName)
        {
            Source = source;
            DestinationStorage = destinationStorage;
            DestinationPath = destinationPath;
            DestinationName = destinationName;
        }

        public override void Process()
        {
            Directory destinationDirectory = DestinationStorage.GetDirectory(DestinationPath);
            File destination = destinationDirectory.CreateFile(DestinationName);

            using (Stream sourceStream = Source.Open(FileAccess.Read))
            using (Stream destinationStream = destination.Open(FileAccess.Write))
                sourceStream.CopyTo(destinationStream);

            destination.Date = Source.Date;
        }

        public override string ToString()
        {
            string destinationPath = DestinationPath != "/" ? DestinationPath : "";
            return "Copy " + Source + " to " + destinationPath + "/" + DestinationName;
        }
    }
}