using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GX
{
	public class LambdaEqualityComparer<T> : EqualityComparer<T>
	{
		private Func<T, T, bool> equals;
		private Func<T, int> getHashCode;

		public LambdaEqualityComparer(Func<T, T, bool> equals, Func<T, int> getHashCode = null)
		{
			this.equals = equals;
			this.getHashCode = getHashCode;
		}
		public override bool Equals(T x, T y) { return this.equals(x, y); }
		public override int GetHashCode(T obj) { return getHashCode == null ? obj.GetHashCode() : getHashCode(obj); }
	}

	/// <summary>
	/// <see cref="LambdaEqualityComparer<T>"/>的工厂
	/// </summary>
	public static class EqualityComparer
	{
		public static IEqualityComparer<T> Create<T>(Func<T, T, bool> equals, Func<T, int> getHashCode = null)
		{
			return new LambdaEqualityComparer<T>(equals, getHashCode);
		}

		public static IEqualityComparer<T> Create<T>(Func<T, T, int> compare, Func<T, int> getHashCode = null)
		{
			return new LambdaEqualityComparer<T>((x, y) => compare(x, y) == 0, getHashCode);
		}

		public static IEqualityComparer<T> Create<T>(IComparer<T> compare, Func<T, int> getHashCode = null)
		{
			return new LambdaEqualityComparer<T>((x, y) => compare.Compare(x, y) == 0, getHashCode);
		}
	}
}
