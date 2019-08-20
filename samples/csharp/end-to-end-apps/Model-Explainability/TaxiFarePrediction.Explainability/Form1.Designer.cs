
namespace TaxiFareRegression.Explainability
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
            this.plot1 = new OxyPlot.WindowsForms.PlotView();
            this.button1 = new System.Windows.Forms.Button();
            this.lblTrip = new System.Windows.Forms.Label();
            this.lblTripID = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblFare = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // plot1
            // 
            this.plot1.Location = new System.Drawing.Point(16, 53);
            this.plot1.Name = "plot1";
            this.plot1.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plot1.Size = new System.Drawing.Size(550, 357);
            this.plot1.TabIndex = 0;
            this.plot1.Text = "plot1";
            this.plot1.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plot1.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plot1.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            this.plot1.Click += new System.EventHandler(this.Plot1_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(491, 437);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Next";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // lblTrip
            // 
            this.lblTrip.AutoSize = true;
            this.lblTrip.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTrip.Location = new System.Drawing.Point(13, 13);
            this.lblTrip.Name = "lblTrip";
            this.lblTrip.Size = new System.Drawing.Size(58, 24);
            this.lblTrip.TabIndex = 2;
            this.lblTrip.Text = "Trip #";
            this.lblTrip.Click += new System.EventHandler(this.Label1_Click);
            // 
            // lblTripID
            // 
            this.lblTripID.AutoSize = true;
            this.lblTripID.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTripID.Location = new System.Drawing.Point(66, 13);
            this.lblTripID.Name = "lblTripID";
            this.lblTripID.Size = new System.Drawing.Size(20, 24);
            this.lblTripID.TabIndex = 3;
            this.lblTripID.Text = "1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(92, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(134, 24);
            this.label1.TabIndex = 4;
            this.label1.Text = "Predicted Fare";
            this.label1.Click += new System.EventHandler(this.Label1_Click_1);
            // 
            // lblFare
            // 
            this.lblFare.AutoSize = true;
            this.lblFare.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFare.Location = new System.Drawing.Point(232, 13);
            this.lblFare.Name = "lblFare";
            this.lblFare.Size = new System.Drawing.Size(45, 24);
            this.lblFare.TabIndex = 5;
            this.lblFare.Text = "1.20";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(410, 437);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 6;
            this.button2.Text = "Previous";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.Button2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(593, 472);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.lblFare);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblTripID);
            this.Controls.Add(this.lblTrip);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.plot1);
            this.Name = "Form1";
            this.Text = "Example 1 (WindowsForms)";
            this.Load += new System.EventHandler(this.Form1_Load_1);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button1;
        public OxyPlot.WindowsForms.PlotView plot1;
        private System.Windows.Forms.Label lblTrip;
        private System.Windows.Forms.Label lblTripID;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblFare;
        private System.Windows.Forms.Button button2;
    }
}

