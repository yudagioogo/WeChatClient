#if UNITY_METRO && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace GX.WinRT
{
	static class IBufferExtensions
	{
		public static byte[] ToBytes(this IBuffer buffer)
		{
			if (buffer == null)
				return null;
			var ret = new byte[buffer.Length];
			CryptographicBuffer.CopyToByteArray(buffer, out ret);
			return ret;
		}

		public static IBuffer ToIBuffer(this byte[] data)
		{
			if (data == null)
				return null;
			return CryptographicBuffer.CreateFromByteArray(data);
		}

		public static IBuffer ToIBuffer(this byte[] data, int offset, int count)
		{
			if (data == null)
				return null;
			var buf = new ArraySegment<byte>(data, offset, count).Array;
			return CryptographicBuffer.CreateFromByteArray(buf);
		}

		public static IBuffer ToIBuffer(this Stream stream)
		{
			if(stream == null)
				return null;
			var buf = stream.ReadAllBytes();
			return CryptographicBuffer.CreateFromByteArray(buf);
		}
	}
}
#endif