using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace GX
{
	/// <summary>
	/// Compares two Unicode strings. Digits in the strings are considered as numerical content rather than text.
	/// This test is not case-sensitive.
	/// ref: http://msdn.microsoft.com/en-us/library/bb759947
	/// </summary>
	public class NaturalComparer : IComparer<string>, IComparer
	{
		private static readonly Regex regex = new Regex(@"\d+(\.\d+)?");

		#region IComparer 成员

		int IComparer.Compare(object x, object y)
		{
			if (!(x is string))
				throw new ArgumentException("Parameter type is not string", "x");
			if (!(y is string))
				throw new ArgumentException("Parameter type is not string", "y");
			return Compare(x as string, y as string);
		}

		#endregion

		#region IComparer<string> 成员

		public virtual int Compare(string x, string y)
		{
			if (x == y)
				return 0;
			using (var ix = regex.Fragment(x).GetEnumerator())
			using (var iy = regex.Fragment(y).GetEnumerator())
			{
				while (true)
				{
					var fx = ix.MoveNext() ? ix.Current : null;
					var fy = iy.MoveNext() ? iy.Current : null;
					// 结束检测
					if (fx == null)
						return fy == null ? 0 : -1;
					if (fy == null)
						return 1;
					// 对应数字段的比较
					if (fx.Item1 && fy.Item1)
					{
						var c = double.Parse(fx.Item2).CompareTo(double.Parse(fy.Item2));
						if (c != 0)
							return c;
					}
					// 普通字符串片段比较
					{
						var c = fx.Item2.CompareTo(fy.Item2);
						if (c != 0)
							return c;
					}
				}
			}
		}

		#endregion
	}

	/// <summary>
	/// 按照文件路径的自然排序
	/// </summary>
	public class NaturalPathComparer : IComparer<string>, IComparer
	{
		private readonly NaturalComparer compare = new NaturalComparer();

		#region IComparer<string> 成员

		public int Compare(string x, string y)
		{
			// 路径比较
			var dirx = Path.GetDirectoryName(x).Split('\\', '/');
			var diry = Path.GetDirectoryName(y).Split('\\', '/');
			for (int i = 0; i < Math.Min(dirx.Length, diry.Length); i++)
			{
				var n = compare.Compare(dirx[i], diry[i]);
				if (n != 0)
					return n;
			}

			// 路径前面相同，按照路径长度比较
			var c = dirx.Length - diry.Length;
			if (c != 0)
				return c;

			// 比较主文件名
			c = compare.Compare(Path.GetFileNameWithoutExtension(x), Path.GetFileNameWithoutExtension(y));
			if (c != 0)
				return c;

			// 比较扩展名
			return compare.Compare(Path.GetExtension(x), Path.GetExtension(y));
		}

		#endregion

		#region IComparer 成员

		int IComparer.Compare(object x, object y)
		{
			if (!(x is string))
				throw new ArgumentException("Parameter type is not string", "x");
			if (!(y is string))
				throw new ArgumentException("Parameter type is not string", "y");
			return Compare(x as string, y as string);
		}

		#endregion
	}
}
