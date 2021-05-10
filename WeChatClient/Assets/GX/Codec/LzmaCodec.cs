using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SevenZip;

namespace GX
{
	/// <summary>
	/// ref: LZMA SDK-9.20: 7zip\Compress\LzmaAlone\LzmaAlone.cs
	/// </summary>
	public class LzmaCodec
	{
		/// <summary>
		/// lzma encode
		/// </summary>
		/// <param name="src"></param>
		/// <param name="dst"></param>
		/// <param name="algorithm">mode, default: 2</param>
		/// <param name="dictionary">-d{N}:  set dictionary - 1 << [0, 29], default: 1 << 23 (8MB)</param>
		/// <param name="numFastBytes">-fb{N}: set number of fast bytes - [5, 273], default: 128</param>
		/// <param name="litContextBits">-lc{N}: set number of literal context bits - [0, 8], default: 3</param>
		/// <param name="litPosBits">-lp{N}: set number of literal pos bits - [0, 4], default: 0</param>
		/// <param name="posStateBits">-pb{N}: set number of pos bits - [0, 4], default: 2</param>
		/// <param name="matchFinder">-mf{MF_ID}: set Match Finder: [bt2, bt4], default: bt4</param>
		public static void Encode(
			Stream src, Stream dst, int algorithm = 2, 
			int dictionary = 1 << 23, int numFastBytes = 128, int litContextBits = 3, int litPosBits = 0, int posStateBits = 2, string matchFinder = "bt4")
		{
			CoderPropID[] propIDs = 
			{
				CoderPropID.DictionarySize,
				CoderPropID.PosStateBits,
				CoderPropID.LitContextBits,
				CoderPropID.LitPosBits,
				CoderPropID.Algorithm,
				CoderPropID.NumFastBytes,
				CoderPropID.MatchFinder,
				CoderPropID.EndMarker
			};
			object[] properties = 
			{
				dictionary,
				posStateBits,
				litContextBits,
				litPosBits,
				algorithm,
				numFastBytes,
				matchFinder,
				false
			};

			var encoder = new SevenZip.Compression.LZMA.Encoder();
			encoder.SetCoderProperties(propIDs, properties);
			encoder.WriteCoderProperties(dst);
			long fileSize = src.Length;
			for (int i = 0; i < 8; i++)
				dst.WriteByte((Byte)(fileSize >> (8 * i)));
			encoder.Code(src, dst, -1, -1, null);
		}

		/// <summary>
		/// lzma decode
		/// </summary>
		/// <param name="src"></param>
		/// <param name="dst"></param>
		public static void Decode(Stream src, Stream dst)
		{
			byte[] properties = new byte[5];
			if (src.Read(properties, 0, 5) != 5)
				throw (new Exception("input .lzma is too short"));
			var decoder = new SevenZip.Compression.LZMA.Decoder();
			decoder.SetDecoderProperties(properties);
			long outSize = 0;
			for (int i = 0; i < 8; i++)
			{
				int v = src.ReadByte();
				if (v < 0)
					throw (new Exception("Can't Read 1"));
				outSize |= ((long)(byte)v) << (8 * i);
			}
			long compressedSize = src.Length - src.Position;
			decoder.Code(src, dst, compressedSize, outSize, null);
		}
		public static byte[] Encode(byte[] buf)
		{
			using(var src = new MemoryStream(buf, false))
			using (var dst = new MemoryStream())
			{
				Encode(src, dst);
				return dst.ToArray();
			}
		}

		public static byte[] Decode(byte[] buf)
		{
			using (var src = new MemoryStream(buf, false))
			using (var dst = new MemoryStream())
			{
				Decode(src, dst);
				return dst.ToArray();
			}
		}
	}
}
