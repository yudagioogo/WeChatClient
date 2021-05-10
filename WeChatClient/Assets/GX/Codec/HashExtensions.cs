#if UNITY_METRO && !UNITY_EDITOR
using HashAlgorithm = Windows.Security.Cryptography.Core.HashAlgorithmProvider;
using Windows.Storage.Streams;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using GX.WinRT;
#else
using System.Security.Cryptography;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GX
{
	public static class HashExtensions
	{
		public static string ToHashString(this byte[] buffer, HashAlgorithm algorithm)
		{
			if (buffer == null)
				return null;
			return ToString(algorithm.ComputeHash(buffer));
		}
		public static string ToHashString(this byte[] buffer, int offset, int count, HashAlgorithm algorithm)
		{
			if (buffer == null)
				return null;
			return ToString(algorithm.ComputeHash(buffer, offset, count));
		}
		public static string ToHashString(this Stream inputStream, HashAlgorithm algorithm)
		{
			if (inputStream == null)
				return null;
			return ToString(algorithm.ComputeHash(inputStream));
		}
		public static string ToHashString(this string str, HashAlgorithm algorithm)
		{
			if (str == null)
				return null;
			return ToString(algorithm.ComputeHash(Encoding.GetBytes(str)));
		}

		private static string ToString(byte[] buf)
		{
			return BitConverter.ToString(buf).Replace("-", string.Empty).ToLowerInvariant();
		}

#if UNITY_METRO && !UNITY_EDITOR
		private static string ToString(IBuffer buf)
		{
			return CryptographicBuffer.EncodeToHexString(buf).ToLowerInvariant();
		}

		private static IBuffer ComputeHash(this HashAlgorithmProvider algorithm, byte[] buffer)
		{
			return algorithm.HashData(buffer.ToIBuffer());
		}
		private static IBuffer ComputeHash(this HashAlgorithmProvider algorithm, byte[] buffer, int offset, int count)
		{
			return algorithm.HashData(buffer.ToIBuffer(offset, count));
		}
		private static IBuffer ComputeHash(this HashAlgorithmProvider algorithm, Stream inputStream)
		{
			return algorithm.HashData(inputStream.ToIBuffer());
		}
#endif
	}
}
