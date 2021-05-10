using System.Collections;
using System;

namespace GX
{
	public class EventArgs<T> : EventArgs
	{
		public T Data { get; set; }
	}
}