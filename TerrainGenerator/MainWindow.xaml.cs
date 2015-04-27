using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;


namespace TerrainGenerator {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public Bitmap perlinMap;

		public MainWindow() {
			InitializeComponent();
		}

		private void GenerateBtn_Click(object sender, RoutedEventArgs e) {
			Boolean[] OctActive = new Boolean[8];
			Int32[] OctAmp = new Int32[8];
			Int32[] OctFreq = new Int32[8];
			OctActive[0] = Oct1Chk.IsChecked ?? false; OctActive[1] = Oct2Chk.IsChecked ?? false;
			OctActive[2] = Oct3Chk.IsChecked ?? false; OctActive[3] = Oct4Chk.IsChecked ?? false;
			OctActive[4] = Oct5Chk.IsChecked ?? false; OctActive[5] = Oct6Chk.IsChecked ?? false;
			OctActive[6] = Oct7Chk.IsChecked ?? false; OctActive[7] = Oct8Chk.IsChecked ?? false;
			OctAmp[0] = Convert.ToInt32(Oct1Amp.Value); OctAmp[1] = Convert.ToInt32(Oct2Amp.Value);
			OctAmp[2] = Convert.ToInt32(Oct3Amp.Value); OctAmp[3] = Convert.ToInt32(Oct4Amp.Value);
			OctAmp[4] = Convert.ToInt32(Oct5Amp.Value); OctAmp[5] = Convert.ToInt32(Oct6Amp.Value);
			OctAmp[6] = Convert.ToInt32(Oct7Amp.Value); OctAmp[7] = Convert.ToInt32(Oct8Amp.Value);
			OctFreq[0] = Convert.ToInt32(Oct1Freq.Value); OctFreq[1] = Convert.ToInt32(Oct2Freq.Value);
			OctFreq[2] = Convert.ToInt32(Oct3Freq.Value); OctFreq[3] = Convert.ToInt32(Oct4Freq.Value);
			OctFreq[4] = Convert.ToInt32(Oct5Freq.Value); OctFreq[5] = Convert.ToInt32(Oct6Freq.Value);
			OctFreq[6] = Convert.ToInt32(Oct7Freq.Value); OctFreq[7] = Convert.ToInt32(Oct8Freq.Value);
			PerlinNoise perlinNoise = new PerlinNoise((int)(DateTime.Now.Ticks << 10));
			perlinMap = new Bitmap(Convert.ToInt32(PerlinMapImage.Width), Convert.ToInt32(PerlinMapImage.Height));
			double widthDivisor = 1 / (double)PerlinMapImage.Width;
			double heightDivisor = 1 / (double)PerlinMapImage.Height;
			perlinMap.SetEachPixelColour(
				(point, color) => {
					// Note that the result from the noise function is in the range -1 to 1, but I want it in the range of 0 to 1
					// that's the reason of the strange code
					double v = 0;
					int OctaveCounter = 0;
					for (int i = 0; i < OctActive.Length; i++) {
						if (OctActive[i]) {
							switch (OctaveCounter) {
								// Not happy with this, but I guess it'll do. Too many octaves seem to be dumb.
								case 0: v += (perlinNoise.Noise(OctFreq[i] * point.X * widthDivisor, OctFreq[i] * point.Y * heightDivisor, OctAmp[i]) + 1) / 2 * 0.7; break;
								case 1: v += (perlinNoise.Noise(OctFreq[i] * point.X * widthDivisor, OctFreq[i] * point.Y * heightDivisor, OctAmp[i]) + 1) / 2 * 0.5; break;
								case 2: v += (perlinNoise.Noise(OctFreq[i] * point.X * widthDivisor, OctFreq[i] * point.Y * heightDivisor, OctAmp[i]) + 1) / 2 * 0.3; break;
								case 3: v += (perlinNoise.Noise(OctFreq[i] * point.X * widthDivisor, OctFreq[i] * point.Y * heightDivisor, OctAmp[i]) + 1) / 2 * 0.1; break;
								case 4: v += (perlinNoise.Noise(OctFreq[i] * point.X * widthDivisor, OctFreq[i] * point.Y * heightDivisor, OctAmp[i]) + 1) / 2 * 0.05; break;
								case 5: v += (perlinNoise.Noise(OctFreq[i] * point.X * widthDivisor, OctFreq[i] * point.Y * heightDivisor, OctAmp[i]) + 1) / 2 * 0.03; break;
								case 6: v += (perlinNoise.Noise(OctFreq[i] * point.X * widthDivisor, OctFreq[i] * point.Y * heightDivisor, OctAmp[i]) + 1) / 2 * 0.01; break;
								case 7: v += (perlinNoise.Noise(OctFreq[i] * point.X * widthDivisor, OctFreq[i] * point.Y * heightDivisor, OctAmp[i]) + 1) / 2 * 0.005; break;
							}
							OctaveCounter++;
                        }
					}
					v = Math.Min(1, Math.Max(0, v));
					byte b = (byte)(v * 255);
					return System.Drawing.Color.FromArgb(b, b, b);
				});
			DisplayBitmap(perlinMap);
		}

		private void DisplayBitmap(Bitmap bitmap) {
			MemoryStream ms = new MemoryStream();
			bitmap.Save(ms, ImageFormat.Png);
			ms.Seek(0, SeekOrigin.Begin);
			BitmapImage bi = new BitmapImage();
			bi.BeginInit();
			bi.StreamSource = ms;
			bi.EndInit();
			PerlinMapImage.Source = bi;
		}
		private void CutOffTerrain_Click(object sender, RoutedEventArgs e) {
			if (perlinMap != null) {
				Bitmap bitmap = new Bitmap(Convert.ToInt32(PerlinMapImage.Width), Convert.ToInt32(PerlinMapImage.Height));
				bitmap.SetEachPixelColour(
					(point, color) => {
						Color origColor = perlinMap.GetPixel(point.X, point.Y);
						if (origColor.R > CutOffLevel.Value) {
							return origColor;
						} else {
							return System.Drawing.Color.FromArgb(255, 255, 255);
						}
					});
				DisplayBitmap(bitmap);
			}
        }

		private void CutOffLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			//CutOffTerrain_Click(null, null);
        }
	}
}
