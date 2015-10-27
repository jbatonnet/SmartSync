using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Common
{
    public abstract class Entry
    {
        public abstract string Name { get; set; }
        public abstract Directory Parent { get; }
        public abstract Storage Storage { get; }
    }
}