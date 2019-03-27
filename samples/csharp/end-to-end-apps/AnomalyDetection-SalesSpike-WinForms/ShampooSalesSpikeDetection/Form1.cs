using Microsoft.Data.DataView;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.TimeSeries;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ShampooSalesSpikeDetection
{
    public partial class Form1 : Form
    {
        private DataTable dataTable = null;
        private string filePath = "";
        Tuple<string, string> tup = null;
        Dictionary<int, Tuple<string, string>> dict = new Dictionary<int, Tuple<string, string>>();

        public Form1()
        {
            InitializeComponent();
        }

        // Find file button
        private void button1_Click(object sender, EventArgs e)
        {
            // Open File Explorer
            DialogResult result = openFileExplorer.ShowDialog();

            // Set text in file path textbox to file path from file explorer
            if (result == DialogResult.OK)
            {
                filePathTextbox.Text = openFileExplorer.FileName;
            }
        }

        // Go button
        private void button2_Click(object sender, EventArgs e)
        {
            // Set filepath from text from filepath textbox
            filePath = filePathTextbox.Text;
            
            // Checkk if file exists
            if (File.Exists(filePath))
            {
                dict = new Dictionary<int, Tuple<string, string>>();

                if (filePath != "")
                {
                    // Reset text in anomaly textbox
                    anomalyText.Text = "";

                    // Display preview of dataset and graph
                    displayDataTableAndGraph();

                    // Load a trained model to detect anomalies and then mark them on the graph
                    detectAnomalies();

                }
                // If file path textbox is empty, prompt user to input file path
                else
                {
                    MessageBox.Show("Please input file path.");
                }
            }
            else
            {
                MessageBox.Show("File does not exist. Try finding the file again.");
            }
        }
    


        private void displayDataTableAndGraph()
        {
            dataTable = new DataTable();
            string[] dataCol = null;
            int a = 0;
            string xAxis = "";
            string yAxis = "";

            string[] dataset = File.ReadAllLines(filePath);
            dataCol = commaSeparatedRadio.Checked ? dataset[0].Split(',') : dataset[0].Split('\t');

            dataTable.Columns.Add(dataCol[0]);
            dataTable.Columns.Add(dataCol[1]);
            xAxis = dataCol[0];
            yAxis = dataCol[1];

            foreach (string line in dataset.Skip(1))
            {
                // Add next row of data
                dataCol = commaSeparatedRadio.Checked ? line.Split(',') : line.Split('\t');
                dataTable.Rows.Add(dataCol);

                tup = new Tuple<string, string>(dataCol[0], dataCol[1]);
                dict.Add(a, tup);

                a++;
            }

            // Set data view preview source
            dataGridView1.DataSource = dataTable;

            // Update y axis min and max values
            double yMax = Convert.ToDouble(dataTable.Compute("max([" + yAxis + "])", string.Empty));
            double yMin = Convert.ToDouble(dataTable.Compute("min([" + yAxis + "])", string.Empty));

            // Set graph source
            graph.DataSource = dataTable;

            // Set graph options
            graph.Series["Series1"].ChartType = SeriesChartType.Line;

            graph.Series["Series1"].XValueMember = xAxis;
            graph.Series["Series1"].YValueMembers = yAxis;

            graph.Legends["Legend1"].Enabled = true;

            graph.ChartAreas["ChartArea1"].AxisX.MajorGrid.LineWidth = 0;
            graph.ChartAreas["ChartArea1"].AxisX.Interval = a / 10;

            graph.ChartAreas["ChartArea1"].AxisY.Maximum = yMax;
            graph.ChartAreas["ChartArea1"].AxisY.Minimum = yMin;
            graph.ChartAreas["ChartArea1"].AxisY.Interval = yMax / 10;


            graph.DataBind();

        }

        private void detectAnomalies()
        {
            // Create MLContext to be shared across the model creation workflow objects 
            var mlcontext = new MLContext();

            // STEP 1: Common data loading configuration for new data
            IDataView dataView = mlcontext.Data.LoadFromTextFile<ShampooSalesData>(path: filePath, hasHeader: true, separatorChar: commaSeparatedRadio.Checked ? ',' : '\t');

            // Step 2: Load model
            // Note -- The model is trained with the shampoo-sales dataset in a separate console app (see Spike_Model)
            ITransformer trainedModel;
            using (FileStream stream = new FileStream(@"../../../Spike_Model/MLModels/ShampooSalesModel.zip", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                trainedModel = mlcontext.Model.Load(stream);
            }
           
            // Step 3: Apply data transformation to create predictions
            IDataView transformedData = trainedModel.Transform(dataView);
            var predictions = mlcontext.Data.CreateEnumerable<ShampooSalesPrediction>(transformedData, reuseRowObject: false);

            // Index key for dictionary (date, sales)
            int a = 0;

            foreach (var prediction in predictions)
            {
                // Check if anomaly is predicted (indicated by an alert)
                if (prediction.Prediction[0] == 1)
                {
                    // Get the date (year-month) where spike is detected
                    var xAxisDate = dict[a].Item1;
                    // Get the number of sales which was detected to be a spike
                    var yAxisSalesNum = dict[a].Item2;

                    // Add anomaly points to graph
                    // and set point/marker options
                    graph.Series["Series1"].Points[a].SetValueXY(a, yAxisSalesNum);
                    graph.Series["Series1"].Points[a].MarkerStyle = MarkerStyle.Star4;
                    graph.Series["Series1"].Points[a].MarkerSize = 10;
                    graph.Series["Series1"].Points[a].MarkerColor = Color.DarkRed;
                    
                    // Print out anomalies as text for user
                    anomalyText.Text = anomalyText.Text + "Anomaly detected in " + xAxisDate + ": " + yAxisSalesNum + "\n";
                }
                a++;
            }
            Console.WriteLine("");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}