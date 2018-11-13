using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Serialization;

namespace ColorDialogEx
{
	internal static class SerializeHelper
	{
		static public void Serialize(Object o, Type objectType, string filename)
		{
			XmlSerializer s = new XmlSerializer(objectType);
			using (TextWriter w = new StreamWriter(filename))
			{
				s.Serialize(w, o);
				w.Close();
			}
		}

		static public Object Deserialize(Type objectType, string filename)
		{
			object newList = null;
			
			if (!File.Exists(filename))
				return null;

			XmlSerializer s = new XmlSerializer(objectType);
			using (TextReader r = new StreamReader(filename))
			{
				newList = s.Deserialize(r);
				r.Close();
			}

			return newList;
		}
	}
}
