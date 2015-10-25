using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Engine
{
    public class SFTPStorage : Storage
    {
        public string Host { get; set; }
        public ushort Port { get; set; } = 21;

        public string User { get; set; }
        public string Password { get; set; }

        public string Path { get; set; } = "/";

        public override Directory Root
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}