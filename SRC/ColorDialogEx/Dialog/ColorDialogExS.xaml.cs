using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ColorDialogEx
{
	/// <summary>
	/// Small version of ColorDialogEx window
	/// </summary>
	public partial class ColorDialogExS : Window
	{

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


		#region Translated color property
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
		public double H { get { return ColorDialog.H; } set { ColorDialog.H = value; } }

		/// <summary>
		/// Get or set Saturation value
		/// </summary>
		public double S { get { return ColorDialog.S; } set { ColorDialog.S = value; } }

		/// <summary>
		/// Get or set Lightness value
		/// </summary>
		public double L { get { return ColorDialog.L; } set { ColorDialog.L = value; } }

		/// <summary>
		/// Get or set Alpha value
		/// </summary>
		public byte A { get { return ColorDialog.A; } set { ColorDialog.A = value; } }

		/// <summary>
		/// Get or set color value w/o specific Alpha
		/// </summary>
		public Color ColorRGB { get { return ColorDialog.ColorRGB; } set { ColorDialog.ColorRGB = value; } }

		/// <summary>
		/// Get or set color value with specific Alpha
		/// </summary>
		public Color ColorARGB { get { return ColorDialog.ColorARGB; } set { ColorDialog.ColorARGB = value; } }

		#endregion


		/// <summary>
		/// Default constructor w/o specified colors (will be Red)
		/// </summary>
		/// <param name="WindowStyle">Default window style</param>
		public ColorDialogExS(EStyles WindowStyle)
		{
			InitializeComponent();

			this.FrontColor = Colors.Red;
			this.BackColor = Colors.Red;

			if (WindowStyle == EStyles.DarkStyle)
				ApplyStyle(WindowStyle);
		}

		/// <summary>
		/// Constructor with specified front color. Back color will be the same.
		/// </summary>
		/// <param name="WindowStyle">Default window style</param>
		/// <param name="FrontColor">Default front color</param>
		public ColorDialogExS(EStyles WindowStyle, Color FrontColor)
		{
			InitializeComponent();

			this.FrontColor = FrontColor;
			this.BackColor = FrontColor;

			if (WindowStyle == EStyles.DarkStyle)
				ApplyStyle(WindowStyle);
		}

		/// <summary>
		/// Constructor with specified front color and back color
		/// </summary>
		/// <param name="WindowStyle">Default window style</param>
		/// <param name="FrontColor">Default front color</param>
		/// <param name="BackColor">Specified back color</param>
		public ColorDialogExS(EStyles WindowStyle, Color FrontColor, Color BackColor)
		{
			InitializeComponent();

			this.FrontColor = FrontColor;
			this.BackColor = BackColor;

			if (WindowStyle == EStyles.DarkStyle)
				ApplyStyle(WindowStyle);
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

				ApplyStyle(backbrush, frontbrush, btnstyle, txtstyle);
			}
			else if (WindowStyle == EStyles.LightStyle)
			{
				SolidColorBrush backbrush = this.FindResource("LightWindowBackground") as SolidColorBrush;
				SolidColorBrush frontbrush = this.FindResource("LControlForeground") as SolidColorBrush;

				Style btnstyle = this.FindResource("LightButtonStyle") as Style;
				Style txtstyle = this.FindResource("LightTextBoxStyle") as Style;

				ApplyStyle(backbrush, frontbrush, btnstyle, txtstyle);
			}
		}

		private void ApplyStyle(SolidColorBrush backbrush, SolidColorBrush frontbrush, Style btnstyle, Style txtstyle)
		{
			Background = backbrush;

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

			btnOK.Style = btnstyle;
			btnCancel.Style = btnstyle;
			txtHTML.Style = txtstyle;
			txtH.Style = txtstyle;
			txtS.Style = txtstyle;
			txtL.Style = txtstyle;
			txtA.Style = txtstyle;
			txtR.Style = txtstyle;
			txtG.Style = txtstyle;
			txtB.Style = txtstyle;
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

			ColorDialog.ColorChanged += (o, args) =>
			{
				Color C = ColorDialog.ColorARGB;
				txtA.Text = C.A.ToString();
				txtR.Text = C.R.ToString();
				txtG.Text = C.G.ToString();
				txtB.Text = C.B.ToString();
				txtH.Text = ColorDialog.H.ToString("#0.00");
				txtS.Text = ColorDialog.S.ToString("0.0000");
				txtL.Text = ColorDialog.L.ToString("0.0000");
				txtHTML.Text = C.ToString();

				FrontColor = C;
			};

			ColorDialog.ColorARGB = FrontColor;
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

		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
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


	}
}
