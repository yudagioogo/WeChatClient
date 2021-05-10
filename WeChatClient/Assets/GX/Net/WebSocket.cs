#if GX_PROTOBUF
using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System;

namespace GX.Net
{
	/// <summary>
	/// 跨平台的WebSocket实现
	/// </summary>
	/// <remarks>
	/// TODO: 若需要多个实例，可以考虑对<see cref="WebSocket.IProxy"/>实现工厂
	/// </remarks>
	public static partial class WebSocket
	{
		public enum State
		{
			None = -1,
			Connecting = 0,
			Open = 1,
			Closing = 2,
			Closed = 3,
		}

		#region Proxy
		/// <summary>
		/// 接口定义参考：http://www.w3.org/TR/2011/CR-websockets-20111208/#the-websocket-interface
		/// </summary>
		public interface IProxy
		{
			GX.Net.WebSocket.State State { get; }
			void Open(string url);
			void Close();
			// TODO: 应该采用统一的异常模型来做网络异常处理，此处返回bool为临时方案
			bool Send(byte[] data);
			byte[] Receive();
		}
		public static IProxy Proxy { get; set; }

		static WebSocket()
		{
#if !UNITY_WINRT || UNITY_EDITOR
			Proxy = new WebSocket4NetProxy();
#endif
		}
		#endregion

		static partial void OnSend(ProtoBuf.IExtensible msg);
		static partial void OnReceive(ProtoBuf.IExtensible msg);

		private static readonly MessageSerializer serizlizer = new global::MessageSerializer();
		public static MessageSerializer Serizlizer { get { return serizlizer; } }

		public static void Open(string url = "ws://echo.websocket.org")
		{
			Debug.Log("WebSocket to: " + url);
			Proxy.Open(url);
		}

		public static bool Send(ProtoBuf.IExtensible msg)
		{
			OnSend(msg);
			var buf = serizlizer.Serialize(msg);
			return Proxy.Send(buf);
		}

		public static bool Send(params ProtoBuf.IExtensible[] message)
		{
			var buf = serizlizer.Serialize(message);
			return Proxy.Send(buf);
		}

		public static IEnumerable<ProtoBuf.IExtensible> Receive()
		{
			var buf = Proxy.Receive();
			if (buf == null)
				yield break;
			using (var mem = new MemoryStream(buf))
			{
				while (mem.Position < mem.Length)
				{
					var msg = serizlizer.Deserialize(mem);
					if (msg == null)
						continue;
					OnReceive(msg);
					yield return msg;
				}
			}
		}

		public static void LogSend(ProtoBuf.IExtensible msg)
		{
			var str = msg != null ? msg.Dump() : "null";
			Debug.Log("<color=green>[SEND]</color>" + str);
		}

		public static void LogReceive(ProtoBuf.IExtensible msg)
		{
			var str = msg != null ? msg.Dump() : "null";
			Debug.Log("<color=yellow>[RECV]</color>" + str);
		}
	}
}
#endif