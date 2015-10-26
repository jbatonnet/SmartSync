using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Engine
{
    public class CreateDirectoryAction : Action
    {
        public Storage Storage { get; private set; }
        public string Path { get; private set; }
        public string Name { get; private set; }

        public CreateDirectoryAction(Storage storage, string path, string name)
        {
            Storage = storage;
            Path = path;
            Name = name;
        }

        public override void Process()
        {
            Directory parent = Storage.GetDirectory(Path);
            parent.CreateDirectory(Name);
        }

        public override string ToString()
        {
            return "Create directory " + Path + "/" + Name;
        }
    }
}