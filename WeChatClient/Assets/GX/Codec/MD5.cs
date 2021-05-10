using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GX
{
	public class MD5
	{
#if !UNITY_WINRT || UNITY_EDITOR
		[ThreadStatic]
		private static System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
#endif
		public static string ComputeHashString(byte[] data) { return ToString(ComputeHash(data)); }
		public static string ComputeHashString(string data) { return ToString(ComputeHash(GX.Encoding.GetBytes(data))); }

		public static byte[] ComputeHash(byte[] data)
		{
#if UNITY_WINRT && !UNITY_EDITOR
			return UnityEngine.Windows.Crypto.ComputeMD5Hash(data);
#else
			if (md5 == null)
				md5 = System.Security.Cryptography.MD5.Create();
			return md5.ComputeHash(data);
#endif
		}

		public static string ComputeHashStringFromFile(string filename) { return ToString(ComputeHashFromFile(filename)); }
		public static byte[] ComputeHashFromFile(string filename)
		{
#if UNITY_WP8 && !UNITY_EDITOR
			byte[] buf;
			using (var file = File.OpenRead(filename))
			{
				buf = file.ReadAllBytes();
			}
			return ComputeHash(buf);
#elif UNITY_METRO && !UNITY_EDITOR
			return ComputeHash(UnityEngine.Windows.File.ReadAllBytes(filename));
#else
			return ComputeHash(File.ReadAllBytes(filename));
#endif
		}

		public static string ToString(byte[] data)
		{
			return BitConverter.ToString(data).Replace("-", string.Empty).ToLowerInvariant();
		}
	}
}
