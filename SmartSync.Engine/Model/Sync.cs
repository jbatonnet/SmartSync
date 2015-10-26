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
}