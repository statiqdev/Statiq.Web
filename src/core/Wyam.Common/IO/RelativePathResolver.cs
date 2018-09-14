using System;
using System.Collections.Generic;

namespace Wyam.Common.IO
{
    internal static class RelativePathResolver
    {
        public static FilePath Resolve(DirectoryPath source, FilePath target)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            return Resolve(source, target.Directory).GetFilePath(target.FileName);
        }

        public static DirectoryPath Resolve(DirectoryPath source, DirectoryPath target)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            // Make sure they're both either relative or absolute
            if (source.IsAbsolute != target.IsAbsolute)
            {
                throw new ArgumentException("Paths must both be relative or both be absolute");
            }

            // Collapse the paths
            source = source.Collapse();
            target = target.Collapse();

            // Check if they share the same provider
            if (source.FileProvider != target.FileProvider)
            {
                return target;
            }

            // Check if they're the same path
            if (source.FullPath == target.FullPath)
            {
                return new DirectoryPath(".");
            }

            // Special case if source is just root
            if (source.IsAbsolute && source.Segments.Length == 0)
            {
                return new DirectoryPath(string.Join("/", target.Segments));
            }

            // Check if they share the same root
            if (target.Segments.Length == 0 || string.CompareOrdinal(source.Segments[0], target.Segments[0]) != 0)
            {
                return target;
            }

            int minimumSegmentsLength = Math.Min(source.Segments.Length, target.Segments.Length);

            int lastCommonRoot = -1;

            // Find common root
            for (int x = 0; x < minimumSegmentsLength; x++)
            {
                if (string.CompareOrdinal(source.Segments[x], target.Segments[x]) != 0)
                {
                    break;
                }

                lastCommonRoot = x;
            }

            if (lastCommonRoot == -1)
            {
                return target;
            }

            // Add relative folders in from path
            List<string> relativeSegments = new List<string>();
            for (int x = lastCommonRoot + 1; x < source.Segments.Length; x++)
            {
                if (source.Segments[x].Length > 0)
                {
                    relativeSegments.Add("..");
                }
            }

            // Add to folders to path
            for (int x = lastCommonRoot + 1; x < target.Segments.Length; x++)
            {
                relativeSegments.Add(target.Segments[x]);
            }

            // Create relative path
            return new DirectoryPath(string.Join("/", relativeSegments));
        }
    }
}
