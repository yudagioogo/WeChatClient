using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;

namespace GX
{
	public interface ISerializable
	{
		void Serialize(Stream s);
		void Deserialize(Stream s);
	}

	public static partial class SerializableExtensions
	{
		public static byte[] Serialize(this ISerializable s)
		{
			if (s == null)
				return new byte[0];
			using (var mem = new System.IO.MemoryStream())
			{
				s.Serialize(mem);
				mem.Flush();
				return mem.ToArray();
			}
		}

		public static void Deserialize(this ISerializable s, byte[] data)
		{
			using (var mem = new System.IO.MemoryStream(data))
			{
				s.Deserialize(mem);
			}
		}

		public static T Clone<T>(this T s) where T : ISerializable
		{
			var other = (T)Activator.CreateInstance(s.GetType());
			other.Deserialize(s.Serialize());
			return other;
		}
	}

	public interface IXmlSerializable
	{
		XElement Serialize();
		void Deserialize(XElement xml);
	}

	public static class XmlSerializableExtensions
	{
		public static string ToXmlString(this IXmlSerializable xml)
		{
			return xml.Serialize().ToString();
		}
	}
}
