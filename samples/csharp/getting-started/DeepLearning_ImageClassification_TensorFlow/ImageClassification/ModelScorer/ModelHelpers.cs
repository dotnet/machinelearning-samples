using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ImageClassification.ModelScorer
{
    public static class ModelHelpers
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

        public static (string,float) GetBestLabel(string[] labels, float[] probs)
        {
            var max = probs.Max();
            var index = probs.AsSpan().IndexOf(max);
            return (labels[index],max);
        }

        public static string[] ReadLabels(string labelsLocation)
        {
            return File.ReadAllLines(labelsLocation);
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
    }
}
