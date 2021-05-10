using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
	public static class ArraySegment
	{
		public static IEnumerable<ArraySegment<T>> Create<T>(params ArraySegment<T>[] items)
		{
			return items;
		}

		public static IEnumerable<ArraySegment<T>> Create<T>(IEnumerable<T[]> items)
		{
			return items.Select(i => new ArraySegment<T>(i));
		}

		public static IEnumerable<ArraySegment<T>> Create<T>(params T[][] items)
		{
			return Create(items as IEnumerable<T[]>);
		}
	}
}
