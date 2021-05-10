using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GX
{
	/// <summary>
	/// 安全队列的简单实现
	/// ref: WebSocket4Net.dll, Version=0.8.0.0
	/// doc: https://msdn.microsoft.com/zh-cn/library/dd267265
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ConcurrentQueue<T>
	{
		private Queue<T> items;
		private object syncRoot = new object();

		public ConcurrentQueue()
		{
			this.items = new Queue<T>();
		}

		public ConcurrentQueue(int capacity)
		{
			this.items = new Queue<T>(capacity);
		}

		public ConcurrentQueue(IEnumerable<T> collection)
		{
			this.items = new Queue<T>(collection);
		}

		/// <summary>
		/// 将对象添加到队列的结尾处。
		/// </summary>
		/// <param name="item"></param>
		public void Enqueue(T item)
		{
			lock (this.syncRoot)
			{
				this.items.Enqueue(item);
			}
		}

		/// <summary>
		/// 尝试移除并返回位于并发队列开头处的对象。
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool TryDequeue(out T item)
		{
			bool result;
			lock (this.syncRoot)
			{
				if (this.items.Count <= 0)
				{
					item = default(T);
					result = false;
				}
				else
				{
					item = this.items.Dequeue();
					result = true;
				}
			}
			return result;
		}
	}
}
