using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Engine
{
    public class ZipStorage : Storage
    {
        public Storage Storage { get; set; }
        public string Path { get; set; }

        public override Directory Root
        {
            get
            {
                throw new NotImplementedException();
            }
        }

    }
}