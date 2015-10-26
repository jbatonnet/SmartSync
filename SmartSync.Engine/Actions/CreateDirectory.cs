using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Engine
{
    public class CreateDirectoryAction : Action
    {
        public Directory Parent { get; private set; }
        public string Name { get; private set; }

        public CreateDirectoryAction(Directory parent, string name)
        {
            Parent = parent;
            Name = name;
        }

        public override void Run()
        {
            Parent.CreateDirectory(Name);
        }

        public override string ToString()
        {
            return "Create directory " + Parent + "/" + Name;
        }
    }
}