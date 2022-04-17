using Microsoft.ML.Transforms.Image;
using System.Drawing;

namespace WeatherRecognition
{
	public class WeatherRecognitionInput
	{
		[ImageType(300, 300)]
		public Bitmap Image { get; set; }
	}
}
