using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SqlBatchInsert
{
    public class SqlBatcher<T> where T : class
    {
        private readonly T[] _data;
        private readonly string _insertLine;
        private readonly PropertyInfo[] _properties;
        private readonly StringBuilder _batchBuilder = new StringBuilder();
        private readonly StringBuilder _rowBuilder = new StringBuilder();

        private int _rowPointer = 0;

        public SqlBatcher(
            T[] data,
            string tableName,
            params string[] columns)
        {
            Type baseType = typeof(T);
            _data = data;

            //string[] columns = header.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray(); ;

            _properties = new PropertyInfo[columns.Length];

            int i = 0;
            foreach (var col in columns)
            {
                _properties[i++] = baseType.GetProperty(col);
            }

            _insertLine = $"insert {tableName} ({string.Join(", ", _properties.Select(pi => pi.Name))}) values";
        }

        public int RowPointer => _rowPointer;

        public string GetInsertCommand(int numRows = 1000)
        {
            if (numRows < 1 || numRows > 1000) throw new ArgumentOutOfRangeException(nameof(numRows), "parameter must be between 1 and 1000");

            if (_rowPointer >= _data.Length) return string.Empty;

            _batchBuilder.Clear();
            _batchBuilder.AppendLine(_insertLine);

            int i = 0;
            while (_rowPointer < _data.Length && i++ < numRows)
            {
                var isLastRow = _rowPointer == _data.Length - 1 || i == numRows;

                _batchBuilder.AppendLine($"({GetRowValues(_rowPointer)}){(isLastRow ? ";" : ",")}");
                _rowPointer++;
            }

            return _batchBuilder.ToString();
        }

        private string GetRowValues(int rowPointer)
        {
            _rowBuilder.Clear();

            T item = _data[rowPointer];

            int i = 0;
            foreach (var pi in _properties)
            {
                _rowBuilder.Append($"{(i++ == 0 ? "" : ", ")}{GetColumnValue(pi, item)}");
            }

            return _rowBuilder.ToString();
        }

        private object GetColumnValue(PropertyInfo pi, T item)
        {
            object value = null;

            value = pi.GetValue(item);

            var type = value == null ? "null" : pi.PropertyType.Name;

            switch (type)
            {
                case "null":
                    return "null";

                case "Decimal":
                    return ((decimal)value).ToString(CultureInfo.InvariantCulture);

                case "DateTime":
                    return $"'{((DateTime)value):s}'";

                case "String":
                    return $"'{value.ToString().Replace("'", "''")}'";

                case "Boolean":
                    return $"{(((bool)value) ? "1" : "0")}";

                default:
                    return value.ToString();
            }
        }
    }
}
