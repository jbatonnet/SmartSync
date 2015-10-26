using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmartSync.Engine
{
    public static class Program
    {
        static void Main(string[] args)
        {
            DiffType diffType = DiffType.Dates;
            SyncType syncType = SyncType.LeftToRight;

            Storage left = new BasicStorage(@"..\..\..\Test\Left");
            Storage right = new BasicStorage(@"..\..\..\Test\Right");

            string[] exclusions = new string[]
            {
                "/Excel file.xlsx",
                "/Directory/SubDirectory 2"
            };

            // Left directories
            Directory leftRoot = left.Root;
            List<Directory> leftDirectories = new List<Directory>() { leftRoot };
            leftDirectories.AddRange(GetSubDirectories(leftRoot, exclusions));

            // Left files
            File[] leftFiles = leftDirectories.SelectMany(d => d.Files)
                                              .Where(f => !exclusions.Any(e => MatchPattern(f.Path, e)))
                                              .ToArray();

            // Right directories
            Directory rightRoot = right.Root;
            List<Directory> rightDirectories = new List<Directory>() { rightRoot };
            rightDirectories.AddRange(GetSubDirectories(rightRoot, exclusions));

            // Right files
            File[] rightFiles = rightDirectories.SelectMany(d => d.Files)
                                                .Where(f => !exclusions.Any(e => MatchPattern(f.Path, e)))
                                                .ToArray();

            // Compute differences
            DirectoryDiff[] directoryDiffs = FullOuterJoin(leftDirectories, rightDirectories, l => l.Path, r => r.Path, (l, r, p) => new DirectoryDiff(l, r))
                .Where(t => t.Left == null || t.Right == null)
                .ToArray();
            FileDiff[] fileDiffs = FullOuterJoin(leftFiles, rightFiles, l => l, r => r, (l, r, p) => new FileDiff(l, r), keyComparer: new FileComparer(diffType))
                .Where(t => t.Left == null || t.Left == null)
                .ToArray();

            List<Action> actions = new List<Action>();

            // Prepare directory actions
            foreach (DirectoryDiff diff in directoryDiffs)
            {
                if (diff.Left == null)
                {
                    if (syncType == SyncType.LeftToRight)
                        actions.Add(new DeleteDirectoryAction(diff.Right));
                    else
                        actions.Add(new CreateDirectoryAction(left.GetDirectory(diff.Right.Parent.Path), diff.Right.Name));
                }
                else if (diff.Right == null)
                {
                    if (syncType == SyncType.RightToleft)
                        actions.Add(new DeleteDirectoryAction(diff.Left));
                    else
                        actions.Add(new CreateDirectoryAction(right.GetDirectory(diff.Left.Parent.Path), diff.Left.Name));
                }
            }

            // Prepare file actions
            foreach (FileDiff diff in fileDiffs)
            {
                if (diff.Left == null)
                {
                    if (syncType == SyncType.LeftToRight)
                        actions.Add(new DeleteFileAction(diff.Right));
                    else
                        actions.Add(new CopyFileAction(diff.Right, left.GetDirectory(diff.Right.Parent.Path), diff.Right.Name));
                }
                else if (diff.Right == null)
                {
                    if (syncType == SyncType.RightToleft)
                        actions.Add(new DeleteFileAction(diff.Left));
                    else
                        actions.Add(new CopyFileAction(diff.Left, right.GetDirectory(diff.Left.Parent.Path), diff.Left.Name));
                }
                else
                {

                }
            }
        }

        public static IEnumerable<Directory> GetSubDirectories(Directory directory, string[] exclusions = null)
        {
            foreach (Directory subDirectory in directory.Directories)
            {
                if (exclusions != null && exclusions.Any(e => MatchPattern(subDirectory.Path, e)))
                    continue;

                yield return subDirectory;

                foreach (Directory subSubDirectory in GetSubDirectories(subDirectory, exclusions))
                    yield return subSubDirectory;
            }
        }
        public static bool MatchPattern(string path, string pattern)
        {
            if (path == pattern)
                return true;

            // Escape characters
            pattern = pattern.Replace(@"\", @"\\");
            pattern = pattern.Replace(".", @"\.");

            // Replace tokens
            pattern = pattern.Replace("**", ".+");
            pattern = pattern.Replace("*", @"[^\\/]+");

            return Regex.IsMatch(path, pattern);
        }

        public static object GetFileKey(File file, DiffType type)
        {
            switch (type)
            {
                case DiffType.Dates: return file.Date;
                case DiffType.Hashes: return file.Hash;
            }

            return null;
        }


        public static IEnumerable<TResult> FullOuterJoin<TLeft, TRight, TKey, TResult>(this IEnumerable<TLeft> left, IEnumerable<TRight> right,
            Func<TLeft, TKey> leftKeySelector,
            Func<TRight, TKey> rightKeySelector,
            Func<TLeft, TRight, TKey, TResult> projection,
            TLeft leftDefault = default(TLeft),
            TRight rightDefault = default(TRight),
            IEqualityComparer<TKey> keyComparer = null)
        {
            keyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
            var alookup = left.ToLookup(leftKeySelector, keyComparer);
            var blookup = right.ToLookup(rightKeySelector, keyComparer);

            var keys = new HashSet<TKey>(alookup.Select(p => p.Key), keyComparer);
            keys.UnionWith(blookup.Select(p => p.Key));

            var join = from key in keys
                       from xa in alookup[key].DefaultIfEmpty(leftDefault)
                       from xb in blookup[key].DefaultIfEmpty(rightDefault)
                       select projection(xa, xb, key);

            return join;
        }
    }
}