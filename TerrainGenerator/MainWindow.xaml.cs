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
		public Bitmap terrainMap;
		public Bitmap foodMap;
		public Bitmap finishedMap;

		public MainWindow() {
			InitializeComponent();
		}

		private void GenerateTerrainBtn_Click(object sender, RoutedEventArgs e) {
			Boolean[] TerrOctActive = new Boolean[4];
			Int32[] TerrOctAmp = new Int32[4];
			Int32[] TerrOctFreq = new Int32[4];
			TerrOctActive[0] = TerrOct1Chk.IsChecked ?? false; TerrOctActive[1] = TerrOct2Chk.IsChecked ?? false;
			TerrOctActive[2] = TerrOct3Chk.IsChecked ?? false; TerrOctActive[3] = TerrOct4Chk.IsChecked ?? false;
			TerrOctAmp[0] = Convert.ToInt32(TerrOct1Amp.Value); TerrOctAmp[1] = Convert.ToInt32(TerrOct2Amp.Value);
			TerrOctAmp[2] = Convert.ToInt32(TerrOct3Amp.Value); TerrOctAmp[3] = Convert.ToInt32(TerrOct4Amp.Value);
			TerrOctFreq[0] = Convert.ToInt32(TerrOct1Freq.Value); TerrOctFreq[1] = Convert.ToInt32(TerrOct2Freq.Value);
			TerrOctFreq[2] = Convert.ToInt32(TerrOct3Freq.Value); TerrOctFreq[3] = Convert.ToInt32(TerrOct4Freq.Value);
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
					for (int i = 0; i < TerrOctActive.Length; i++) {
						if (TerrOctActive[i]) {
							switch (OctaveCounter) {
								case 0: v += (perlinNoise.Noise(TerrOctFreq[i] * point.X * widthDivisor, TerrOctFreq[i] * point.Y * heightDivisor, TerrOctAmp[i]) + 1) / 2 * 0.7; break;
								case 1: v += (perlinNoise.Noise(TerrOctFreq[i] * point.X * widthDivisor, TerrOctFreq[i] * point.Y * heightDivisor, TerrOctAmp[i]) + 1) / 2 * 0.5; break;
								case 2: v += (perlinNoise.Noise(TerrOctFreq[i] * point.X * widthDivisor, TerrOctFreq[i] * point.Y * heightDivisor, TerrOctAmp[i]) + 1) / 2 * 0.3; break;
								case 3: v += (perlinNoise.Noise(TerrOctFreq[i] * point.X * widthDivisor, TerrOctFreq[i] * point.Y * heightDivisor, TerrOctAmp[i]) + 1) / 2 * 0.1; break;
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
				terrainMap = new Bitmap(Convert.ToInt32(PerlinMapImage.Width), Convert.ToInt32(PerlinMapImage.Height));
				terrainMap.SetEachPixelColour(
					(point, color) => {
						Color origColor = perlinMap.GetPixel(point.X, point.Y);
						if (origColor.R < CutOffLevel.Value) {
							return System.Drawing.Color.FromArgb(0, 0, 0);
						} else {
							return System.Drawing.Color.FromArgb(255, 255, 255);
						}
					});
				DisplayBitmap(terrainMap);
			}
        }

		private void CutOffLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			//CutOffTerrain_Click(null, null);
        }

		private void DisplayMap_Click(object sender, RoutedEventArgs e) {
			DisplayBitmap(perlinMap);
		}

		private void GenerateFoodBtn_Click(object sender, RoutedEventArgs e) {
			Boolean[] FoodOctActive = new Boolean[4];
			Int32[] FoodOctAmp = new Int32[4];
			Int32[] FoodOctFreq = new Int32[4];
			FoodOctActive[0] = FoodOct1Chk.IsChecked ?? false; FoodOctActive[1] = FoodOct2Chk.IsChecked ?? false;
			FoodOctActive[2] = FoodOct3Chk.IsChecked ?? false; FoodOctActive[3] = FoodOct4Chk.IsChecked ?? false;
			FoodOctAmp[0] = Convert.ToInt32(FoodOct1Amp.Value); FoodOctAmp[1] = Convert.ToInt32(FoodOct2Amp.Value);
			FoodOctAmp[2] = Convert.ToInt32(FoodOct3Amp.Value); FoodOctAmp[3] = Convert.ToInt32(FoodOct4Amp.Value);
			FoodOctFreq[0] = Convert.ToInt32(FoodOct1Freq.Value); FoodOctFreq[1] = Convert.ToInt32(FoodOct2Freq.Value);
			FoodOctFreq[2] = Convert.ToInt32(FoodOct3Freq.Value); FoodOctFreq[3] = Convert.ToInt32(FoodOct4Freq.Value);
			PerlinNoise perlinNoise = new PerlinNoise((int)(DateTime.Now.Ticks << 10));
			foodMap = (Bitmap)terrainMap.Clone();
			double widthDivisor = 1 / (double)PerlinMapImage.Width;
			double heightDivisor = 1 / (double)PerlinMapImage.Height;
			foodMap.SetEachPixelColour(
				(point, color) => {
					// Note that the result from the noise function is in the range -1 to 1, but I want it in the range of 0 to 1
					// that's the reason of the strange code
					if (color.R == 255) {
						double v = 0;
						int OctaveCounter = 0;
						for (int i = 0; i < FoodOctActive.Length; i++) {
							if (FoodOctActive[i]) {
								switch (OctaveCounter) {
									// Not happy with this, but I guess it'll do. Too many octaves seem to be dumb.
									case 0: v += (perlinNoise.Noise(FoodOctFreq[i] * point.X * widthDivisor, FoodOctFreq[i] * point.Y * heightDivisor, FoodOctAmp[i]) + 1) / 2 * 0.7; break;
									case 1: v += (perlinNoise.Noise(FoodOctFreq[i] * point.X * widthDivisor, FoodOctFreq[i] * point.Y * heightDivisor, FoodOctAmp[i]) + 1) / 2 * 0.5; break;
									case 2: v += (perlinNoise.Noise(FoodOctFreq[i] * point.X * widthDivisor, FoodOctFreq[i] * point.Y * heightDivisor, FoodOctAmp[i]) + 1) / 2 * 0.3; break;
									case 3: v += (perlinNoise.Noise(FoodOctFreq[i] * point.X * widthDivisor, FoodOctFreq[i] * point.Y * heightDivisor, FoodOctAmp[i]) + 1) / 2 * 0.1; break;
								}
								OctaveCounter++;
							}
						}
						v = Math.Min(1, Math.Max(0, v));
						byte b = (byte)(v * 255);
						return System.Drawing.Color.FromArgb(b, 0, 0);
					} else {
						return color;
                    }
				});
			DisplayBitmap(foodMap);
		}

		private void CutOffFood_Click(object sender, RoutedEventArgs e) {
			if (foodMap != null) {
				finishedMap = new Bitmap(Convert.ToInt32(PerlinMapImage.Width), Convert.ToInt32(PerlinMapImage.Height));
				finishedMap.SetEachPixelColour(
					(point, color) => {
						Color origColor = foodMap.GetPixel(point.X, point.Y);
						if (origColor.R != 0) {
							if (origColor.R < FoodCutOff.Value) {
								return System.Drawing.Color.FromArgb(255, 255, 255);
							} else {
								return System.Drawing.Color.FromArgb(255, 0, 0);
							}
						}
						return origColor;
					});
				DisplayBitmap(finishedMap);
			}
		}
	}
}
