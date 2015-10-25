using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Engine
{
    public abstract class Storage
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public abstract Directory Root { get; }
    }
}