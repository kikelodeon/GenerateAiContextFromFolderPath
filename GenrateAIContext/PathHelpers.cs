using System;
using System.IO;

namespace GenrateAIContext
{
    internal static class PathHelpers
    {
        public static string GetRelativePath(string basePath, string fullPath)
        {
            var bp = basePath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return Uri.UnescapeDataString(
                new Uri(bp).MakeRelativeUri(new Uri(fullPath)).ToString())
                   .Replace('/', Path.DirectorySeparatorChar);
        }
    }
}
