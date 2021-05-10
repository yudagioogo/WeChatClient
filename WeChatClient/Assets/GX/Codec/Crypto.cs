#if UNITY
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

#if UNITY_METRO && !UNITY_EDITOR
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using GX.WinRT;
#endif


namespace GX
{
	public class Crypto
	{
		public interface IProxy
		{
			void SetKey(byte[] key, byte[] iv);
			byte[] Encode(byte[] buf);
			byte[] Decode(byte[] buf);
		}

		public static IProxy Proxy { get; set; }

#if !UNITY_METRO || UNITY_EDITOR
		class AesProxy : IProxy
		{
			private readonly SymmetricAlgorithm algorithm = new System.Security.Cryptography.AesManaged() { KeySize = 128 };
			private SecretBytes IV;
			private SecretBytes KEY;

			#region IProxy 成员

			public void SetKey(byte[] key, byte[] iv)
			{
				KEY = new SecretBytes() { Bytes = key };
				IV = new SecretBytes() { Bytes = iv };
			}

			public byte[] Encode(byte[] buf)
			{
				using (var mem = new MemoryStream())
				using (var crypto = new CryptoStream(mem, algorithm.CreateEncryptor(KEY.Bytes, IV.Bytes), CryptoStreamMode.Write))
				{
					crypto.Write(buf, 0, buf.Length);
					crypto.FlushFinalBlock();
					crypto.Close();
					return mem.ToArray();
				}
			}

			public byte[] Decode(byte[] buf)
			{
				using (var mem = new MemoryStream())
				using (var crypto = new CryptoStream(mem, algorithm.CreateDecryptor(KEY.Bytes, IV.Bytes), CryptoStreamMode.Write))
				{
					crypto.Write(buf, 0, buf.Length);
					crypto.FlushFinalBlock();
					crypto.Close();
					return mem.ToArray();
				}
			}

			#endregion
		}
#else
		/// <summary>
		/// ref: http://canbilgin.wordpress.com/2012/10/03/simple-aes-symmetric-key-encryption-in-winrt/
		/// </summary>
		class AesProxy : IProxy
		{
			private GX.SecretBytes KEY;
			private GX.SecretBytes IV;

			#region IProxy 成员

			public void SetKey(byte[] key, byte[] iv)
			{
				this.KEY = new SecretBytes() { Bytes = key };
				this.IV = new SecretBytes() { Bytes = iv };
			}

			public byte[] Encode(byte[] buf)
			{
				var encoder = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesEcbPkcs7);
				var encoderKey = encoder.CreateSymmetricKey(KEY.Bytes.ToIBuffer());
				return CryptographicEngine.Encrypt(encoderKey, buf.ToIBuffer(), IV.Bytes.ToIBuffer()).ToBytes();
			}

			public byte[] Decode(byte[] buf)
			{
				var decoder = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesCbcPkcs7);
				var decoderKey = decoder.CreateSymmetricKey(KEY.Bytes.ToIBuffer());
				return CryptographicEngine.Decrypt(decoderKey, buf.ToIBuffer(), IV.Bytes.ToIBuffer()).ToBytes();
			}

			#endregion
		}
#endif

		static Crypto()
		{
			Proxy = new AesProxy();
			Proxy.SetKey(
				GX.MD5.ComputeHash(Encoding.GetBytes(SystemInfo.deviceUniqueIdentifier)),
				new byte[16]);
		}

		public static byte[] Encode(byte[] buf)
		{
			return Proxy.Encode(buf);
		}

		public static byte[] Decode(byte[] buf)
		{
			return Proxy.Decode(buf);
		}
	}
}
#endif
