using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Engine
{
    public enum DiffType
    {
        Dates,
        Hashes
    }

    public enum SyncType
    {
        LeftToRight,
        RightToleft,
        Sync
    }

    public class Profile
    {
        public DiffType DiffType { get; protected set; } = DiffType.Dates;
        public SyncType SyncType { get; protected set; } = SyncType.Sync;

        public Storage Left { get; protected set; }
        public Storage Right { get; protected set; }
    }
}