using System.IO;
using System.Linq;

using Microsoft.ML.Data;

namespace CreditCardFraudDetection.Common
{
    public static class LocalConsoleHelper
    {
        public static string GetAssetsPath(params string[] paths)
        {
            var _dataRoot = new FileInfo(typeof(LocalConsoleHelper).Assembly.Location);

            if (paths == null || paths.Length == 0)
            {
                return null;
            }

            return Path.Combine(paths.Prepend(_dataRoot.Directory.FullName).ToArray());
        }

        public static string DeleteAssets(params string[] paths)
        {
            string location = GetAssetsPath(paths);

            if (!string.IsNullOrWhiteSpace(location) && File.Exists(location))
            {
                File.Delete(location);
            }

            return location;
        }
    }
}
