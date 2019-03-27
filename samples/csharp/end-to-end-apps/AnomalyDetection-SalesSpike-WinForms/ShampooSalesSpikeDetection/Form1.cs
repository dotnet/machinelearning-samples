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
        private Hashtable hashTable = null;
        private Hashtable hashTable2 = null;
        private DataTable dataTable = null;
        private string filePath = "";
        private int confidenceLevel = 0;
        private int pValue = 0;
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

            // Num Sales (key) -> Month (value)
            hashTable = new Hashtable();
            // Month (key) -> int (value) for x-axis
            hashTable2 = new Hashtable();
            
            if (filePath != "")
            {
                // Reset text in anomaly textbox
                anomalyText.Text = "";

                // Display preview of dataset and graph
                displayDataTableAndGraph();

                // Set confidence level and p-value
                setConfLevelandPValue();

                // Use ML.NET to detect anomalies and then mark them on the graph
                detectAnomalies();

            }
            // If file path textbox is empty, prompt user to input file path
            else
            {
                MessageBox.Show("Please input file path.");
            }
        }

        private void displayDataTableAndGraph()
        {
            dataTable = new DataTable();
            string[] dataCol = null;
            int a = 0;
            string xAxis = "";
            string yAxis = "";
            string key = "";

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

                // Get quantity on y axis (e.g. number of sales) as string
                key = dataCol[1];

                // Use number of sales as key to date (year-month)
                hashTable.Add(key, dataCol[0]);

                // Use date (year-month) as key to integer a
                hashTable2.Add(dataCol[0], a);
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

            // STEP 1: Common data loading configuration
            IDataView dataView = mlcontext.Data.LoadFromTextFile<ShampooSalesData>(path: filePath, hasHeader:true, separatorChar: commaSeparatedRadio.Checked ? ',' : '\t');

            // Set up IidSpikeDetector arguments
            string outputColumnName = nameof(ShampooSalesPrediction.Prediction);
            string inputColumnName = nameof(ShampooSalesData.numSales);

            // STEP 2: Set the training algorithm    
            var trainingPipeline = mlcontext.Transforms.IidSpikeEstimator(outputColumnName, inputColumnName, confidenceLevel, pValue);

            // STEP 3:Train the model by fitting the dataview
            ITransformer trainedModel = trainingPipeline.Fit(dataView);

            // Apply data transformation to create predictions
            IDataView transformedData = trainedModel.Transform(dataView);
            var predictions = mlcontext.Data.CreateEnumerable<ShampooSalesPrediction>(transformedData, reuseRowObject: false);

            var key = "";
            int m = 0;

            foreach (var prediction in predictions)
            {
                // Check if anomaly is predicted (indicated by an alert)
                if (prediction.Prediction[0] == 1)
                {
                    // Get the value that is predicted to be an anomaly
                    double numPredicted = prediction.Prediction[1];

                    // Use prediction (converted to string) as key to get date from first hashtable
                    // to be able to print out corresponding date to user
                    key = Math.Round(numPredicted, 3).ToString();
    
                    var dateAxisValue = hashTable[key];

                    // Use date as key to get int number from second hashtable
                    // to use as x-axis coordinates for anomaly points/markers
                    var xVal = (hashTable2[dateAxisValue]);
                    m = Convert.ToInt32(hashTable2[dateAxisValue]);
                    
                    // Add anomaly points to graph
                    // and set point/marker options
                    graph.Series["Series1"].Points[m].SetValueXY(xVal, key);
                    graph.Series["Series1"].Points[m].MarkerStyle = MarkerStyle.Star4;
                    graph.Series["Series1"].Points[m].MarkerSize = 10;
                    graph.Series["Series1"].Points[m].MarkerColor = Color.DarkRed;
                    
                    // Print out anomalies as text for user
                    anomalyText.Text = anomalyText.Text + "Anomaly detected in " + dateAxisValue + ": " + key + "\n";
                }
            }
            Console.WriteLine("");
        }

        private void setConfLevelandPValue()
        {
            // Set confidence level and P-value
            // If no values set, 95 and 4 are default values
            if (confTextBox.Text == "")
            {
                confTextBox.Text = "95";
            }
            if (pValueTextbox.Text == "")
            {
                pValueTextbox.Text = "9";
            }
            confidenceLevel = Convert.ToInt32(confTextBox.Text);
            pValue = Convert.ToInt32(pValueTextbox.Text);

        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
