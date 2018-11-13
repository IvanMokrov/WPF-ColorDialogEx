using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ColorDialogEx
{
	/// <summary>
	/// Separate control of color dialog
	/// </summary>
	public partial class ColorDialogExCtl : Canvas
	{

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
		public double H { get { return _colorDialogLogic.H; } set { _colorDialogLogic.H = value; } }

		/// <summary>
		/// Get or set Saturation value
		/// </summary>
		public double S { get { return _colorDialogLogic.S; } set { _colorDialogLogic.S = value; } }

		/// <summary>
		/// Get or set Lightness value
		/// </summary>
		public double L { get { return _colorDialogLogic.L; } set { _colorDialogLogic.L = value; } }

		/// <summary>
		/// Get or set Alpha value
		/// </summary>
		public byte A { get { return _colorDialogLogic.A; } set { _colorDialogLogic.A = value; } }

		/// <summary>
		/// Get or set color value w/o specific Alpha
		/// </summary>
		public Color ColorRGB { get { return _colorDialogLogic.ColorRGB; } set { _colorDialogLogic.ColorRGB = value; } }

		/// <summary>
		/// Get or set color value with specific Alpha
		/// </summary>
		public Color ColorARGB { get { return _colorDialogLogic.ColorARGB; } set { _colorDialogLogic.ColorARGB = value; } }

		#endregion

		private readonly ColorDialogExLogic _colorDialogLogic;

		public ColorDialogExCtl()
		{
			InitializeComponent();
			_colorDialogLogic = new ColorDialogExLogic(this);
			_colorDialogLogic.ColorChanged += (sender, args) => OnColorChanged(args);
		}

		private void Canvas_SizeChanged_1(object sender, SizeChangedEventArgs e)
		{
			_colorDialogLogic.RefreshResize();
		}
	}
}
