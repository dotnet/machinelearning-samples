using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.ML.Data;
using Microsoft.ML;
using static Microsoft.ML.TrainCatalogBase;
using Microsoft.ML.Auto;
using System.Text;

namespace Common
{
    public static class ConsoleHelper
    {
        public static void PrintRegressionMetrics(string name, RegressionMetrics metrics)
        {
            Console.WriteLine($"*************************************************");
            Console.WriteLine($"*       Metrics for {name} regression model      ");
            Console.WriteLine($"*------------------------------------------------");
            Console.WriteLine($"*       LossFn:        {metrics.LossFunction:0.##}");
            Console.WriteLine($"*       R2 Score:      {metrics.RSquared:0.##}");
            Console.WriteLine($"*       Absolute loss: {metrics.MeanAbsoluteError:#.##}");
            Console.WriteLine($"*       Squared loss:  {metrics.MeanSquaredError:#.##}");
            Console.WriteLine($"*       RMS loss:      {metrics.RootMeanSquaredError:#.##}");
            Console.WriteLine($"*************************************************");
        }

        public static void PrintBinaryClassificationMetrics(string name, BinaryClassificationMetrics metrics)
        {
            Console.WriteLine($"************************************************************");
            Console.WriteLine($"*       Metrics for {name} binary classification model      ");
            Console.WriteLine($"*-----------------------------------------------------------");
            Console.WriteLine($"*       Accuracy: {metrics.Accuracy:P2}");
            Console.WriteLine($"*       Area Under Curve:      {metrics.AreaUnderRocCurve:P2}");
            Console.WriteLine($"*       Area under Precision recall Curve:  {metrics.AreaUnderPrecisionRecallCurve:P2}");
            Console.WriteLine($"*       F1Score:  {metrics.F1Score:P2}");
            Console.WriteLine($"*       PositivePrecision:  {metrics.PositivePrecision:#.##}");
            Console.WriteLine($"*       PositiveRecall:  {metrics.PositiveRecall:#.##}");
            Console.WriteLine($"*       NegativePrecision:  {metrics.NegativePrecision:#.##}");
            Console.WriteLine($"*       NegativeRecall:  {metrics.NegativeRecall:P2}");
            Console.WriteLine($"************************************************************");
        }

        public static void PrintMultiClassClassificationMetrics(string name, MulticlassClassificationMetrics metrics)
        {
            Console.WriteLine($"************************************************************");
            Console.WriteLine($"*    Metrics for {name} multi-class classification model   ");
            Console.WriteLine($"*-----------------------------------------------------------");
            Console.WriteLine($"    AccuracyMacro = {metrics.MacroAccuracy:0.####}, a value between 0 and 1, the closer to 1, the better");
            Console.WriteLine($"    AccuracyMicro = {metrics.MicroAccuracy:0.####}, a value between 0 and 1, the closer to 1, the better");
            Console.WriteLine($"    LogLoss = {metrics.LogLoss:0.####}, the closer to 0, the better");
            Console.WriteLine($"    LogLoss for class 1 = {metrics.PerClassLogLoss[0]:0.####}, the closer to 0, the better");
            Console.WriteLine($"    LogLoss for class 2 = {metrics.PerClassLogLoss[1]:0.####}, the closer to 0, the better");
            Console.WriteLine($"    LogLoss for class 3 = {metrics.PerClassLogLoss[2]:0.####}, the closer to 0, the better");
            Console.WriteLine($"************************************************************");
        }

        public static void PrintRegressionIterationMetrics(int iteration, string trainerName, RegressionMetrics metrics)
        {
            Console.WriteLine($"{iteration,-3}{trainerName,-35}{metrics.RSquared,-10:0.###}{metrics.LossFunction,-8:0.##}{metrics.MeanAbsoluteError,-15:#.##}{metrics.MeanSquaredError,-15:#.##}{metrics.RootMeanSquaredError,-10:#.##}");
        }

        public static void PrintObserveProgressRegressionHeader()
        {
            Console.WriteLine($"*************************************************");
            Console.WriteLine($"*       Metrics for regression models     ");
            Console.WriteLine($"*------------------------------------------------");
            Console.WriteLine($"{" ",-3}{"Trainer",-35}{"R2-Score",-10}{"LossFn",-8}{"Absolute-loss",-15}{"Squared-loss",-15}{"RMS-loss",-10}");
            Console.WriteLine();
        }

        public static void ShowDataViewInConsole(MLContext mlContext, IDataView dataView, int numberOfRows = 4)
        {
            string msg = string.Format("Show data in DataView: Showing {0} rows with the columns", numberOfRows.ToString());
            ConsoleWriteHeader(msg);

            var preViewTransformedData = dataView.Preview(maxRows: numberOfRows);

            foreach (var row in preViewTransformedData.RowView)
            {
                var ColumnCollection = row.Values;
                string lineToPrint = "Row--> ";
                foreach (KeyValuePair<string, object> column in ColumnCollection)
                {
                    lineToPrint += $"| {column.Key}:{column.Value}";
                }
                Console.WriteLine(lineToPrint + "\n");
            }
        }

        public static void ConsoleWriteHeader(params string[] lines)
        {
            var defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(" ");
            foreach (var line in lines)
            {
                Console.WriteLine(line);
            }
            var maxLength = lines.Select(x => x.Length).Max();
            Console.WriteLine(new string('#', maxLength));
            Console.ForegroundColor = defaultColor;
        }

        public static void Print(ColumnInferenceResults results)
        {
            Console.WriteLine("Inferred dataset columns --");
            new ColumnInferencePrinter(results).Print();
            Console.WriteLine();
        }

        public static string BuildStringTable(IList<string[]> arrValues)
        {
            int[] maxColumnsWidth = GetMaxColumnsWidth(arrValues);
            var headerSpliter = new string('-', maxColumnsWidth.Sum(i => i + 3) - 1);

            var sb = new StringBuilder();
            for (int rowIndex = 0; rowIndex < arrValues.Count; rowIndex++)
            {
                if (rowIndex == 0)
                {
                    sb.AppendFormat("  {0} ", headerSpliter);
                    sb.AppendLine();
                }

                for (int colIndex = 0; colIndex < arrValues[0].Length; colIndex++)
                {
                    // Print cell
                    string cell = arrValues[rowIndex][colIndex];
                    cell = cell.PadRight(maxColumnsWidth[colIndex]);
                    sb.Append(" | ");
                    sb.Append(cell);
                }

                // Print end of line
                sb.Append(" | ");
                sb.AppendLine();

                // Print splitter
                if (rowIndex == 0)
                {
                    sb.AppendFormat(" |{0}| ", headerSpliter);
                    sb.AppendLine();
                }

                if (rowIndex == arrValues.Count - 1)
                {
                    sb.AppendFormat("  {0} ", headerSpliter);
                }
            }

            return sb.ToString();
        }

        private static int[] GetMaxColumnsWidth(IList<string[]> arrValues)
        {
            var maxColumnsWidth = new int[arrValues[0].Length];
            for (int colIndex = 0; colIndex < arrValues[0].Length; colIndex++)
            {
                for (int rowIndex = 0; rowIndex < arrValues.Count; rowIndex++)
                {
                    int newLength = arrValues[rowIndex][colIndex].Length;
                    int oldLength = maxColumnsWidth[colIndex];

                    if (newLength > oldLength)
                    {
                        maxColumnsWidth[colIndex] = newLength;
                    }
                }
            }

            return maxColumnsWidth;
        }

        class ColumnInferencePrinter
        {
            private static readonly string[] TableHeaders = new[] { "Name", "Data Type", "Purpose" };

            private readonly ColumnInferenceResults _results;

            public ColumnInferencePrinter(ColumnInferenceResults results)
            {
                _results = results;
            }

            public void Print()
            {
                var tableRows = new List<string[]>();

                // Add headers
                tableRows.Add(TableHeaders);

                // Add column data
                var info = _results.ColumnInformation;
                AppendTableRow(tableRows, info.LabelColumnName, "Label");
                AppendTableRow(tableRows, info.ExampleWeightColumnName, "Weight");
                AppendTableRow(tableRows, info.SamplingKeyColumnName, "Sampling Key");
                AppendTableRows(tableRows, info.CategoricalColumnNames, "Categorical");
                AppendTableRows(tableRows, info.NumericColumnNames, "Numeric");
                AppendTableRows(tableRows, info.TextColumnNames, "Text");
                AppendTableRows(tableRows, info.IgnoredColumnNames, "Ignored");

                Console.WriteLine(ConsoleHelper.BuildStringTable(tableRows));
            }

            private void AppendTableRow(ICollection<string[]> tableRows,
                string columnName, string columnPurpose)
            {
                if (columnName == null)
                {
                    return;
                }

                tableRows.Add(new[]
                {
                columnName,
                GetColumnDataType(columnName),
                columnPurpose
            });
            }

            private void AppendTableRows(ICollection<string[]> tableRows,
                IEnumerable<string> columnNames, string columnPurpose)
            {
                foreach (var columnName in columnNames)
                {
                    AppendTableRow(tableRows, columnName, columnPurpose);
                }
            }

            private string GetColumnDataType(string columnName)
            {
                return _results.TextLoaderOptions.Columns.First(c => c.Name == columnName).DataKind.ToString();
            }
        }
    }
}
