using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Collections.Generic
{
	public static class KeyValuePair
	{
		public static KeyValuePair<TKey, TValue> Create<TKey, TValue>()
		{
			return new KeyValuePair<TKey, TValue>();
		}

		public static KeyValuePair<TKey, TValue> Create<TKey, TValue>(TKey key, TValue value)
		{
			return new KeyValuePair<TKey, TValue>(key, value);
		}

		public static KeyValuePair<TKey, TValue> Create<TKey, TValue>(Tuple<TKey, TValue> tupel)
		{
			return new KeyValuePair<TKey, TValue>(tupel.Item1, tupel.Item2);
		}
	}
}
