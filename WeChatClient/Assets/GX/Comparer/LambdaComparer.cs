using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GX
{
	public class LambdaComparer<T> : Comparer<T>
	{
		private Comparison<T> comparison;

		public LambdaComparer(Comparison<T> comparison)
		{
			this.comparison = comparison;
		}

		public override int Compare(T x, T y)
		{
			return comparison(x, y);
		}
	}
}
