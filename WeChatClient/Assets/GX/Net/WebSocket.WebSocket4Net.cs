#if GX_PROTOBUF && (!UNITY_WINRT || UNITY_EDITOR)
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GX.Net
{
	class WebSocket4NetProxy : GX.Net.WebSocket.IProxy
	{
		WebSocket4Net.WebSocket socket;
		readonly object syncRoot = new object();
		readonly Queue<byte[]> receiveQueue = new Queue<byte[]>();

		#region IProxy 成员
		public GX.Net.WebSocket.State State
		{
			get
			{
				if (socket == null)
					return GX.Net.WebSocket.State.None;
				switch (socket.State)
				{
					case WebSocket4Net.WebSocketState.None: return GX.Net.WebSocket.State.None;
					case WebSocket4Net.WebSocketState.Connecting: return GX.Net.WebSocket.State.Connecting;
					case WebSocket4Net.WebSocketState.Open: return GX.Net.WebSocket.State.Open;
					case WebSocket4Net.WebSocketState.Closing: return GX.Net.WebSocket.State.Closing;
					case WebSocket4Net.WebSocketState.Closed: return GX.Net.WebSocket.State.Closed;
					default: throw new NotImplementedException();
				}
			}
		}

		public void Open(string url)
		{
			Close();

			socket = new WebSocket4Net.WebSocket(url);
			socket.DataReceived += (s, e) =>
			{
				//Debug.Log("WebSocket DataReceived: length=" + e.Data.Length);
				lock (syncRoot)
				{
					receiveQueue.Enqueue(e.Data);
				}
			};
			socket.Closed += (s, e) => Debug.Log("WebSocket Closed");
			socket.Opened += (s, e) => Debug.Log("WebSocket Opened");
			socket.Error += (s, e) =>
			{
				Close();
				Debug.LogError("WebSocket Error: " + e.Exception.Message);
			};
			socket.MessageReceived += (s, e) => Debug.Log("WebSocket MessageReceived: " + e.Message);

			socket.Open();
		}

		public void Close()
		{
			lock (syncRoot)
			{
				receiveQueue.Clear();
				if (socket != null)
				{
					try { socket.Close(); }
					catch { }
					socket = null;
				}
			}
		}

		public bool Send(byte[] data)
		{
			if (socket == null)
				return false;
			socket.Send(data, 0, data.Length);
			return true;
		}

		public byte[] Receive()
		{
			if (receiveQueue == null)
				return null;
			lock (syncRoot)
			{
				if (receiveQueue.Count == 0)
					return null;
				return receiveQueue.Dequeue();
			}
		}

		#endregion
	}
}
#endif