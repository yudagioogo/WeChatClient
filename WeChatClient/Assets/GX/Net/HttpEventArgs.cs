#if UNITY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GX.Net
{
	public class HttpRequestEventArgs : EventArgs
	{
		public string Url { get; private set; }
		public string Data { get; private set; }
		public HttpRequestEventArgs(string url, string data)
		{
			this.Url = url;
			this.Data = data;
		}
	}

	public class HttpResponseEventArgs : EventArgs
	{
		public WWW WWW { get; private set; }
		public HttpResponseEventArgs(WWW www)
		{
			this.WWW = www;
		}
	}
}
#endif
