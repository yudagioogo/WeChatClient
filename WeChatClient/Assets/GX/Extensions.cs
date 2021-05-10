using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Text.RegularExpressions;
#if UNITY
using UnityEngine;
#endif
using System.Xml;

namespace GX
{
	/// <remarks> ref: http://miaodadao.lofter.com/post/7a31e_f7032c </remarks>
	public static partial class Extensions
	{
		#region Random
		private static readonly System.Random random = new System.Random();

		/// <summary>
		/// 得到随机的bool值
		/// </summary>
		/// <param name="random"></param>
		/// <param name="trueRate">返回true的概率[0, 1]</param>
		/// <returns></returns>
		public static bool NextBoolean(this System.Random random, double trueRate)
		{
			return trueRate > random.NextDouble();
		}

		/// <summary>
		/// 产生给定范围的浮点随机数。
		/// <paramref name="minValue"/>和<paramref name="maxValue"/>两个参数可自动矫正大小
		/// </summary>
		/// <remarks> ref: http://stackoverflow.com/questions/5289613/generate-random-float-between-two-floats </remarks>
		/// <param name="random"></param>
		/// <param name="minValue"></param>
		/// <param name="maxValue"></param>
		/// <returns></returns>
		public static double NextDouble(this System.Random random, double minValue, double maxValue)
		{
			double min, max;
			if (minValue < maxValue)
			{
				min = minValue;
				max = maxValue;
			}
			else if (minValue > maxValue)
			{
				min = maxValue;
				max = minValue;
			}
			else
			{
				return minValue;
			}

			return min + random.NextDouble() * (max - min);
		}

		/// <summary>
		/// 按照<paramref name="probability"/>的概率加权，获取随机下标。
		/// 开闭规则为<c>[,) [,)... [,]</c>，正好在两个概率区间的临界值算作后面区间命中。
		/// 概率列表，无须归一化，只有大于0的概率才会被命中。当所有概率都不大于0时，将随机返回一个
		/// </summary>
		/// <param name="probability"></param>
		/// <returns></returns>
		public static int NextIndex(this System.Random random, IEnumerable<double> probability)
		{
			// 概率求和，不考虑负概率
			var sum = probability.Aggregate((s, d) => d > 0 ? s + d : s); // 只有大于0的概率才会被命中
			if (sum == 0)
				return random.Next(probability.Count()); // 当所有概率都不大于0时，将随机返回一个
			// 计算随机数
			var r = random.NextDouble(0, sum);
			// 根据随机数进行区间选取
			var i = 0;
			foreach (var p in probability)
			{
				if (p > 0)
				{
					if (r < p) // 不能用“<=”以控制开闭区间
						return i;
					r -= p;
				}
				i++;
			}
			return i;
		}

		/// <summary>
		/// 从序列中随机选择一个元素
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <returns>失败返回<c>default(T)</c></returns>
		public static T Random<T>(this IEnumerable<T> source)
		{
			if (source == null)
				return default(T);
			var list = (source as IList<T>) ?? source.ToList();
			if (list.Count == 0)
				return default(T);
			return list[random.Next(list.Count)];
		}
		/// <summary>
		/// Rearranges the elements in <paramref name="source"/> randomly.
		/// </summary>
		/// <param name="source"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		/// <remarks>
		/// ref:
		/// http://stackoverflow.com/questions/1287567/is-using-random-and-orderby-a-good-shuffle-algorithm
		/// http://stackoverflow.com/questions/48087/select-a-random-n-elements-from-listt-in-c-sharp
		/// http://www.cplusplus.com/reference/algorithm/random_shuffle/
		/// http://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
		/// </remarks>
		public static IEnumerable<T> RandomShuffle<T>(this IEnumerable<T> source)
		{
			if (source == null)
				yield break;
			var list = source.ToList(); // 必须创建一个拷贝，以避免原序列被意外修改
			for (int i = list.Count - 1; i >= 0; i--)
			{
				// Swap element "i" with a random earlier element it (or itself)
				// ... except we don't really need to swap it fully, as we can
				// return it immediately, and afterwards it's irrelevant.
				int swapIndex = random.Next(i + 1);
				yield return list[swapIndex];
				list[swapIndex] = list[i];
			}
		}
		#endregion

		#region Enumerable
		public static IEnumerable<object> AsEnumerable(this IEnumerator it)
		{
			if (it == null)
				yield break;
			while (it.MoveNext())
				yield return it.Current;
		}

		public static IEnumerable<T> AsEnumerable<T>(this IEnumerator<T> it)
		{
			if (it == null)
				yield break;
			while (it.MoveNext())
				yield return it.Current;
		}

		public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dic, IEnumerable<KeyValuePair<TKey, TValue>> collection)
		{
			foreach (var d in collection)
				dic.Add(d.Key, d.Value);
		}

		public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
		{
			if (collection == null || items == null)
				return;
			foreach (var i in items)
				collection.Add(i);
		}

		/// <summary>
		/// 对二维数组进行“平面化”的一维迭代访问
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <returns></returns>
		public static IEnumerable<IEnumerable<T>> AsEnumerable<T>(this T[,] data)
		{
			if (data == null)
				yield break;
			for (var i0 = data.GetLowerBound(0); i0 <= data.GetUpperBound(0); i0++)
			{
				yield return AsEnumerable(data, i0);
			}
		}

		private static IEnumerable<T> AsEnumerable<T>(this T[,] data, int i0)
		{
			for (var i1 = data.GetLowerBound(1); i1 <= data.GetUpperBound(1); i1++)
			{
				yield return data[i0, i1];
			}
		}

		public static bool AreEqual<T>(this IEnumerable<T> a, IEnumerable<T> b)
		{
			return Enumerable.SequenceEqual(a, b);
		}

		public static bool AreEqual<T>(this IEnumerable<IEnumerable<T>> a, IEnumerable<IEnumerable<T>> b)
		{
			return Enumerable.SequenceEqual(a, b,
			new LambdaEqualityComparer<IEnumerable<T>>(AreEqual));
		}

		public static bool AreEqual(this IEnumerable<KeyValuePair<string, IEnumerable<IEnumerable<string>>>> a, IEnumerable<KeyValuePair<string, IEnumerable<IEnumerable<string>>>> b)
		{
			return Enumerable.SequenceEqual(a, b,
			new LambdaEqualityComparer<KeyValuePair<string, IEnumerable<IEnumerable<string>>>>(
				(sa, sb) => sa.Key == sb.Key && AreEqual(sa.Value, sb.Value)));
		}

		/// <summary>
		/// 从给定容器确保拿出给定数量的元素，不足按照<paramref name="valueFactory"/>给定的方式补齐
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="count"></param>
		/// <param name="valueFactory">为null将采用<c>default(T)</c>生成默认元素</param>
		/// <returns></returns>
		public static IEnumerable<T> TakeWhile<T>(this IEnumerable<T> data, int count, Func<T> valueFactory = null)
		{

			int n = 0;
			foreach (var d in data.Take(count))
			{
				n++;
				yield return d;
			}
			for (; n < count; n++)
				yield return valueFactory == null ? default(T) : valueFactory();
		}

		/// <summary>
		/// 将容器大小调整至<paramref name="count"/>
		/// 容器过大则丢弃后面的元素，过小则根据<paramref name="valueFactory"/>提供的规则补齐
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="count"></param>
		/// <param name="valueFactory">容器大小不足扩容时的值填充，默认为<c>default(T)</c></param>
		/// <returns></returns>
		public static List<T> Resize<T>(this List<T> data, int count, Func<T> valueFactory = null)
		{
			while (data.Count < count)
				data.Add(valueFactory != null ? valueFactory() : default(T));
			if (data.Count > count)
				data.RemoveRange(count, data.Count - count);
			return data;
		}

		/// <summary>Merges two sequences by using the specified predicate function.</summary>
		/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains merged elements of two input sequences.</returns>
		/// <param name="first">The first sequence to merge.</param>
		/// <param name="second">The second sequence to merge.</param>
		/// <param name="resultSelector">A function that specifies how to merge the elements from the two sequences.</param>
		/// <typeparam name="TFirst">The type of the elements of the first input sequence.</typeparam>
		/// <typeparam name="TSecond">The type of the elements of the second input sequence.</typeparam>
		/// <typeparam name="TResult">The type of the elements of the result sequence.</typeparam>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="first" /> or <paramref name="second" /> is null.</exception>
		public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
		{
			return ZipIterator(first, second, resultSelector);
		}

		/// <summary>Merges two sequences by using the specified predicate function.</summary>
		/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains merged elements of two input sequences.</returns>
		/// <param name="first">The first sequence to merge.</param>
		/// <param name="second">The second sequence to merge.</param>
		/// <typeparam name="TFirst">The type of the elements of the first input sequence.</typeparam>
		/// <typeparam name="TSecond">The type of the elements of the second input sequence.</typeparam>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="first" /> or <paramref name="second" /> is null.</exception>
		public static IEnumerable<Tuple<TFirst, TSecond>> Zip<TFirst, TSecond>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second)
		{
			return ZipIterator(first, second, (f, s) => Tuple.Create(f, s));
		}

		private static IEnumerable<TResult> ZipIterator<TFirst, TSecond, TResult>(IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
		{
			using (IEnumerator<TFirst> f = first.GetEnumerator())
			using (IEnumerator<TSecond> s = second.GetEnumerator())
			{
				while (f.MoveNext() && s.MoveNext())
				{
					yield return resultSelector(f.Current, s.Current);
				}
			}
		}

		public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> first, TSource tail)
		{
			foreach (var d in first)
				yield return d;
			yield return tail;
		}

		/// <summary>
		/// 只在有必要时将<paramref name="source"/>转换为<c>IList&lt;T&gt;</c>，可减少不必要的<c>ToList</c>调用
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <returns></returns>
		public static IList<T> AsList<T>(this IEnumerable<T> source)
		{
			return (source as IList<T>) ?? source.ToList();
		}

		/// <summary>
		/// 只在有必要时将<paramref name="source"/>转换为<c>T[]</c>，可减少不必要的<c>ToList</c>调用
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <returns></returns>
		public static T[] AsArray<T>(this IEnumerable<T> source)
		{
			return (source as T[]) ?? source.ToArray();
		}

		/// <summary>
		/// 稳定的插入排序算法
		/// ref: http://www.csharp411.com/c-stable-sort/
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="comparison"></param>
		public static void InsertionSort<T>(this IList<T> list, Comparison<T> comparison)
		{
			if (list == null)
				return;
			if (comparison == null)
				throw new ArgumentNullException("comparison");

			int count = list.Count;
			for (int j = 1; j < count; j++)
			{
				T key = list[j];

				int i = j - 1;
				for (; i >= 0 && comparison(list[i], key) > 0; i--)
				{
					list[i + 1] = list[i];
				}
				list[i + 1] = key;
			}
		}
		/// <summary>
		/// 稳定的插入排序算法
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		public static void InsertionSort<T>(this IList<T> list) where T : IComparable<T>
		{
			var compare = new Comparison<T>((a, b) => a == null ? (b == null ? 0 : -1) : a.CompareTo(b));
			InsertionSort(list, compare);
		}

#if UNITY_METRO && !UNITY_EDITOR
	public static void ForEach<T>(this List<T> list, Action<T> action)
	{
		foreach (var i in list)
			action(i);
	}
#endif
		#endregion

		#region 数值类型的序列化
		public static byte[] ToBytes(this bool value) { return BitConverter.GetBytes(value); }
		public static byte[] ToBytes(this float value) { return BitConverter.GetBytes(value); }
		public static byte[] ToBytes(this double value) { return BitConverter.GetBytes(value); }
		public static byte[] ToBytes(this sbyte value) { return new byte[] { (byte)value }; }
		public static byte[] ToBytes(this byte value) { return new byte[] { value }; }
		public static byte[] ToBytes(this short value) { return BitConverter.GetBytes(value); }
		public static byte[] ToBytes(this ushort value) { return BitConverter.GetBytes(value); }
		public static byte[] ToBytes(this int value) { return BitConverter.GetBytes(value); }
		public static byte[] ToBytes(this uint value) { return BitConverter.GetBytes(value); }
		public static byte[] ToBytes(this long value) { return BitConverter.GetBytes(value); }
		public static byte[] ToBytes(this ulong value) { return BitConverter.GetBytes(value); }

		public static void Write(this Stream s, bool value) { s.Write(value.ToBytes()); }
		public static void Write(this Stream s, float value) { s.Write(value.ToBytes()); }
		public static void Write(this Stream s, double value) { s.Write(value.ToBytes()); }
		public static void Write(this Stream s, sbyte value) { s.WriteByte((byte)value); }
		public static void Write(this Stream s, byte value) { s.WriteByte(value); }
		public static void Write(this Stream s, short value) { s.Write(value.ToBytes()); }
		public static void Write(this Stream s, ushort value) { s.Write(value.ToBytes()); }
		public static void Write(this Stream s, int value) { s.Write(value.ToBytes()); }
		public static void Write(this Stream s, uint value) { s.Write(value.ToBytes()); }
		public static void Write(this Stream s, long value) { s.Write(value.ToBytes()); }
		public static void Write(this Stream s, ulong value) { s.Write(value.ToBytes()); }
		public static void Write(this Stream s, byte[] buffer) { s.Write(buffer, 0, buffer.Length); }

		public static void Read(this Stream s, out bool result) { result = BitConverter.ToBoolean(s.ReadBytes(sizeof(bool)), 0); }
		public static void Read(this Stream s, out float result) { result = BitConverter.ToSingle(s.ReadBytes(sizeof(float)), 0); }
		public static void Read(this Stream s, out double result) { result = BitConverter.ToDouble(s.ReadBytes(sizeof(double)), 0); }
		public static void Read(this Stream s, out sbyte result) { result = (sbyte)s.ReadByte(); }
		public static void Read(this Stream s, out byte result) { result = (byte)s.ReadByte(); }
		public static void Read(this Stream s, out short result) { result = BitConverter.ToInt16(s.ReadBytes(sizeof(short)), 0); }
		public static void Read(this Stream s, out ushort result) { result = BitConverter.ToUInt16(s.ReadBytes(sizeof(ushort)), 0); }
		public static void Read(this Stream s, out int result) { result = BitConverter.ToInt32(s.ReadBytes(sizeof(int)), 0); }
		public static void Read(this Stream s, out uint result) { result = BitConverter.ToUInt32(s.ReadBytes(sizeof(uint)), 0); }
		public static void Read(this Stream s, out long result) { result = BitConverter.ToInt64(s.ReadBytes(sizeof(long)), 0); }
		public static void Read(this Stream s, out ulong result) { result = BitConverter.ToUInt64(s.ReadBytes(sizeof(ulong)), 0); }
		public static void Read(this Stream s, byte[] buffer) { s.Read(buffer, 0, buffer.Length); }
		#endregion

		#region Stream

		public static int Available(this System.IO.Stream s)
		{
			return (int)(s.Length - s.Position);
		}

		public static IEnumerable<string> ReadAllLines(this TextReader reader)
		{
			while (true)
			{
				var line = reader.ReadLine();
				if (line == null)
					yield break;
				yield return line;
			}
		}

		public static IEnumerable<string> ReadAllLines(this string str, bool containsTerminating = false)
		{
			if (str == null)
				yield break;
			using (var reader = new StringReader(str))
			{
				foreach (var lien in ReadAllLines(reader))
					yield return lien;
			}
			if (containsTerminating)
			{
				if (str.Length == 0 || str.EndsWith("\n"))
					yield return "";
			}
		}

		/// <summary>
		/// 从当前位置开始读取若干字节
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static byte[] ReadBytes(this Stream stream, int count)
		{
			var buffer = new byte[count];
			int offset = 0;
			do
			{
				int n = stream.Read(buffer, offset, count - offset);
				if (n == 0)
					throw new System.IO.EndOfStreamException();
				offset += n;
			}
			while (offset < count);
			return buffer;
		}

		/// <summary>
		/// 从当前位置开始读取所有字节
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static byte[] ReadAllBytes(this Stream stream)
		{
			var mem = stream as MemoryStream;
			if (mem != null && mem.Position == 0)
				return mem.ToArray();

			if (stream.CanSeek)
				return ReadBytes(stream, (int)(stream.Length - stream.Position));

			using (var cache = new MemoryStream())
			{
				byte[] buffer = new byte[4096];
				int n;
				while ((n = stream.Read(buffer, 0, buffer.Length)) != 0)
				{
					cache.Write(buffer, 0, n);
				}
				return cache.ToArray();
			}
		}

		/// <summary>
		/// Reads the bytes from the current stream and writes them to another stream.
		/// ref: https://msdn.microsoft.com/en-us/library/dd782932
		/// </summary>
		/// <param name="src"></param>
		/// <param name="dst">The stream to which the contents of the current stream will be copied.</param>
		/// <remarks>Copying begins at the current position in the current stream, and does not reset the position of the destination stream after the copy operation is complete.</remarks>
		public static void CopyTo(this Stream src, Stream dst)
		{
			byte[] buffer = new byte[4096];
			int read;
			while ((read = src.Read(buffer, 0, buffer.Length)) > 0)
				dst.Write(buffer, 0, read);
		}
		#endregion

		#region Diagnostics
		private static string GetKnownDumpItemValue(object obj)
		{
			if (obj == null)
				return "<null>";
			else if (obj is string)
				return "\"" + obj + "\"";
			else if (obj.GetType().IsPrimitive()) // Boolean、Byte、SByte、Int16、UInt16、Int32、UInt32、Int64、UInt64、IntPtr、UIntPtr、Char、Double 和 Single
				return obj.ToString();
			else if (obj.GetType().IsEnum())
				return obj.ToString();
#if UNITY
			else if (obj is Vector2)
			{
				var data = (Vector2)obj;
				return string.Format("({0}, {1})", data.x, data.y);
			}
			else if (obj is Vector3)
			{
				var data = (Vector3)obj;
				return string.Format("({0}, {1}, {2})", data.x, data.y, data.z);
			}
			else if (obj is Vector4)
			{
				var data = (Vector4)obj;
				return string.Format("({0}, {1}, {2}, {3})", data.x, data.y, data.z, data.w);
			}
			else if (obj is Resolution)
			{
				var data = (Resolution)obj;
				return string.Format("{0} x {1}, {2}fps", data.width, data.height, data.refreshRate);
			}
			else if (obj is Rect)
			{
				var data = (Rect)obj;
				return string.Format("x:[{0}, {1}], y:[{2}, {3}], width:{4}, height:{5}",
					data.xMin, data.xMax, data.yMin, data.yMax, data.width, data.height);
			}
#endif
			else if (obj is DateTime)
			{
				var data = (DateTime)obj;
				return data.ToString();
			}
			else if (obj is byte[])
			{
				var data = (byte[])obj;
				return BitConverter.ToString(data);
			}
			else if (obj is IList<byte>)
			{
				var data = obj as IList<byte>;
				return BitConverter.ToString(data.ToArray());
			}
			else if (obj is IList)
			{
				var data = (IList)obj;
				return "{" + string.Join(", ", (from object i in data select Dump(i)).ToArray()) + "}";
			}
#if GX_PROTOBUF
		else if (obj is ProtoBuf.IExtensible)
		{
			var data = (ProtoBuf.IExtensible)obj;
			var sb = new StringBuilder();
			using (var writer = new StringWriter(sb))
			{
				WriteTo(data, writer);
			}
			return sb.ToString();
		}
#endif
			return null;
		}

		private static string GetDumpItemValue(object obj, System.Reflection.FieldInfo info)
		{
			try { var data = info.GetValue(obj); return GetKnownDumpItemValue(data) ?? Dump(data); }
			catch (Exception ex) { return string.Format("[{0}] {1}", ex.GetType().FullName, ex.Message); }
		}

		private static string GetDumpItemValue(object obj, System.Reflection.PropertyInfo info)
		{
			try { var data = info.GetValue(obj, null); return GetKnownDumpItemValue(data) ?? Dump(data); }
			catch (Exception ex) { return string.Format("[{0}] {1}", ex.GetType().FullName, ex.Message); }
		}

		/// <summary>
		/// 得到所有公有字段和属性的值，用于调试。
		/// 可用<see cref="NoDumpAttribute"/>来标记需要忽略的字段或属性
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static string Dump(this object obj)
		{
			var str = GetKnownDumpItemValue(obj);
			if (str != null)
				return str;
			var kv = (
				from f in obj.GetType().GetRuntimeFields()
				where f.GetCustomAttribute<NoDumpAttribute>() == null
				select f.Name + " = " + GetDumpItemValue(obj, f))
				.Union(
				from p in obj.GetType().GetRuntimeProperties()
				where p.GetCustomAttribute<NoDumpAttribute>() == null
				select p.Name + " : " + GetDumpItemValue(obj, p))
				.ToArray();
			return obj.GetType().FullName + " { " + string.Join(", ", kv) + " }";
		}

#if GX_PROTOBUF
	private static void WriteTo(ProtoBuf.IExtensible proto, TextWriter writer, int indent = 0)
	{
		var prefix = new string(' ', 4 * indent);
		var tab = "    ";
		if (proto == null)
		{
			writer.Write("<null>");
			return;
		}
		writer.Write(proto.GetType().FullName); writer.WriteLine(" {");
		foreach (var p in proto.GetType().GetRuntimeProperties())
		{
			if (p.GetCustomAttributes(typeof(ProtoBuf.ProtoMemberAttribute), false).Any() == false)
				continue;
			writer.Write(prefix); writer.Write(tab); writer.Write(p.Name); writer.Write(" = ");
			var value = p.GetValue(proto, null);
			if (value is ProtoBuf.IExtensible)
			{
				WriteTo((ProtoBuf.IExtensible)value, writer, indent + 1);
			}
			else if (value is string)
			{
				writer.Write("\"{0}\"", value);
			}
			else if (value is byte[])
			{
				var data = value as byte[];
				writer.Write("#{0} {1}", data.Length, BitConverter.ToString(data));
			}
			else if(value == null)
			{
				writer.Write("null");
			}
			else if (value is IList)
			{
				writer.Write('['); writer.WriteLine();
				foreach (var line in (value as IList))
				{
					writer.Write(prefix); writer.Write(tab); writer.Write(tab);
					if (line is ProtoBuf.IExtensible)
					{
						WriteTo(line as ProtoBuf.IExtensible, writer, indent + 2);
					}
					else if (line is string)
					{
						writer.Write("\"{0}\"", line);
					}
					else if (line is byte[])
					{
						var data = line as byte[];
						writer.Write("#{0} {1}", data.Length, BitConverter.ToString(data));
					}
					else if (line == null)
					{
						writer.Write("null");
					}
					else
					{
						writer.Write(line);
					}
					writer.WriteLine();
				}
				writer.Write(prefix); writer.Write(tab); writer.Write(']');
			}
			else
			{
				writer.Write(value);
			}
			writer.WriteLine();
		}
		writer.Write(prefix); writer.Write("}");
	}
#endif
		#endregion

		#region XML
		/// <summary>
		/// 获取指定名称的xml属性值
		/// </summary>
		/// <param name="e"></param>
		/// <param name="name"></param>
		/// <returns>失败返回null</returns>
		public static string AttributeValue(this XElement e, XName name)
		{
			if (e != null)
			{
				var a = e.Attribute(name);
				if (a != null)
					return a.Value;
			}
			return null;
		}
		/// <summary>
		/// 只在有必要的情况下设置属性的值
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="e"></param>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public static void AttributeValue<T>(this XElement e, XName name, T value)
		{
			if (value == null || value.Equals(default(T)))
				e.SetAttributeValue(name, null);
			else
				e.SetAttributeValue(name, value);
		}

		public static void Save(this XDocument doc, string filename, XmlWriterSettings format)
		{
			using (var w = XmlWriter.Create(filename, format))
			{
				doc.Save(w);
			}
		}
		#endregion

		#region TryParse & Parse
		public static bool TryParse(this string value, out bool result)
		{
			return bool.TryParse(value, out result);
		}
		public static bool Parse(this string value, bool defaultValue)
		{
			bool ret;
			return bool.TryParse(value, out ret) ? ret : defaultValue;
		}

		public static bool TryParse(this string value, out byte result)
		{
			return byte.TryParse(value, out result);
		}
		public static byte Parse(this string value, byte defaultValue)
		{
			byte ret;
			return byte.TryParse(value, out ret) ? ret : defaultValue;
		}

		public static bool TryParse(this string value, out short result)
		{
			return short.TryParse(value, out result);
		}
		public static short Parse(this string value, short defaultValue)
		{
			short ret;
			return short.TryParse(value, out ret) ? ret : defaultValue;
		}

		public static bool TryParse(this string value, out ushort result)
		{
			return ushort.TryParse(value, out result);
		}
		public static ushort Parse(this string value, ushort defaultValue)
		{
			ushort ret;
			return ushort.TryParse(value, out ret) ? ret : defaultValue;
		}

		public static bool TryParse(this string value, out int result)
		{
			return int.TryParse(value, out result);
		}
		public static int Parse(this string value, int defaultValue)
		{
			int ret;
			return int.TryParse(value, out ret) ? ret : defaultValue;
		}

		public static bool TryParse(this string value, out uint result)
		{
			return uint.TryParse(value, out result);
		}
		public static uint Parse(this string value, uint defaultValue)
		{
			uint ret;
			return uint.TryParse(value, out ret) ? ret : defaultValue;
		}

		public static bool TryParse(this string value, out long result)
		{
			return long.TryParse(value, out result);
		}
		public static long Parse(this string value, long defaultValue)
		{
			long ret;
			return long.TryParse(value, out ret) ? ret : defaultValue;
		}

		public static bool TryParse(this string value, out ulong result)
		{
			return ulong.TryParse(value, out result);
		}
		public static ulong Parse(this string value, ulong defaultValue)
		{
			ulong ret;
			return ulong.TryParse(value, out ret) ? ret : defaultValue;
		}

		public static bool TryParse(this string value, out float result)
		{
			return float.TryParse(value, out result);
		}
		public static float Parse(this string value, float defaultValue)
		{
			float ret;
			return float.TryParse(value, out ret) ? ret : defaultValue;
		}

#if UNITY
		/// <summary>
		/// 颜色值解析
		/// </summary>
		/// <param name="value">支持的格式：#RGB, #RRGGBB, #AARRGGBB, black, blue, clear, cyan, gray, green, magenta, red, white, yellow</param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryParse(this string value, out Color result)
		{
			if (string.IsNullOrEmpty(value))
			{
				result = Color.white;
				return false;
			}
			if (value.StartsWith("#"))
				return Extensions.TryParseColorFromARGB(value.Substring(1), out result);
			else
				return Extensions.TryParseColorFromName(value, out result);
		}
		/// <summary>
		/// 颜色值解析
		/// </summary>
		/// <param name="value">支持的格式：#RGB, #RRGGBB, #AARRGGBB, ColorName</param>
		public static Color Parse(this string value, Color defaultValue)
		{
			Color ret;
			return TryParse(value, out ret) ? ret : defaultValue;
		}
#endif
		#endregion

		#region Text & String
		/// <summary>
		/// ref: http://stackoverflow.com/questions/5377566/get-raw-pixel-value-in-bitmap-image
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string ToBitString(this byte value)
		{
			var sb = new StringBuilder(8);
			for (var i = 7; i >= 0; i--)
				sb.Append((value & (1 << i)) > 0 ? '1' : '0');
			return sb.ToString();
		}
		public static string ToBitString(this short value)
		{
			return string.Join(" ", BitConverter.GetBytes(value).Select(b => b.ToBitString()).ToArray());
		}
		public static string ToBitString(this ushort value)
		{
			return string.Join(" ", BitConverter.GetBytes(value).Select(b => b.ToBitString()).ToArray());
		}
		public static string ToBitString(this int value)
		{
			return string.Join(" ", BitConverter.GetBytes(value).Select(b => b.ToBitString()).ToArray());
		}
		public static string ToBitString(this uint value)
		{
			return string.Join(" ", BitConverter.GetBytes(value).Select(b => b.ToBitString()).ToArray());
		}
		public static string ToBitString(this long value)
		{
			return string.Join(" ", BitConverter.GetBytes(value).Select(b => b.ToBitString()).ToArray());
		}
		public static string ToBitString(this ulong value)
		{
			return string.Join(" ", BitConverter.GetBytes(value).Select(b => b.ToBitString()).ToArray());
		}

		/// <summary>
		/// 将字符串由正则表达式分割成片段，并标注每段是否匹配
		/// </summary>
		/// <param name="regex"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public static IEnumerable<Tuple<bool, string>> Fragment(this Regex regex, string input)
		{
			if (input == null)
				yield break;

			int i = 0;
			for (var m = regex.Match(input); m.Success; i = m.Index + m.Length, m = m.NextMatch())
			{
				int len = m.Index - i;
				if (len > 0)
					yield return Tuple.Create(false, input.Substring(i, len));
				yield return Tuple.Create(true, m.Value);
			}
			if (i < input.Length)
				yield return Tuple.Create(false, input.Substring(i));
		}
		#endregion

		#region Unity
#if UNITY
		/// <summary>
		/// Gets or add a component. Usage example:
		/// BoxCollider boxCollider = transform.GetOrAddComponent&lt;BoxCollider&gt;();
		/// </summary>
		/// <example><code>
		/// BoxCollider boxCollider = transform.GetOrAddComponent&lt;BoxCollider&gt;();
		/// </code></example>
		/// <remarks> ref: http://wiki.unity3d.com/index.php?title=Singleton </remarks>
		public static T GetOrAddComponent<T>(this Component child) where T : Component
		{
			T result = child.GetComponent<T>();
			if (result == null)
			{
				result = child.gameObject.AddComponent<T>();
			}
			return result;
		}

		class ActionToEnumerator : IEnumerable
		{
			private Action action;
			public ActionToEnumerator(Action action) { this.action = action; }

			#region IEnumerable 成员

			public IEnumerator GetEnumerator()
			{
				if (action == null)
					yield break;
				action();
			}

			#endregion
		}

		public static Coroutine StartCoroutine(this MonoBehaviour mb, Action action)
		{
			return mb.StartCoroutine(new ActionToEnumerator(action).GetEnumerator());
		}

		/// <summary>
		/// 递归得到所有指定类型的<see cref="Component"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="go"></param>
		/// <returns></returns>
		public static IEnumerable<T> GetComponentsDescendant<T>(this GameObject go) where T : Component
		{
			if (go == null)
				return Enumerable.Empty<T>();
			return GetComponentsDescendant<T>(go.transform);
		}

		/// <summary>
		/// 递归得到所有指定类型的<see cref="Component"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="c"></param>
		/// <returns></returns>
		public static IEnumerable<T> GetComponentsDescendant<T>(this Component c) where T : Component
		{
			if (c == null)
				return Enumerable.Empty<T>();
			return GetComponentsDescendant<T>(c.transform);
		}

		/// <summary>
		/// 递归得到所有指定类型的<see cref="Component"/>
		/// ref: http://answers.unity3d.com/questions/555101/possible-to-make-gameobjectgetcomponentinchildren.html
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="transform"></param>
		/// <returns></returns>
		public static IEnumerable<T> GetComponentsDescendant<T>(this Transform transform) where T : Component
		{
			if (transform == null)
				yield break;
			foreach (var c in transform.GetComponents<T>())
				yield return c;

			for (int i = 0; i < transform.childCount; i++)
			{
				foreach (var c in GetComponentsDescendant<T>(transform.GetChild(i)))
					yield return c;
			}
		}

		/// <summary>
		/// 得到给定节点的所有子节点列表
		/// </summary>
		/// <param name="transform"></param>
		/// <returns></returns>
		public static IEnumerable<Transform> GetAllChildren(this Transform transform)
		{
			if (transform == null)
				yield break;
			for (var i = 0; i < transform.childCount; i++)
				yield return transform.GetChild(i);
		}

		/// <summary>
		/// 销毁当前节点的所有子节点
		/// </summary>
		/// <param name="transform"></param>
		public static void DestroyAllChildren(this Transform transform)
		{
			if (transform == null)
				return;
			foreach (Transform t in transform)
				GameObject.Destroy(t.gameObject);
		}

		/// <summary>
		/// 立即删除所有子节点
		/// </summary>
		/// <param name="transform"></param>
		public static void DestroyAllChildrenImmediate(this Transform transform)
		{
			if (transform == null)
				return;
			foreach (var t in transform.Cast<Transform>().ToList())
				GameObject.DestroyImmediate(t.gameObject);
		}

		/// <summary>
		/// 得到节点全路径
		/// </summary>
		/// <remarks>
		/// ref: http://answers.unity3d.com/questions/8500/how-can-i-get-the-full-path-to-a-gameobject.html
		/// </remarks>
		/// <param name="current"></param>
		/// <returns></returns>
		public static string GetPath(this Transform current)
		{
			if (current.parent == null)
				return "/" + current.name;
			return GetPath(current.parent) + "/" + current.name;
		}
		/// <summary>
		/// 得到节点全路径
		/// </summary>
		/// <param name="component"></param>
		/// <returns></returns>
		public static string GetPath(this Component component)
		{
			return GetPath(component.transform) + ":" + component.GetType().ToString();
		}

		/// <summary>
		/// 重新设置父节点，且不改变当前transform的local属性
		/// </summary>
		/// <param name="current"></param>
		/// <param name="newParent"></param>
		public static void SetParentOnly(this Transform current, Transform newParent)
		{
			var oldPosition = current.localPosition;
			var oldScale = current.localScale;
			var oldRotation = current.localRotation;
			var oldEulerAngles = current.localEulerAngles;

			current.SetParent(newParent);

			current.localPosition = oldPosition;
			current.localScale = oldScale;
			current.localRotation = oldRotation;
			current.localEulerAngles = oldEulerAngles;
		}

		/// <summary>
		/// 递归找出指定名称的子节点
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="childname">要获取的子节点名称</param>
		public static Transform GetComponentsInChildren(this Transform parent, string childname)
		{
			if (parent == null)
				return null;
			return parent.GetComponentsInChildren<Transform>()
				.FirstOrDefault(t => t.name == childname);
		}
#endif
		#endregion

		#region Color
#if UNITY
		/// <summary>
		/// 颜色值解析
		/// ref: http://www.dreamdu.com/css/css_colors/
		/// </summary>
		/// <param name="argb">支持的格式：RGB, RRGGBB, AARRGGBB</param>
		/// <param name="color"></param>
		/// <returns></returns>
		private static bool TryParseColorFromARGB(string argb, out Color color)
		{
			color = Color.white;
			if (string.IsNullOrEmpty(argb))
				return false;
			switch (argb.Length)
			{
				case 3:
					argb = new string(new char[] { 'F', 'F', argb[0], argb[0], argb[1], argb[1], argb[2], argb[2] });
					goto case 8;
				case 6:
					argb = "FF" + argb;
					goto case 8;
				case 8:
					uint result;
					if (uint.TryParse(argb, System.Globalization.NumberStyles.AllowHexSpecifier, null, out result))
					{
						float inv = 1f / 255f;
						color.a = inv * ((result >> 24) & 0xFF);
						color.r = inv * ((result >> 16) & 0xFF);
						color.g = inv * ((result >> 8) & 0xFF);
						color.b = inv * ((result >> 0) & 0xFF);
						return true;
					}
					break;
				default:
					break;
			}
			return false;
		}

		/// <summary>
		/// 颜色值解析
		/// </summary>
		/// <param name="colorName">支持的颜色名：black, blue, clear, cyan, gray, green, magenta, red, white, yellow</param>
		/// <param name="color"></param>
		/// <returns></returns>
		private static bool TryParseColorFromName(string colorName, out Color color)
		{
			switch (colorName)
			{
				case "black": color = Color.black; return true;
				case "blue": color = Color.blue; return true;
				case "clear": color = Color.clear; return true;
				case "cyan": color = Color.cyan; return true;
				case "gray": color = Color.gray; return true;
				case "green": color = Color.green; return true;
				case "grey": color = Color.grey; return true;
				case "magenta": color = Color.magenta; return true;
				case "red": color = Color.red; return true;
				case "white": color = Color.white; return true;
				case "yellow": color = Color.yellow; return true;
				default: color = Color.white; return false;
			}
		}
#endif
		#endregion

		#region NGUI
#if GX_NGUI
		/// <summary>
		/// 得到鼠标点击/悬浮处的URL内容
		/// </summary>
		/// <returns>失败返回<c>null</c></returns>
		public static string GetUrlTouch(this UILabel label)
		{
			return label != null ? label.GetUrlAtPosition(UICamera.lastHit.point) : null;
		}

		/// <summary>
		/// 测量给定<see cref="UILabel"/>的宽度能容纳的字符串长度
		/// </summary>
		/// <param name="label"></param>
		/// <param name="text">要测量的字符串，为null则采用<c>label.text</c></param>
		/// <param name="startIndex">起始下标</param>
		/// <returns>满足<paramref name="label"/>一行宽度的字符串末尾下标，其他则返回<paramref name="startIndex"/></returns>
		/// <remarks>该函数不会改变<paramref name="label"/>的状态，但会污染<see cref="NGUIText"/>的状态</remarks>
		public static int WrapLine(this UILabel label, string text = null, int startIndex = 0)
		{
			if (label == null)
				return startIndex;
			if (text == null)
				text = label.text;
			if (startIndex < 0 || startIndex >= text.Length)
				return startIndex;

			label.UpdateNGUIText(); // 更新 NGUIText 的状态
			NGUIText.Prepare(text); // 准备字体以备测量

			var cur_extent = 0f;
			var prev = 0;
			for (var c = startIndex; c < text.Length; ++c)
			{
				var ch = text[c];
				var w = NGUIText.GetGlyphWidth(ch, prev);
				if (w == 0f)
					continue;
				cur_extent += w + NGUIText.finalSpacingX;
				if (NGUIText.rectWidth < cur_extent)
					return c;
			}

			return text.Length;
		}

		/// <summary>
		/// 使<see cref="UIInput"/>具有输入焦点
		/// </summary>
		/// <param name="input"></param>
		public static void Focus(this UIInput input)
		{
			if (input == null)
				return;
			UICamera.selectedObject = input.gameObject;
		}

		/// <summary>
		/// 切换Active状态
		/// </summary>
		/// <param name="target"></param>
		public static void ToggleActive(this GameObject target)
		{
			if (target == null)
				return;
			target.SetActive(!target.activeSelf);
		}
		/// <summary>
		/// 切换Active状态
		/// </summary>
		/// <param name="target"></param>
		public static void ToggleActive(this MonoBehaviour target)
		{
			if (target == null)
				return;
			ToggleActive(target.gameObject);
		}

		/// <summary>
		/// Adjust the widgets' depth by the specified value.
		/// Returns '0' if nothing was adjusted, '1' if panels were adjusted, and '2' if widgets were adjusted.
		/// </summary>
		/// <remarks>copy from NGUI-3.5.3 Scripts\Internal\NGUITools.cs，这里用老版本的NGUI实现代码</remarks>
		private static int AdjustDepth(GameObject go, int adjustment)
		{
			if (go != null)
			{
				UIPanel panel = go.GetComponent<UIPanel>();

				if (panel != null)
				{
					UIPanel[] panels = go.GetComponentsInChildren<UIPanel>(true);

					for (int i = 0; i < panels.Length; ++i)
					{
						UIPanel p = panels[i];
#if UNITY_EDITOR
						NGUITools.RegisterUndo(p, "Depth Change");
#endif
						p.depth = p.depth + adjustment;
					}
					return 1;
				}
				else
				{
					UIWidget[] widgets = go.GetComponentsInChildren<UIWidget>(true);

					for (int i = 0, imax = widgets.Length; i < imax; ++i)
					{
						UIWidget w = widgets[i];
#if UNITY_EDITOR
						NGUITools.RegisterUndo(w, "Depth Change");
#endif
						w.depth = w.depth + adjustment;
					}
					return 2;
				}
			}
			return 0;
		}

		/// <summary>
		/// Bring all of the widgets on the specified object forward.
		/// 和新版本的<see cref="NGUITools.BringForward"/>实现不同
		/// </summary>
		/// <remarks>copy from NGUI-3.5.3 Scripts\Internal\NGUITools.cs</remarks>
		public static void BringForward(this GameObject ui)
		{
			int val = AdjustDepth(ui, 1000);
			if (val == 1) NGUITools.NormalizePanelDepths();
			else if (val == 2) NGUITools.NormalizeWidgetDepths();
		}

		/// <summary>
		///  动态加载时，设置界面父节点
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="anchorsTo"></param>
		public static void SetParent(this MonoBehaviour curObj, GameObject parent, GameObject anchorsTo = null)
		{
			curObj.gameObject.transform.SetParentOnly(parent.transform);
			if (anchorsTo != null)
			{
				var widget = curObj.gameObject.GetComponent<UIWidget>();
				if (widget != null)
				{
					widget.SetAnchor(anchorsTo.transform);
				}
			}
		}
#endif
		#endregion

		#region NGUI Tween
#if GX_NGUI
		public static TweenPosition TweenMoveTo(this GameObject obj, float duration, Vector3 from, Vector3 to, float delay = 0)
		{
			if (obj == null)
				return null;
			var tween = TweenPosition.Begin<TweenPosition>(obj, duration);
			tween.from = from;
			tween.to = to;
			tween.delay = delay;
			tween.ResetToBeginning();
			return tween;
		}

		public static TweenPosition TweenMoveTo(this GameObject obj, float duration, float xFrom, float yFrom, float xTo, float yTo, float delay = 0)
		{
			return TweenMoveTo(obj, duration, new Vector3(xFrom, yFrom, 0), new Vector3(xTo, yTo, 0), delay);
		}

		public static TweenPosition TweenMoveBy(this GameObject obj, float duration, Vector3 delta, float delay = 0)
		{
			if (obj == null)
				return null;
			var cur = obj.transform.position;
			return TweenMoveTo(obj, duration, cur, cur + delta, delay);
		}

		public static TweenPosition TweenMoveBy(this GameObject obj, float duration, float deltaX, float deltaY, float delay = 0)
		{
			return TweenMoveBy(obj, duration, new Vector3(deltaX, deltaY, 0), delay);
		}
#endif
		#endregion

		#region Google Protocol Buffers
#if GX_PROTOBUF
		/// <summary>
		/// Create a deep clone of the supplied instance; any sub-items are also cloned.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="pb"></param>
		/// <returns></returns>
		public static T DeepClone<T>(this T pb) where T : ProtoBuf.IExtensible
		{
			if (pb == null)
				return default(T);
			return ProtoBuf.Serializer.DeepClone<T>(pb);
		}

		/// <summary>
		/// 得到给定类型中，所有具有ProtoMemberAttribute特性的属性
		/// </summary>
		/// <param name="protobuf"></param>
		/// <returns></returns>
		public static IEnumerable<System.Reflection.PropertyInfo> GetProtoMemberNames(System.Type protobuf)
		{
			return
				from p in GX.Reflection.GetRuntimeProperties(protobuf)
				let m = p.GetCustomAttributes(typeof(ProtoBuf.ProtoMemberAttribute), true) as ProtoBuf.ProtoMemberAttribute[]
				where m != null && m.Any()
				select p;
		}
#endif
		#endregion

		#region Convert DateTime & Unix GMT +8
		static readonly DateTime UnixBase = new DateTime(1970, 1, 1, 0, 0, 0);

		/// <summary>
		/// 将本地时区的DateTime时间转换成Unix时戳
		/// </summary>
		/// <param name="time">本地时间</param>
		/// <returns></returns>
		public static uint ToUnixTime(this DateTime time)
		{
			return (uint)(time - UnixBase).TotalSeconds;
		}

		/// <summary>
		/// 将本地时区的Unix时戳转换成DateTime类型
		/// </summary>
		/// <param name="localGMTTime"></param>
		/// <returns></returns>
		public static DateTime ToDateTime(this uint localGMTTime)
		{
			return UnixBase + TimeSpan.FromSeconds(localGMTTime);
		}
		#endregion
	}

	/// <summary>
	/// 标记被<see cref="Dump"/>方法忽略的字段或属性
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
	public sealed class NoDumpAttribute : Attribute
	{
	}
}
