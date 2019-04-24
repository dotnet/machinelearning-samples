namespace SpikeDetection.WinForms
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.LegendItem legendItem1 = new System.Windows.Forms.DataVisualization.Charting.LegendItem();
            System.Windows.Forms.DataVisualization.Charting.LegendItem legendItem2 = new System.Windows.Forms.DataVisualization.Charting.LegendItem();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.debugInstructionsLabel = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.helloWorldLabel = new System.Windows.Forms.Label();
            this.filePathTextbox = new System.Windows.Forms.TextBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.button2 = new System.Windows.Forms.Button();
            this.performanceCounter1 = new System.Diagnostics.PerformanceCounter();
            this.commaSeparatedRadio = new System.Windows.Forms.RadioButton();
            this.tabSeparatedRadio = new System.Windows.Forms.RadioButton();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.backgroundWorker2 = new System.ComponentModel.BackgroundWorker();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.anomalyText = new System.Windows.Forms.RichTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.graph = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.openFileExplorer = new System.Windows.Forms.OpenFileDialog();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.changePointDet = new System.Windows.Forms.CheckBox();
            this.spikeDet = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.performanceCounter1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.graph)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // debugInstructionsLabel
            // 
            this.debugInstructionsLabel.AutoSize = true;
            this.debugInstructionsLabel.Location = new System.Drawing.Point(17, 105);
            this.debugInstructionsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.debugInstructionsLabel.Name = "debugInstructionsLabel";
            this.debugInstructionsLabel.Size = new System.Drawing.Size(154, 25);
            this.debugInstructionsLabel.TabIndex = 1;
            this.debugInstructionsLabel.Text = "Data File Path:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(423, 130);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(121, 46);
            this.button1.TabIndex = 2;
            this.button1.Text = "Find";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // helloWorldLabel
            // 
            this.helloWorldLabel.AutoSize = true;
            this.helloWorldLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.helloWorldLabel.Location = new System.Drawing.Point(13, 25);
            this.helloWorldLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.helloWorldLabel.Name = "helloWorldLabel";
            this.helloWorldLabel.Size = new System.Drawing.Size(385, 51);
            this.helloWorldLabel.TabIndex = 3;
            this.helloWorldLabel.Text = "Anomaly Detection";
            // 
            // filePathTextbox
            // 
            this.filePathTextbox.Location = new System.Drawing.Point(22, 138);
            this.filePathTextbox.Name = "filePathTextbox";
            this.filePathTextbox.Size = new System.Drawing.Size(394, 31);
            this.filePathTextbox.TabIndex = 4;
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(18, 388);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 33;
            this.dataGridView1.Size = new System.Drawing.Size(526, 812);
            this.dataGridView1.TabIndex = 5;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(14, 310);
            this.button2.Margin = new System.Windows.Forms.Padding(4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(544, 46);
            this.button2.TabIndex = 6;
            this.button2.Text = "Go";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // commaSeparatedRadio
            // 
            this.commaSeparatedRadio.AutoSize = true;
            this.commaSeparatedRadio.Checked = true;
            this.commaSeparatedRadio.Location = new System.Drawing.Point(13, 19);
            this.commaSeparatedRadio.Name = "commaSeparatedRadio";
            this.commaSeparatedRadio.Size = new System.Drawing.Size(221, 29);
            this.commaSeparatedRadio.TabIndex = 9;
            this.commaSeparatedRadio.TabStop = true;
            this.commaSeparatedRadio.Text = "Comma Separated";
            this.commaSeparatedRadio.UseVisualStyleBackColor = true;
            // 
            // tabSeparatedRadio
            // 
            this.tabSeparatedRadio.AutoSize = true;
            this.tabSeparatedRadio.Location = new System.Drawing.Point(13, 66);
            this.tabSeparatedRadio.Name = "tabSeparatedRadio";
            this.tabSeparatedRadio.Size = new System.Drawing.Size(185, 29);
            this.tabSeparatedRadio.TabIndex = 10;
            this.tabSeparatedRadio.Text = "Tab Separated";
            this.tabSeparatedRadio.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 360);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(197, 25);
            this.label1.TabIndex = 11;
            this.label1.Text = "Data View Preview:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(600, 130);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(210, 25);
            this.label2.TabIndex = 13;
            this.label2.Text = "Anomalies Detected:";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.Size = new System.Drawing.Size(61, 4);
            // 
            // anomalyText
            // 
            this.anomalyText.ForeColor = System.Drawing.Color.Black;
            this.anomalyText.Location = new System.Drawing.Point(605, 168);
            this.anomalyText.Name = "anomalyText";
            this.anomalyText.Size = new System.Drawing.Size(1194, 174);
            this.anomalyText.TabIndex = 17;
            this.anomalyText.Text = "";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(600, 360);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 25);
            this.label3.TabIndex = 18;
            this.label3.Text = "Graph:";
            // 
            // graph
            // 
            chartArea1.AxisX.Title = "Month";
            chartArea1.AxisY.Maximum = 700D;
            chartArea1.AxisY.Minimum = 0D;
            chartArea1.AxisY.Title = "Sales";
            chartArea1.Name = "ChartArea1";
            this.graph.ChartAreas.Add(chartArea1);
            legendItem1.ImageStyle = System.Windows.Forms.DataVisualization.Charting.LegendImageStyle.Marker;
            legendItem1.MarkerBorderColor = System.Drawing.Color.DarkRed;
            legendItem1.MarkerBorderWidth = 0;
            legendItem1.MarkerColor = System.Drawing.Color.DarkRed;
            legendItem1.MarkerSize = 15;
            legendItem1.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Star4;
            legendItem1.Name = "Spike";
            legendItem2.ImageStyle = System.Windows.Forms.DataVisualization.Charting.LegendImageStyle.Marker;
            legendItem2.MarkerBorderColor = System.Drawing.Color.DarkBlue;
            legendItem2.MarkerBorderWidth = 0;
            legendItem2.MarkerColor = System.Drawing.Color.DarkBlue;
            legendItem2.MarkerSize = 15;
            legendItem2.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Star4;
            legendItem2.Name = "Change point";
            legend1.CustomItems.Add(legendItem1);
            legend1.CustomItems.Add(legendItem2);
            legend1.Enabled = false;
            legend1.Name = "Legend1";
            this.graph.Legends.Add(legend1);
            this.graph.Location = new System.Drawing.Point(605, 388);
            this.graph.Name = "graph";
            this.graph.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.None;
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.Color = System.Drawing.Color.DimGray;
            series1.IsVisibleInLegend = false;
            series1.IsXValueIndexed = true;
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            series2.IsVisibleInLegend = false;
            series2.Legend = "Legend1";
            series2.Name = "Series2";
            this.graph.Series.Add(series1);
            this.graph.Series.Add(series2);
            this.graph.Size = new System.Drawing.Size(1194, 812);
            this.graph.TabIndex = 19;
            this.graph.Text = "graph";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.commaSeparatedRadio);
            this.panel1.Controls.Add(this.tabSeparatedRadio);
            this.panel1.Location = new System.Drawing.Point(14, 183);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(241, 116);
            this.panel1.TabIndex = 22;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.changePointDet);
            this.panel2.Controls.Add(this.spikeDet);
            this.panel2.Location = new System.Drawing.Point(261, 183);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(297, 116);
            this.panel2.TabIndex = 23;
            // 
            // changePointDet
            // 
            this.changePointDet.AutoSize = true;
            this.changePointDet.Location = new System.Drawing.Point(24, 66);
            this.changePointDet.Name = "changePointDet";
            this.changePointDet.Size = new System.Drawing.Size(271, 29);
            this.changePointDet.TabIndex = 25;
            this.changePointDet.Text = "Change Point Detection";
            this.changePointDet.UseVisualStyleBackColor = true;
            // 
            // spikeDet
            // 
            this.spikeDet.AutoSize = true;
            this.spikeDet.Location = new System.Drawing.Point(24, 20);
            this.spikeDet.Name = "spikeDet";
            this.spikeDet.Size = new System.Drawing.Size(195, 29);
            this.spikeDet.TabIndex = 24;
            this.spikeDet.Text = "Spike Detection";
            this.spikeDet.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1841, 1231);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.graph);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.anomalyText);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.filePathTextbox);
            this.Controls.Add(this.helloWorldLabel);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.debugInstructionsLabel);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Anomaly Detection";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            //((System.ComponentModel.ISupportInitialize)(this.performanceCounter1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.graph)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label debugInstructionsLabel;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label helloWorldLabel;
        private System.Windows.Forms.TextBox filePathTextbox;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button button2;
        private System.Diagnostics.PerformanceCounter performanceCounter1;
        private System.Windows.Forms.RadioButton commaSeparatedRadio;
        private System.Windows.Forms.RadioButton tabSeparatedRadio;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.ComponentModel.BackgroundWorker backgroundWorker2;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.RichTextBox anomalyText;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataVisualization.Charting.Chart graph;
        private System.Windows.Forms.OpenFileDialog openFileExplorer;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.CheckBox spikeDet;
        private System.Windows.Forms.CheckBox changePointDet;
    }
}

