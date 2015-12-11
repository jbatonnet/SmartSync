using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartSync.Common;

namespace SmartSync.Perforce
{
    public class PerforceStorage : BasicStorage
    {
        public string Host { get; set; }
        public ushort Port { get; set; }
        public string User { get; set; }
        public string Client { get; set; }

        public bool Submit { get; set; } = false;
        public string Description { get; set; } = "Synchronization by SmartSync";

        public override Directory Root
        {
            get
            {
                // TODO: Check Perforce connection
                return base.Root;
            }
        }
    }
}