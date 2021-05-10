#if GX_ZLIB
// need Ionic.Zip.dll
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GX
{
	public class ZlibCodec
	{
		public static byte[] Encode(byte[] buf)
		{
			return Ionic.Zlib.ZlibStream.CompressBuffer(buf);
		}
		public static byte[] Encode(ArraySegment<byte> buf)
		{
			using (var mem = new MemoryStream())
			{
				using (var zlib = new Ionic.Zlib.ZlibStream(mem, Ionic.Zlib.CompressionMode.Compress))
				{
					zlib.Write(buf.Array, buf.Offset, buf.Count);
				}
				return mem.ToArray();
			}
		}
		public static void Encode(Stream src, Stream dst)
		{
			using (var zlib = new Ionic.Zlib.ZlibStream(dst, Ionic.Zlib.CompressionMode.Compress, true))
			{
				src.CopyTo(zlib);
			}
		}

		public static byte[] Decode(byte[] buf)
		{
			return Ionic.Zlib.ZlibStream.UncompressBuffer(buf);
		}
		public static byte[] Decode(ArraySegment<byte> buf)
		{
			using (var mem = new MemoryStream(buf.Array, buf.Offset, buf.Count, false))
			using (var zlib = new Ionic.Zlib.ZlibStream(mem, Ionic.Zlib.CompressionMode.Decompress))
			{
				return zlib.ReadAllBytes();
			}
		}
		public static void Decode(Stream src, Stream dst)
		{
			using (var zlib = new Ionic.Zlib.ZlibStream(src, Ionic.Zlib.CompressionMode.Decompress, true))
			{
				zlib.CopyTo(dst);
			}
		}
	}
}
#endif
