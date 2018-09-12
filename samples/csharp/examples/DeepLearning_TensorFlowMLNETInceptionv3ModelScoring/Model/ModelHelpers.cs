using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TensorFlowMLNETInceptionv3ModelScoring.Model
{
    public static class ModelHelpers
    {
        static FileInfo currentAssemblyLocation = new FileInfo(typeof(Program).Assembly.Location);
        static private readonly string _dataRoot = Path.Combine(currentAssemblyLocation.Directory.FullName, "assets");

        public static string GetAssetsPath(params string[] paths)
        {
            if (paths == null || paths.Length == 0)
                return null;

            return Path.GetFullPath(Path.Combine(paths.Prepend(_dataRoot).ToArray()));
        }

        public static string DeleteAssets(params string[] paths)
        {
            var location = GetAssetsPath(paths);

            if (!string.IsNullOrWhiteSpace(location) && File.Exists(location))
                File.Delete(location);
            return location;
        }

        public static IEnumerable<string> Columns<T>() where T : class
        {
            return typeof(T).GetProperties().Select(p => p.Name);
        }

        public static IEnumerable<string> Columns<T, U>() where T : class
        {
            var typeofU = typeof(U);
            return typeof(T).GetProperties().Where(c => c.PropertyType == typeofU).Select(p => p.Name);
        }

        public static IEnumerable<string> Columns<T, U, V>() where T : class
        {
            var typeofUV = new[] { typeof(U), typeof(V) };
            return typeof(T).GetProperties().Where(c => typeofUV.Contains(c.PropertyType)).Select(p => p.Name);
        }

        public static IEnumerable<string> Columns<T, U, V, W>() where T : class
        {
            var typeofUVW = new[] { typeof(U), typeof(V), typeof(W) };
            return typeof(T).GetProperties().Where(c => typeofUVW.Contains(c.PropertyType)).Select(p => p.Name);
        }

        public static string[] ColumnsNumerical<T>() where T : class
        {
            return Columns<T, float, int>().ToArray();
        }

        public static string[] ColumnsString<T>() where T : class
        {
            return Columns<T, string>().ToArray();
        }

        public static string[] ColumnsDateTime<T>() where T : class
        {
            return Columns<T, DateTime>().ToArray();
        }

        public static string GetLabel(string[] labels, float[] probs)
        {
            var index = probs.AsSpan().IndexOf(probs.Max());
            return labels[index];
        }
    }
}
