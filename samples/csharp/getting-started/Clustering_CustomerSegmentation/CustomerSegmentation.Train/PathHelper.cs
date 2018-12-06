using Microsoft.ML.Runtime.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CustomerSegmentation
{
    public static class PathHelper
    {
        static FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);

        public static string GetAssetsPath(params string[] paths)
        {
            if (paths == null || paths.Length == 0)
                return null;

            return Path.Combine(paths.Prepend(_dataRoot.Directory.FullName).ToArray());
        }

        public static string DeleteAssets(params string[] paths)
        {
            var location = GetAssetsPath(paths);

            if (!string.IsNullOrWhiteSpace(location) && File.Exists(location))
                File.Delete(location);
            return location;
        }

    }

}
