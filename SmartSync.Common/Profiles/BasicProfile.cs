using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Common
{
    public class BasicProfileContainer : Profile
    {
        public override DiffType DiffType
        {
            get
            {
                return diffType;
            }
        }
        public override SyncType SyncType
        {
            get
            {
                return syncType;
            }
        }

        public override IEnumerable<string> Exclusions
        {
            get
            {
                return exclusions;
            }
        }

        public override Storage Left
        {
            get
            {
                return left;
            }
        }
        public override Storage Right
        {
            get
            {
                return right;
            }
        }

        protected DiffType diffType;
        protected SyncType syncType;
        protected List<string> exclusions = new List<string>();
        protected Storage left, right;
    }

    public class BasicProfile : BasicProfileContainer
    {
        public new DiffType DiffType
        {
            get
            {
                return diffType;
            }
            set
            {
                diffType = value;
            }
        }
        public new SyncType SyncType
        {
            get
            {
                return syncType;
            }
            set
            {
                syncType = value;
            }
        }

        public new List<string> Exclusions
        {
            get
            {
                return exclusions;
            }
        }

        public new Storage Left
        {
            get
            {
                return left;
            }
            set
            {
                left = value;
            }
        }
        public new Storage Right
        {
            get
            {
                return right;
            }
            set
            {
                right = value;
            }
        }
    }
}