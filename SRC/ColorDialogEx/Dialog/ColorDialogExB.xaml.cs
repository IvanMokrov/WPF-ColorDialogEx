using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace ColorDialogEx
{
	/// <summary>
	/// Big version of ColorDialogEx window
	/// </summary>
	public partial class ColorDialogExB : Window
	{

		#region Local variables

		private SolidColorBrush WindowBackground;

		private WriteableBitmap WBColorSetBtnBackground;
		private List<Color> ColorSet;

		private Palette CurrentPalette;

		private Color _frontColor;
		private Color _backColor;

		public Color FrontColor
		{
			get { return _frontColor; }
			set
			{
				_frontColor = value;
				((SolidColorBrush)FrontColorRect.Fill).Color = Color.FromArgb(255, _frontColor.R, _frontColor.G, _frontColor.B);
			}
		}

		public Color BackColor
		{
			get { return _backColor; }
			set
			{
				_backColor = value;
				((SolidColorBrush)BackColorRect1.Fill).Color = Color.FromArgb(255, _backColor.R, _backColor.G, _backColor.B);
				((SolidColorBrush)BackColorRect2.Fill).Color = Color.FromArgb(255, _backColor.R, _backColor.G, _backColor.B);
			}
		}

		#endregion


		#region Override translated color property
		/// <summary>
		/// Color is changed by user
		/// </summary>
		public event EventHandler<ColorEventArgs> ColorChanged;
		protected virtual void OnColorChanged(ColorEventArgs e)
		{
			EventHandler<ColorEventArgs> handler = ColorChanged;
			if (handler != null) handler(this, e);
		}

		/// <summary>
		/// Get or set Hue value
		/// </summary>
		public double H { get { return HSLConverter.RgbToHsl(_frontColor).H; } set { btnToggleColorSet.IsChecked = false; ColorDialog.H = value; } }

		/// <summary>
		/// Get or set Saturation value
		/// </summary>
		public double S { get { return HSLConverter.RgbToHsl(_frontColor).S; } set { btnToggleColorSet.IsChecked = false; ColorDialog.S = value; } }

		/// <summary>
		/// Get or set Lightness value
		/// </summary>
		public double L { get { return HSLConverter.RgbToHsl(_frontColor).L; } set { btnToggleColorSet.IsChecked = false; ColorDialog.L = value; } }

		/// <summary>
		/// Get or set Alpha value
		/// </summary>
		public byte A { get { return _frontColor.A; } set { btnToggleColorSet.IsChecked = false; ColorDialog.A = value; } }

		/// <summary>
		/// Get or set color value w/o specific Alpha
		/// </summary>
		public Color ColorRGB { get { return Color.FromArgb(255, _frontColor.R, _frontColor.G, _frontColor.B); } set { btnToggleColorSet.IsChecked = false; ColorDialog.ColorRGB = value; } }

		/// <summary>
		/// Get or set color value with specific Alpha
		/// </summary>
		public Color ColorARGB { get { return _frontColor; } set { btnToggleColorSet.IsChecked = false; ColorDialog.ColorARGB = value; } }

		#endregion


		/// <summary>
		/// Default constructor w/o specified colors (will be Red)
		/// </summary>
		/// <param name="WindowStyle">Default window style</param>
		public ColorDialogExB(EStyles WindowStyle)
		{
			InitializeComponent();

			this.FrontColor = Colors.Red;
			this.BackColor = Colors.Red;

			ApplyStyle(WindowStyle);
		}

		/// <summary>
		/// Constructor with specified front color. Back color will be the same.
		/// </summary>
		/// <param name="WindowStyle">Default window style</param>
		/// <param name="FrontColor">Default front color</param>
		public ColorDialogExB(EStyles WindowStyle, Color FrontColor)
		{
			InitializeComponent();

			this.FrontColor = FrontColor;
			this.BackColor = FrontColor;

			ApplyStyle(WindowStyle);
		}

		/// <summary>
		/// Constructor with specified front color and back color
		/// </summary>
		/// <param name="WindowStyle">Default window style</param>
		/// <param name="FrontColor">Default front color</param>
		/// <param name="BackColor">Specified back color</param>
		public ColorDialogExB(EStyles WindowStyle, Color FrontColor, Color BackColor)
		{
			InitializeComponent();

			this.FrontColor = FrontColor;
			this.BackColor = BackColor;

			ApplyStyle(WindowStyle);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			txtHTML.IsReadOnly = true;

			DropShadowEffect shadowEffect = new DropShadowEffect();
			shadowEffect.ShadowDepth = 3;
			shadowEffect.BlurRadius = 3;
			shadowEffect.Opacity = 0.4;

			DropShadowEffect shadowEffect2 = new DropShadowEffect();
			shadowEffect2.ShadowDepth = 1;
			shadowEffect2.BlurRadius = 5;
			shadowEffect2.Opacity = 0.4;

			BackColorRect1.Effect = shadowEffect2;
			BackColorRect2.Effect = shadowEffect2;
			FrontColorRect.Effect = shadowEffect;

			// load defaul palette
			CurrentPalette = new Palette();
			try
			{
				string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				path += "\\Palettes";
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
				string filename = path + "\\default.palette";
				Palette tmp = (Palette)SerializeHelper.Deserialize(typeof (Palette), filename);

				ApplyPalette(tmp);
			}
			catch (Exception)
			{
			}

			ColorSet = new List<Color>();
			WBColorSetBtnBackground = new WriteableBitmap((int)btnToggleColorSet.Width, (int)btnToggleColorSet.Height, 96, 96, PixelFormats.Pbgra32, null);
			btnToggleColorSet.Background = new ImageBrush(WBColorSetBtnBackground);
			
			ColorDialog.ColorChanged += OnColorChanged;
			ColorDialog.ColorARGB = FrontColor;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				// save default palette
				string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				path += "\\Palettes";
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
				string filename = path + "\\default.palette";
				SerializeHelper.Serialize(CurrentPalette, typeof(Palette), filename);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		/// <summary>
		/// Apply new style to window and controls
		/// </summary>
		/// <param name="WindowStyle"></param>
		public void ApplyStyle(EStyles WindowStyle)
		{
			if (WindowStyle == EStyles.DarkStyle)
			{
				SolidColorBrush backbrush = this.FindResource("DarkWindowBackground") as SolidColorBrush;
				SolidColorBrush frontbrush = this.FindResource("DControlForeground") as SolidColorBrush;

				Style btnstyle = this.FindResource("DarkButtonStyle") as Style;
				Style txtstyle = this.FindResource("DarkTextBoxStyle") as Style;
				Style palettebtn = this.FindResource("DarkPaletteButtonStyle") as Style;

				WindowBackground = backbrush;
				ApplyStyle(frontbrush, btnstyle, txtstyle, palettebtn);
			}
			else if (WindowStyle == EStyles.LightStyle)
			{
				SolidColorBrush backbrush = this.FindResource("LightWindowBackground") as SolidColorBrush;
				SolidColorBrush frontbrush = this.FindResource("LControlForeground") as SolidColorBrush;

				Style btnstyle = this.FindResource("LightButtonStyle") as Style;
				Style txtstyle = this.FindResource("LightTextBoxStyle") as Style;
				Style palettebtn = this.FindResource("LightPaletteButtonStyle") as Style;

				WindowBackground = backbrush;
				ApplyStyle(frontbrush, btnstyle, txtstyle, palettebtn);
			}
		}

		private void ApplyStyle(SolidColorBrush frontbrush, Style btnstyle, Style txtstyle, Style palettebtn)
		{
			Background = WindowBackground;

			btnOK.Foreground = frontbrush;
			btnCancel.Foreground = frontbrush;
			txtHTML.Foreground = frontbrush;
			txtH.Foreground = frontbrush;
			txtS.Foreground = frontbrush;
			txtL.Foreground = frontbrush;
			txtA.Foreground = frontbrush;
			txtR.Foreground = frontbrush;
			txtG.Foreground = frontbrush;
			txtB.Foreground = frontbrush;

			btnColor0.Style = palettebtn;
			btnColor1.Style = palettebtn;
			btnColor2.Style = palettebtn;
			btnColor3.Style = palettebtn;
			btnColor4.Style = palettebtn;
			btnColor5.Style = palettebtn;
			btnColor6.Style = palettebtn;
			btnColor7.Style = palettebtn;
			btnColor8.Style = palettebtn;
			btnColor9.Style = palettebtn;
			btnColor10.Style = palettebtn;
			btnColor11.Style = palettebtn;
			btnColor12.Style = palettebtn;
			btnColor13.Style = palettebtn;
			btnColor14.Style = palettebtn;

			btnOK.Style = btnstyle;
			btnCancel.Style = btnstyle;
			btnLoadPalette.Style = btnstyle;
			btnSavePalette.Style = btnstyle;

			txtHTML.Style = txtstyle;
			txtH.Style = txtstyle;
			txtS.Style = txtstyle;
			txtL.Style = txtstyle;
			txtA.Style = txtstyle;
			txtR.Style = txtstyle;
			txtG.Style = txtstyle;
			txtB.Style = txtstyle;

			popupRectangle.Fill = WindowBackground;
			txtColorName.Style = txtstyle;
			btnLoadColorBack.Style = btnstyle;
			btnLoadColorFront.Style = btnstyle;
			btnSaveColor.Style = btnstyle;
		}

		private void ApplyPalette(Palette palette)
		{
			if (palette != null)
				CurrentPalette = palette;

			btnColor0.Background = GetPaletteBackground(int.Parse(btnColor0.Tag.ToString()));
			btnColor1.Background = GetPaletteBackground(int.Parse(btnColor1.Tag.ToString()));
			btnColor2.Background = GetPaletteBackground(int.Parse(btnColor2.Tag.ToString()));
			btnColor3.Background = GetPaletteBackground(int.Parse(btnColor3.Tag.ToString()));
			btnColor4.Background = GetPaletteBackground(int.Parse(btnColor4.Tag.ToString()));
			btnColor5.Background = GetPaletteBackground(int.Parse(btnColor5.Tag.ToString()));
			btnColor6.Background = GetPaletteBackground(int.Parse(btnColor6.Tag.ToString()));
			btnColor7.Background = GetPaletteBackground(int.Parse(btnColor7.Tag.ToString()));
			btnColor8.Background = GetPaletteBackground(int.Parse(btnColor8.Tag.ToString()));
			btnColor9.Background = GetPaletteBackground(int.Parse(btnColor9.Tag.ToString()));
			btnColor10.Background = GetPaletteBackground(int.Parse(btnColor10.Tag.ToString()));
			btnColor11.Background = GetPaletteBackground(int.Parse(btnColor11.Tag.ToString()));
			btnColor12.Background = GetPaletteBackground(int.Parse(btnColor12.Tag.ToString()));
			btnColor13.Background = GetPaletteBackground(int.Parse(btnColor13.Tag.ToString()));
			btnColor14.Background = GetPaletteBackground(int.Parse(btnColor14.Tag.ToString()));
		}

		private Brush GetPaletteBackground(int tag)
		{
			return
				CurrentPalette.PaletteColor[tag] != new Color()
				? new SolidColorBrush(Color.FromArgb(255, CurrentPalette.PaletteColor[tag].R, CurrentPalette.PaletteColor[tag].G, CurrentPalette.PaletteColor[tag].B))
				: WindowBackground;
		}

		private void OnColorChanged(object sender, ColorEventArgs colorEventArgs)
		{
			Color C = colorEventArgs.ColorARGB;
			txtA.Text = C.A.ToString();
			txtR.Text = C.R.ToString();
			txtG.Text = C.G.ToString();
			txtB.Text = C.B.ToString();
			txtH.Text = ColorDialog.H.ToString("#0.00");
			txtS.Text = ColorDialog.S.ToString("0.0000");
			txtL.Text = ColorDialog.L.ToString("0.0000");
			txtHTML.Text = C.ToString();

			if (btnToggleColorSet.IsChecked == true)
			{
				if (ColorSet.Count + 1 > WBColorSetBtnBackground.PixelWidth)
					ColorSet.RemoveRange(0, ColorSet.Count - WBColorSetBtnBackground.PixelWidth + 1);
				ColorSet.Add(C);

				// calc middle color from this color set;
				uint rSumm = 0, gSumm = 0, bSumm = 0, aSumm = 0;
				for (int cnt = 0; cnt < ColorSet.Count; cnt++)
				{
					rSumm += ColorSet[cnt].R;
					gSumm += ColorSet[cnt].G;
					bSumm += ColorSet[cnt].B;
					aSumm += ColorSet[cnt].A;
				}
				byte r = (byte)(rSumm / ColorSet.Count);
				byte g = (byte)(gSumm / ColorSet.Count);
				byte b = (byte)(bSumm / ColorSet.Count);
				byte a = (byte)(aSumm / ColorSet.Count);

				FrontColor = Color.FromArgb(a, r, g, b);

				RedrawColorSetBackground();
			}
			else
			{
				FrontColor = C;
			}
		}

		private void RedrawColorSetBackground()
		{
			WriteableBitmap WB = WBColorSetBtnBackground;

			uint[] pixels = new uint[(WB.PixelWidth * WB.PixelHeight)];

			int red, green, blue, alpha;
			for (int x = 0; x < WB.PixelWidth; ++x)
			{
				for (int y = 0; y < WB.PixelHeight; ++y)
				{
					int i = (WB.PixelWidth * y + x);

					if (x < ColorSet.Count)
					{
						red = ColorSet[x].R;
						green = ColorSet[x].G;
						blue = ColorSet[x].B;
						alpha = ColorSet[x].A;
					}
					else
					{
						red = green = blue = alpha = 255;
					}

					pixels[i] = (uint)((alpha << 24) + (red << 16) + (green << 8) + blue);
				}
			}

			WB.WritePixels(new Int32Rect(0, 0, WB.PixelWidth, WB.PixelHeight), pixels, WB.PixelWidth * WB.Format.BitsPerPixel / 8, 0);
		}

		private void txt_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Enter)
				return;

			if (Equals(sender, txtH))
			{
				double d;
				if (double.TryParse(txtH.Text, out d))
					ColorDialog.H = d;
				else
					txtH.Text = ColorDialog.H.ToString("#0.00");
				txtH.SelectAll();
			}
			else if (Equals(sender, txtS))
			{
				double d;
				if (double.TryParse(txtS.Text, out d))
					ColorDialog.S = d;
				else
					txtS.Text = ColorDialog.S.ToString("#0.0000");
				txtS.SelectAll();
			}
			else if (Equals(sender, txtL))
			{
				double d;
				if (double.TryParse(txtL.Text, out d))
					ColorDialog.L = d;
				else
					txtL.Text = ColorDialog.L.ToString("#0.0000");
				txtL.SelectAll();
			}
			else if (Equals(sender, txtA))
			{
				byte d;
				Color c = ColorDialog.ColorARGB;

				if (byte.TryParse(txtA.Text, out d))
					ColorDialog.ColorARGB = Color.FromArgb(d, c.R, c.G, c.B);
				else
					txtA.Text = c.A.ToString();
				txtA.SelectAll();
			}
			else if (Equals(sender, txtR))
			{
				byte d;
				Color c = ColorDialog.ColorARGB;

				if (byte.TryParse(txtR.Text, out d))
					ColorDialog.ColorARGB = Color.FromArgb(c.A, d, c.G, c.B);
				else
					txtR.Text = c.R.ToString();
				txtR.SelectAll();
			}
			else if (Equals(sender, txtG))
			{
				byte d;
				Color c = ColorDialog.ColorARGB;

				if (byte.TryParse(txtG.Text, out d))
					ColorDialog.ColorARGB = Color.FromArgb(c.A, c.R, d, c.B);
				else
					txtG.Text = c.G.ToString();
				txtG.SelectAll();
			}
			else if (Equals(sender, txtB))
			{
				byte d;
				Color c = ColorDialog.ColorARGB;

				if (byte.TryParse(txtB.Text, out d))
					ColorDialog.ColorARGB = Color.FromArgb(c.A, c.R, c.G, d);
				else
					txtB.Text = c.B.ToString();
				txtB.SelectAll();
			}
		}

		private void txtPrevMouseDown(object sender, MouseButtonEventArgs e)
		{
			TextBox tb = (TextBox)sender;
			tb.SelectAll();
			Keyboard.Focus(tb);
			e.Handled = true;
		}

		private void txtGotFocus(object sender, RoutedEventArgs e)
		{
			TextBox tb = (TextBox)sender;
			tb.SelectAll();
		}

		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}

		private void btnTogglePalette_Checked(object sender, System.Windows.RoutedEventArgs e)
		{
			btnToggleColorSet.Content = "ColorSet: enabled";
			OnColorChanged(this, new ColorEventArgs { ColorARGB = FrontColor, ColorRGB = Color.FromArgb(255, FrontColor.R, FrontColor.G, FrontColor.B) });
		}

		private void btnTogglePalette_Unchecked(object sender, System.Windows.RoutedEventArgs e)
		{
			btnToggleColorSet.Content = "ColorSet: disabled";
			FrontColor = ColorDialog.ColorARGB;
		}

		private void btnLoadPalette_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				path += "\\Palettes";
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);

				OpenFileDialog OFD = new OpenFileDialog();
				OFD.InitialDirectory = path;
				OFD.Filter = "ColorDialog palette|*.palette";
				if (OFD.ShowDialog() == true)
				{
					Palette tmp =(Palette)SerializeHelper.Deserialize(typeof(Palette), OFD.FileName);
					ApplyPalette(tmp);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void btnSavePalette_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				path += "\\Palettes";
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);

				SaveFileDialog SFD = new SaveFileDialog();
				SFD.InitialDirectory = path;
				SFD.Filter = "ColorDialog palette|*.palette";
				if (SFD.ShowDialog() == true)
				{
					SerializeHelper.Serialize(CurrentPalette, typeof(Palette), SFD.FileName);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void btnPalette_Click(object sender, RoutedEventArgs e)
		{
			Button target = (Button)sender;
			int idx;

			if (!int.TryParse(target.Tag.ToString(), out idx))
				return;

			if (CurrentPalette.PaletteColor[idx] == new Color())
			{
				btnLoadColorBack.IsEnabled = false;
				btnLoadColorFront.IsEnabled = false;
				txtColorName.Text = FrontColor.ToString();
			}
			else
			{
				btnLoadColorBack.IsEnabled = true;
				btnLoadColorFront.IsEnabled = true;
				txtColorName.Text = CurrentPalette.ColorName[idx];
			}

			palettePopup.PlacementTarget = target;
			palettePopup.StaysOpen = false;
			palettePopup.IsOpen = true;
			txtColorName.Focus();
		}

		private void btnSaveColor_Click(object sender, RoutedEventArgs e)
		{
			Button target = (Button)palettePopup.PlacementTarget;
			int idx;

			if (!int.TryParse(target.Tag.ToString(), out idx))
				return;

			CurrentPalette.PaletteColor[idx] = FrontColor;
			CurrentPalette.ColorName[idx] = txtColorName.Text;
			ApplyPalette(CurrentPalette);

			palettePopup.IsOpen = false;
		}

		private void btnLoadFrontColor_Click(object sender, RoutedEventArgs e)
		{
			Button target = (Button)palettePopup.PlacementTarget;
			int idx;

			if (!int.TryParse(target.Tag.ToString(), out idx))
				return;

			if (CurrentPalette.PaletteColor[idx] == new Color())
				return;

			ColorARGB = CurrentPalette.PaletteColor[idx];

			palettePopup.IsOpen = false;
		}

		private void btnLoadBackColor_Click(object sender, RoutedEventArgs e)
		{
			Button target = (Button)palettePopup.PlacementTarget;
			int idx;

			if (!int.TryParse(target.Tag.ToString(), out idx))
				return;

			if (CurrentPalette.PaletteColor[idx] == new Color())
				return;

			BackColor = CurrentPalette.PaletteColor[idx];

			palettePopup.IsOpen = false;
		}

	}
}
