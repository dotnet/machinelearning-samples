using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OnnxObjectDetectionStreamingApp
{
    /// <summary>
    /// Interaction logic for TestDrawing.xaml
    /// </summary>
    public partial class TestDrawing : Page
    {
        public TestDrawing()
        {
            InitializeComponent();

            var rect = new Rectangle
            {
                Stroke = new SolidColorBrush(Colors.Red),
                Fill = new SolidColorBrush(Colors.Transparent),
                Width = 200,
                Height = 200
            };

            DrawingCanvas.Children.Add(rect);
        }
    }
}
