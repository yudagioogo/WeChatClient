using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Security
{
	public static class SecureStringExtensions
	{
		public static SecureString SetValue(this SecureString ss, string str)
		{
			if (ss != null)
			{
				ss.Clear();
				if (str != null)
				{
					foreach (var c in str)
						ss.AppendChar(c);
				}
			}
			return ss;
		}

		public static string GetValue(this SecureString ss)
		{
			if (ss == null)
				return string.Empty;
			var p = IntPtr.Zero;
			try
			{
				p = Marshal.SecureStringToBSTR(ss);
				return Marshal.PtrToStringBSTR(p);
			}
			finally
			{
				if (p != IntPtr.Zero)
					Marshal.ZeroFreeBSTR(p);
			}
		}
	}
}
