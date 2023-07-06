
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.ML;

namespace XORApp
{
    public static class Program
    {
        private static string BaseDatasetsRelativePath = "../../../../Data";
        private static string TrainDataRelativePath = 
            $"{BaseDatasetsRelativePath}/xor-repeat10.txt";
        private static string TrainDataRelativePath2 =
            $"{BaseDatasetsRelativePath}/xor2.txt";
        private static string TrainDataRelativePath2R =
            $"{BaseDatasetsRelativePath}/xor2-repeat3.txt";
        private static string TrainDataRelativePath3 =
            $"{BaseDatasetsRelativePath}/xor3.txt";
        private static string TrainDataRelativePathAutoML =
            $"{BaseDatasetsRelativePath}/xorAutoML-repeat10.csv";

        private static string TrainDataPath = GetAbsolutePath(TrainDataRelativePath);
        private static string TrainDataPath2 = GetAbsolutePath(TrainDataRelativePath2);
        private static string TrainDataPath2R = GetAbsolutePath(TrainDataRelativePath2R);
        private static string TrainDataPath3 = GetAbsolutePath(TrainDataRelativePath3);
        private static string TrainDataPathAutoML = GetAbsolutePath(TrainDataRelativePathAutoML);

        private static string BaseModelsRelativePath = "../../../../MLModels";
        private static string ModelRelativePath = $"{BaseModelsRelativePath}/XORModel";
        private static string ModelPath = GetAbsolutePath(ModelRelativePath);

        public static void Main(string[] args)
        {
            var mlContext = new MLContext();

            string ModelPathZip = ModelPath + ".zip";
            string ModelPath1Zip = ModelPath + "1.zip";
            string ModelPath2Zip = ModelPath + "2.zip";
            string ModelPath3Zip = ModelPath + "3.zip";

        Retry:
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("XOR Test, choose an option from the following list:");
            Console.WriteLine("0: Exit");
            Console.WriteLine("1: 1 XOR from RAM");
            Console.WriteLine("2: 1 XOR from file");
            Console.WriteLine("3: 1 XOR from file (AutoML)");
            Console.WriteLine("4: 1 XOR (vector mode) from RAM");
            Console.WriteLine("5: 1 XOR (vector mode) from file");
            Console.WriteLine("6: 2 XOR (vector mode) from RAM");
            Console.WriteLine("7: 2 XOR (vector mode) from full file");
            Console.WriteLine("8: 2 XOR (vector mode) from minimal file");
            Console.WriteLine("9: 3 XOR (vector mode) from minimal file");

            var k = Console.ReadKey();
            switch (k.KeyChar)
            {
                case '0': return;

                case '1':
                    var largeSet = XOR1.LoadData();
                    XOR1.Train(mlContext, ModelPathZip, largeSet);
                    XOR1.TestSomePredictions(mlContext, ModelPathZip);
                    break;

                case '2':
                    XOR1.TrainFromFile(mlContext, ModelPathZip, TrainDataPath);
                    XOR1.TestSomePredictions(mlContext, ModelPathZip);
                    break;
                
                case '3':
                    XOR1AutoML.TrainFromFile(mlContext, ModelPathZip, TrainDataPathAutoML);
                    XOR1AutoML.TestSomePredictions(mlContext, ModelPathZip);
                    break;

                case '4':
                    var largeSet2 = XOR1Vector.LoadData();
                    XOR1Vector.Train(mlContext, ModelPathZip, largeSet2);
                    XOR1Vector.TestSomePredictions(mlContext, ModelPathZip);
                    break;

                case '5':
                    XOR1Vector.TrainFromFile(mlContext, ModelPathZip, TrainDataPath);
                    XOR1Vector.TestSomePredictions(mlContext, ModelPathZip);
                    break;

                case '6':
                    var largeSet3 = XOR2Vector.LoadData();
                    int iRowCount1 = largeSet3.Count;
                    XOR2Vector.Train(mlContext, ModelPath1Zip, ModelPath2Zip, largeSet3);
                    XOR2Vector.TestSomePredictions(mlContext, ModelPath1Zip, ModelPath2Zip, iRowCount1, largeSet3);
                    break;

                case '7':
                    int iRowCount2 = 0;
                    var samples2 = new List<XOR2Vector.XOR2Data>();
                    XOR2Vector.TrainFromFile(mlContext, ModelPath1Zip, ModelPath2Zip, 
                        TrainDataPath2, TrainDataPath2R, out iRowCount2, out samples2, bRepeat:false);
                    XOR2Vector.TestSomePredictions(mlContext, ModelPath1Zip, ModelPath2Zip, iRowCount2, samples2);
                    break;
                
                case '8':
                    int iRowCount3 = 0;
                    var samples3 = new List<XOR2Vector.XOR2Data>();
                    XOR2Vector.TrainFromFile(mlContext, ModelPath1Zip, ModelPath2Zip,
                        TrainDataPath2, TrainDataPath2R, out iRowCount3, out samples3, 
                        bRepeat: true);
                    XOR2Vector.TestSomePredictions(mlContext, ModelPath1Zip, ModelPath2Zip, iRowCount3, samples3);
                    break;
                
                case '9':
                    int iRowCount = 0;
                    var samples = new List<XOR3Vector.XOR3Data>();
                    XOR3Vector.TrainFromFile(mlContext,
                        ModelPath1Zip, ModelPath2Zip, ModelPath3Zip, TrainDataPath3,
                        out iRowCount, out samples);
                    XOR3Vector.TestSomePredictions(mlContext,
                        ModelPath1Zip, ModelPath2Zip, ModelPath3Zip, iRowCount, samples);
                    break;

                case 'a':
                    // Does not work: VectorType for Output
                    // ML.NET does not yet support multi-target regression (MTR)
                    // (only via TensorFlow and Python)
                    // https://github.com/dotnet/machinelearning/issues/2134
                    // System.ArgumentOutOfRangeException HResult=0x80131502
                    // Message=Schema mismatch for label column 'Output': expected Single, got Vector<Single> Arg_ParamName_Name
                    var largeSet4 = XOR2VectorMTR.LoadData();
                    XOR2VectorMTR.Train(mlContext, ModelPath1Zip, ModelPath2Zip, largeSet4);
                    XOR2VectorMTR.TestSomePredictions(mlContext, ModelPath1Zip, ModelPath2Zip);
                    break;

            }
            goto Retry;

            //Console.WriteLine("=============== End of process, hit any key to finish ===============");
            //Console.ReadKey();
        }

        public static string GetAbsolutePath(string relativePath)
        {
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
            fullPath = Path.GetFullPath(fullPath);
            return fullPath;
        }
    }

    public static class DataViewHelper
    {
        public static DataTable ToDataTable(this IDataView dataView)
        {
            DataTable dt = null;
            if (dataView != null)
            {
                dt = new DataTable();
                var preview = dataView.Preview();
                dt.Columns.AddRange(preview.Schema.Select(x => new DataColumn(x.Name)).ToArray());
                foreach (var row in preview.RowView)
                {
                    var r = dt.NewRow();
                    foreach (var col in row.Values)
                    {
                        r[col.Key] = col.Value;
                    }
                    dt.Rows.Add(r);

                }
            }
            return dt;
        }
    }
}
