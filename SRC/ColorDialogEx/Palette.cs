using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace ColorDialogEx
{
	[Serializable]
	public class Palette
	{
		public string[] ColorName { get; set; }
		public Color[] PaletteColor { get; set; }

		public Palette()
		{
			PaletteColor = new Color[15];
			ColorName = new string[15];
		}
	}
}
