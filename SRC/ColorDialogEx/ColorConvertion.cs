using System;
using System.Windows.Media;

namespace ColorDialogEx
{
	internal class HSL
	{
		private double _h, _s, _l;

		internal HSL()
		{
			_h = 0; _s = 1; _l = 0.5;
		}

		public HSL(double H, double S, double L)
		{
			_h = H; _s = S; _l = L;
		}

		public double H
		{
			get { return _h; }
			set { _h = value > 1 ? 1 : value < 0 ? 0 : value; }
		}

		public double S
		{
			get { return _s; }
			set { _s = value > 1 ? 1 : value < 0 ? 0 : value; }
		}

		public double L
		{
			get { return _l; }
			set { _l = value > 1 ? 1 : value < 0 ? 0 : value; }
		}
	}

	internal static class HSLConverter
	{
		public static HSL RgbToHsl(Color color)
		{
			double r = color.R;
			double g = color.G;
			double b = color.B;

			double hue = 0;

			if ((r >= g) && (g >= b))
				hue = 60 * (g - b) / (r - b);
			else if ((g > r) && (r >= b))
				hue = 60 * (2 - (r - b) / (g - b));
			else if ((g >= b) && (b > r))
				hue = 60 * (2 + (b - r) / (g - r));
			else if ((b > g) && (g > r))
				hue = 60 * (4 - (g - r) / (b - r));
			else if ((b > r) && (r >= g))
				hue = 60 * (4 + (r - g) / (b - g));
			else if ((r >= b) && (b > g))
				hue = 60 * (6 - (b - g) / (r - g));
			
			r = r / 255d;
			g = g / 255d;
			b = b / 255d;

			double var_Min = Math.Min(Math.Min(r, g), b);
			double var_Max = Math.Max(Math.Max(r, g), b);
			double del_Max = var_Max - var_Min;

			double l = ((var_Max + var_Min) / 2.0);

			double s;
			if (del_Max == 0)
			{
				hue = s = 0;
			}
			else
			{
				if (l < 0.5) s = del_Max / (var_Max + var_Min);
				else s = (del_Max / (2.0 - var_Max - var_Min));
			}

			HSL result = new HSL(hue, s, l);
			return result;
		}

		public static Color HslToRgb(HSL hsl)
		{
			hsl.H = hsl.H / 360d;

			Color rgb = new Color();

			double hue = hsl.H;
			double saturation = hsl.S;
			double lightness = hsl.L;

			rgb.A = 255;

			if (saturation == 0)
			{
				rgb.R = rgb.G = rgb.B = (byte)(lightness * 255);
			}
			else
			{
				double tmp1, tmp2;

				if (lightness < 0.5) 
					tmp2 = lightness * (1d + saturation);
				else 
					tmp2 = (lightness + saturation) - (saturation * lightness);

				tmp1 = 2 * lightness - tmp2;

				rgb.R = (byte) Math.Round(255 * Hue2Rgb(tmp1, tmp2, hue + (1d / 3d)));
				rgb.G = (byte) Math.Round(255 * Hue2Rgb(tmp1, tmp2, hue));
				rgb.B = (byte) Math.Round(255 * Hue2Rgb(tmp1, tmp2, hue - (1d / 3d)));
			}

			return rgb;
		}

		private static double Hue2Rgb(double v1, double v2, double vH)
		{
			if (vH < 0)
				vH += 1.0F;
			if (vH > 1)
				vH -= 1.0F;
			if ((6 * vH) < 1)
				return (v1 + (v2 - v1) * 6 * vH);
			if ((2 * vH) < 1)
				return (v2);
			if ((3 * vH) < 2)
				return (v1 + (v2 - v1) * ((2d / 3d) - vH) * 6);
			return (v1);
		}
	}
}
