using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Engine
{
    public class SftpDirectory : Directory
    {
        public override string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public override Directory Parent
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override IEnumerable<Directory> Directories
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override IEnumerable<File> Files
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public SftpDirectory()
        {

        }

        public override Directory CreateDirectory(string name)
        {
            throw new NotImplementedException();
        }

        public override void DeleteDirectory(Directory directory)
        {
            throw new NotImplementedException();
        }

        public override void DeleteFile(File file)
        {
            throw new NotImplementedException();
        }
    }
}