using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ColorDialogEx;
using Microsoft.Win32;

namespace ColorDialogExTest
{
	public partial class Window1 : Window
	{
		private Color currentColor = Colors.Red;

		private WriteableBitmap WBBackup1;
		private WriteableBitmap WBBackup2;

		private WriteableBitmap WBMain;  // main writeable bitmap
		private WriteableBitmap WBFront; // frontbuffer 

		public Window1()
		{
			InitializeComponent();
		}

		private void Window_Loaded_1(object sender, RoutedEventArgs e)
		{
			CreateWritableBitmap();

			img.Source = WBMain;

			img.Stretch = Stretch.None;
			img.HorizontalAlignment = HorizontalAlignment.Left;
			img.VerticalAlignment = VerticalAlignment.Top;

			imgf.Source = WBFront;

			imgf.Stretch = Stretch.None;
			imgf.HorizontalAlignment = HorizontalAlignment.Left;
			imgf.VerticalAlignment = VerticalAlignment.Top;


			img.MouseLeftButtonDown += (i_MouseLeftButtonDown);
			img.MouseRightButtonDown += (i_MouseRightButtonDown);


			imgf.PreviewMouseLeftButtonDown += i_MouseLeftButtonDown;
			imgf.PreviewMouseLeftButtonUp += i_MouseLeftButtonUp;
			imgf.PreviewMouseRightButtonDown += i_MouseRightButtonDown;
			imgf.PreviewMouseRightButtonUp += i_MouseRightButtonUp;

			imgf.MouseMove += (i_MouseMove);
		}

		private void btn1_Click(object sender, RoutedEventArgs e)
		{
			ColorDialogExS DLG = new ColorDialogExS(EStyles.LightStyle, currentColor);
			DLG.Owner = this;

			if (DLG.ShowDialog() == true)
			{
				currentColor = DLG.ColorARGB;
			}
		}

		private void btn2_Click(object sender, RoutedEventArgs e)
		{
			ColorDialogExS DLG = new ColorDialogExS(EStyles.DarkStyle, currentColor);
			DLG.Owner = this;

			if (DLG.ShowDialog() == true)
			{
				currentColor = DLG.ColorARGB;
			}
		}

		private void btn3_Click(object sender, RoutedEventArgs e)
		{
			ColorDialogExB DLG = new ColorDialogExB(EStyles.LightStyle, currentColor);
			DLG.Owner = this;

			if (DLG.ShowDialog() == true)
			{
				currentColor = DLG.ColorARGB;
			}
		}

		private void btn4_Click(object sender, RoutedEventArgs e)
		{
			ColorDialogExB DLG = new ColorDialogExB(EStyles.DarkStyle, currentColor);
			DLG.Owner = this;

			if (DLG.ShowDialog() == true)
			{
				currentColor = DLG.ColorARGB;
			}
		}

		private void CreateWritableBitmap()
		{
			if (img.Width < 16 || img.Height < 16)
				return;

			RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);
			RenderOptions.SetEdgeMode(img, EdgeMode.Aliased);

			RenderOptions.SetBitmapScalingMode(imgf, BitmapScalingMode.NearestNeighbor);
			RenderOptions.SetEdgeMode(imgf, EdgeMode.Aliased);

			int red, green, blue, alpha;

			// back image 
			WBMain = new WriteableBitmap((int) img.Width, (int) img.Height, 96, 96, PixelFormats.Bgra32, null);
			uint[] pixels = new uint[WBMain.PixelWidth * WBMain.PixelHeight];

			for (int x = 0; x < WBMain.PixelWidth; ++x)
			{
				for (int y = 0; y < WBMain.PixelHeight; ++y)
				{
					int i = (WBMain.PixelWidth * y + x);

					red = green = blue = alpha= 255;

					pixels[i] = (uint)((alpha << 24) + (red << 16) + (green << 8) + blue);
				}
			}

			WBMain.WritePixels(new Int32Rect(0, 0, WBMain.PixelWidth, WBMain.PixelHeight), pixels, WBMain.PixelWidth * WBMain.Format.BitsPerPixel / 8, 0);

			// front image is transparent
			WBFront = new WriteableBitmap((int)img.Width, (int)img.Height, 96, 96, PixelFormats.Bgra32, null);
			for (int x = 0; x < WBFront.PixelWidth; ++x)
			{
				for (int y = 0; y < WBFront.PixelHeight; ++y)
				{
					int i = (WBFront.PixelWidth * y + x);

					red = green = blue = alpha = 0;

					pixels[i] = (uint)((alpha << 24) + (red << 16) + (green << 8) + blue);
				}
			}
			WBFront.WritePixels(new Int32Rect(0, 0, WBFront.PixelWidth, WBFront.PixelHeight), pixels, WBFront.PixelWidth * WBFront.Format.BitsPerPixel / 8, 0);
		}

		private void CombineWB()
		{
			if (WBBackup1 != null)
				WBBackup2 = WBBackup1.Clone();
			WBBackup1 = WBMain.Clone();

			Int32Rect rect = new Int32Rect(0, 0, WBMain.PixelWidth, WBMain.PixelHeight);

			int stride = rect.Width * (WBMain.Format.BitsPerPixel / 8);
			byte[] src = new byte[stride * WBMain.PixelHeight];
			WBFront.CopyPixels(rect, src, rect.Width * WBMain.Format.BitsPerPixel / 8, 0);

			byte[] dst = new byte[stride * rect.Height];
			WBMain.CopyPixels(rect, dst, rect.Width * WBMain.Format.BitsPerPixel / 8, 0);


			byte red, green, blue, alpha;

			//float srcA = currentColor.A / 255f;
			//float outA = srcA + (1 * (1 - srcA));
			float mult = currentColor.A / 255f;

			for (int x = 0; x < rect.Width; ++x)
			{
				for (int y = 0; y < rect.Height; ++y)
				{
					int k = (rect.Width * y + x) * 4;

					int sB = (src[k + 0]);
					int sG = (src[k + 1]);
					int sR = (src[k + 2]);
					int sA = (src[k + 3]);

					int dB = (dst[k + 0]);
					int dG = (dst[k + 1]);
					int dR = (dst[k + 2]);


					alpha = 255;
					if (sA > 0)
					{
						//red = (byte)((sR * 1 + dR * 1 * (1 - srcA)) / outA);
						//green = (byte)((sG * 1 + dG * 1 * (1 - srcA)) / outA);
						//blue = (byte)((sB * 1 + dB * 1 * (1 - srcA)) / outA);

						red = (byte)(sR * mult + dR * (1 - mult));
						green = (byte)(sG * mult + dG * (1 - mult));
						blue = (byte)(sB * mult + dB * (1 - mult));
					}
					else
					{
						red = (byte)dR;
						green = (byte)dG;
						blue = (byte)dB;
					}

					dst[k + 0] = blue;
					dst[k + 1] = green;
					dst[k + 2] = red;
					dst[k + 3] = alpha;

					src[k + 0] = 0;
					src[k + 1] = 0;
					src[k + 2] = 0;
					src[k + 3] = 0;
				}
			}

			WBFront.WritePixels(rect, src, rect.Width * WBMain.Format.BitsPerPixel / 8, 0);
			WBMain.WritePixels(rect, dst, rect.Width * WBMain.Format.BitsPerPixel / 8, 0);
		}

		private void DrawPixel(MouseEventArgs e)
		{
			int X = (int)e.GetPosition(img).X;
			int Y = (int)e.GetPosition(img).Y;

			if (X - slider1.Value < 0) X = (int) slider1.Value;
			if (X + slider1.Value > WBMain.PixelWidth) X = (int) (WBMain.PixelWidth - slider1.Value);
			if (Y - slider1.Value < 0) Y = (int) slider1.Value;
			if (Y + slider1.Value > WBMain.PixelHeight) Y = (int) (WBMain.PixelHeight - slider1.Value);

			Int32Rect rect = new Int32Rect((int)(X - slider1.Value), (int)(Y - slider1.Value), (int)(slider1.Value * 2), (int)(slider1.Value * 2));
			
			uint[] pixels = new uint[rect.Width * rect.Height];

			for (int x = 0; x < rect.Width; ++x)
			{
				for (int y = 0; y < rect.Height; ++y)
				{
					int i = (rect.Width * y + x);

					int alpha = currentColor.A;
					int red = currentColor.R;
					int green = currentColor.G;
					int blue = currentColor.B;


					pixels[i] = (uint)((alpha << 24) + (red << 16) + (green << 8) + blue);
				}
			}

			//if (currentColor.A != 255 )
				WBFront.WritePixels(rect, pixels, rect.Width * WBFront.Format.BitsPerPixel / 8, 0);
			//else
			//	WB.WritePixels(rect, pixels, rect.Width * WBFront.Format.BitsPerPixel / 8, 0);
		}

		private void ErasePixel(MouseEventArgs e)
		{
			int X = (int)e.GetPosition(img).X;
			int Y = (int)e.GetPosition(img).Y;

			if (X - slider1.Value < 0) X = (int)slider1.Value;
			if (X + slider1.Value > WBMain.PixelWidth) X = (int)(WBMain.PixelWidth - slider1.Value);
			if (Y - slider1.Value < 0) Y = (int)slider1.Value;
			if (Y + slider1.Value > WBMain.PixelHeight) Y = (int)(WBMain.PixelHeight - slider1.Value);

			Int32Rect rect = new Int32Rect((int)(X - slider1.Value), (int)(Y - slider1.Value), (int)(slider1.Value * 2), (int)(slider1.Value * 2));

			uint[] pixels = new uint[(rect.Width * rect.Height)];

			float mult = currentColor.A / 255f;

			for (int x = 0; x < rect.Width; ++x)
			{
				for (int y = 0; y < rect.Height; ++y)
				{
					int i = (rect.Width * y + x);

					int alpha = 255;
					int red = 255;
					int green = 255;
					int blue = 255;

					pixels[i] = (uint)((alpha << 24) + (red << 16) + (green << 8) + blue);
				}
			}

			WBMain.WritePixels(rect, pixels, rect.Width * WBMain.Format.BitsPerPixel / 8, 0);
		}

		private void i_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			imgf.CaptureMouse();
			DrawPixel(e);
		}

		private void i_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			imgf.ReleaseMouseCapture();

			//if (currentColor.A != 255)
				CombineWB();
		}

		private void i_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			imgf.CaptureMouse();
			ErasePixel(e);
		}

		private void i_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			imgf.ReleaseMouseCapture();
		}

		private void i_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				DrawPixel(e);
			}
			if (e.RightButton == MouseButtonState.Pressed)
			{
				ErasePixel(e);
			}
		}

		private void btnUndo_Click(object sender, RoutedEventArgs e)
		{
			if (WBBackup1 != null)
			{
				WBMain = WBBackup1.Clone();
				if (WBBackup2 != null)
					WBBackup1 = WBBackup2.Clone();

				img.Source = WBMain;
			}
		}

		private void btnLoad_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				OpenFileDialog OFD = new OpenFileDialog();
				OFD.Filter = "PNG images (.png)|*.png";

				if (OFD.ShowDialog() == true)
				{
					BitmapSource bs = BitmapFrame.Create(new Uri(OFD.FileName), BitmapCreateOptions.None, BitmapCacheOption.Default);
					if (bs.Format != PixelFormats.Bgra32)
						bs = new FormatConvertedBitmap(bs, PixelFormats.Bgra32, null, 1); 

					WriteableBitmap wbmp = new WriteableBitmap(bs.PixelWidth, bs.PixelHeight, bs.DpiX, bs.DpiY, bs.Format, bs.Palette);
					
					Int32Rect rect = new Int32Rect(0, 0, bs.PixelWidth, bs.PixelHeight);
					wbmp.Lock();
					bs.CopyPixels(rect, wbmp.BackBuffer, wbmp.BackBufferStride * wbmp.PixelHeight, wbmp.BackBufferStride);
					wbmp.AddDirtyRect(rect);
					wbmp.Unlock();
					WBMain = wbmp.Clone();
					img.Source = WBMain;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				SaveFileDialog SFD = new SaveFileDialog();
				SFD.Filter = "PNG images (.png)|*.png";

				if (SFD.ShowDialog() == true)
				{
					using (FileStream stream5 = new FileStream(SFD.FileName, FileMode.Create, FileAccess.Write))
					{
						PngBitmapEncoder encoder = new PngBitmapEncoder();
						encoder.Frames.Add(BitmapFrame.Create(WBMain));
						encoder.Save(stream5);
						stream5.Close();
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

	}
}
