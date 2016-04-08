using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Common
{
    public abstract class Action
    {
        public abstract void Process();
    }

    public static class ActionExtensions
    {
        public static IEnumerable<Action> Sort(this IEnumerable<Action> me)
        {
            Action[] actions = me.ToArray();

            DeleteFileAction[] deleteFileActions = actions.OfType<DeleteFileAction>().ToArray();
            DeleteDirectoryAction[] deleteDirectoryActions = actions.OfType<DeleteDirectoryAction>().ToArray();
            Action[] otherActions = actions.Except(deleteFileActions).Except(deleteDirectoryActions).ToArray();

            return otherActions.Concat(deleteFileActions).Concat(deleteDirectoryActions.OrderByDescending(a => a.Directory.Path));
        }
    }
}