using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmartSync.Common
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

    public abstract class Profile : IDisposable
    {
        static Profile()
        {
            Bootstrap.Initialize();
        }

        public virtual DiffType DiffType { get; } = DiffType.Dates;
        public virtual SyncType SyncType { get; } = SyncType.Sync;

        public abstract IEnumerable<string> Exclusions { get; }

        public abstract Storage Left { get; }
        public abstract Storage Right { get; }

        // TODO: Provide helpers to get all directories/files

        public virtual IEnumerable<Diff> GetDifferences()
        {
            string[] exclusions = Exclusions.ToArray();

            // Left directories
            Directory leftRoot = Left.Root;
            List<Directory> leftDirectories = new List<Directory>() { leftRoot };
            leftDirectories.AddRange(GetSubDirectories(leftRoot, exclusions));
            
            // Right directories
            Directory rightRoot = Right.Root;
            List<Directory> rightDirectories = new List<Directory>() { rightRoot };
            rightDirectories.AddRange(GetSubDirectories(rightRoot, exclusions));

            // Compute directory differences
            IEnumerable<DirectoryDiff> directoryDiffs = FullOuterJoin(leftDirectories, rightDirectories, l => l.Path, r => r.Path, (l, r, p) => new DirectoryDiff(Left, l, Right, r))
                                                                  .Where(d => d.Left == null || d.Right == null);
            foreach (DirectoryDiff directoryDiff in directoryDiffs)
                yield return directoryDiff;

            // Left files
            File[] leftFiles = leftDirectories.SelectMany(d => d.Files)
                                              .Where(f => !exclusions.Any(e => MatchPattern(f.Path, e)))
                                              .ToArray();
            // Right files
            File[] rightFiles = rightDirectories.SelectMany(d => d.Files)
                                                .Where(f => !exclusions.Any(e => MatchPattern(f.Path, e)))
                                                .ToArray();

            // Compute file differences
            IEnumerable<FileDiff> fileDiffs = FullOuterJoin(leftFiles, rightFiles, l => l, r => r, (l, r, p) => new FileDiff(Left, l, Right, r), keyComparer: new FileComparer(DiffType))
                                                  .Where(d => d.Left == null || d.Right == null);
            foreach (FileDiff fileDiff in fileDiffs)
                yield return fileDiff;
        }

        public virtual void Dispose()
        {
            Left?.Dispose();
            Right?.Dispose();
        }

        protected static IEnumerable<Directory> GetSubDirectories(Directory directory, string[] exclusions = null)
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
        protected static bool MatchPattern(string path, string pattern)
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
        protected static object GetFileKey(File file, DiffType type)
        {
            switch (type)
            {
                case DiffType.Dates: return file.Date;
                case DiffType.Hashes: return file.Hash;
            }

            return null;
        }
        protected static IEnumerable<TResult> FullOuterJoin<TLeft, TRight, TKey, TResult>(IEnumerable<TLeft> left, IEnumerable<TRight> right, Func<TLeft, TKey> leftKeySelector, Func<TRight, TKey> rightKeySelector, Func<TLeft, TRight, TKey, TResult> projection, TLeft leftDefault = default(TLeft), TRight rightDefault = default(TRight), IEqualityComparer<TKey> keyComparer = null)
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