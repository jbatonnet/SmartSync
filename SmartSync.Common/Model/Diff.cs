using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Common
{
    public interface Diff
    {
        Storage LeftStorage { get; }
        Entry Left { get; }

        Storage RightStorage { get; }
        Entry Right { get; }

        Action GetAction(SyncType syncType);
    }
}