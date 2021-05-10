using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Linq;

namespace GX
{
	/// <summary>
	/// 可指定最大长度的队列
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class CircularQueue<T> : IEnumerable<T>, ICollection, IEnumerable
	{
		private readonly Queue<T> queue = new Queue<T>();

		/// <summary>
		/// 容器的最大容量（可达到），默认为0
		/// </summary>
		public int Capacity { get; set; }

		public bool TryDequeue(out T item)
		{
			if (Count > 0)
			{
				item = Dequeue();
				return true;
			}
			else
			{
				item = default(T);
				return false;
			}
		}

		public T Dequeue()
		{
			return queue.Dequeue();
		}

		public void Enqueue(T item)
		{
			queue.Enqueue(item);
			while (queue.Count > Capacity)
				queue.Dequeue();
		}

		/// <summary>
		/// 从容器中移除所有对象。 
		/// </summary>
		public void Clear() { queue.Clear(); }
		/// <summary>
		/// 确定某元素是否在容器中。 
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Contains(T item) { return queue.Contains(item); }
		/// <summary>
		/// 返回位于容器开始处的对象但不将其移除。 
		/// </summary>
		/// <returns></returns>
		public T Peek() { return queue.Peek(); }
		public T[] ToArray() { return queue.ToArray(); }

		#region IEnumerable<T> 成员

		public IEnumerator<T> GetEnumerator() { return (queue as IEnumerable<T>).GetEnumerator(); }

		#endregion

		#region IEnumerable 成员

		IEnumerator IEnumerable.GetEnumerator() { return (queue as IEnumerable).GetEnumerator(); }

		#endregion

		#region ICollection 成员

		void ICollection.CopyTo(Array array, int index) { (queue as ICollection).CopyTo(array, index); }
		public void CopyTo(T[] array, int index) { queue.CopyTo(array, index); }
		public int Count { get { return queue.Count; } }
		public bool IsSynchronized { get { return (queue as ICollection).IsSynchronized; } }
		public object SyncRoot { get { return (queue as ICollection).SyncRoot; } }

		#endregion
	}
}