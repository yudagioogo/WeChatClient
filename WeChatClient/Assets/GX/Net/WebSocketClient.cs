#if GX_PROTOBUF
using UnityEngine;
using System.Collections;
using GX;
using GX.Net;
using System.IO;
using System;

public class WebSocketClient : Singleton<WebSocketClient>
{
	public const string EchoServer = "ws://echo.websocket.org";

	public MessageDispatcher<ProtoBuf.IExtensible> Dispatcher { get; private set; }
	public GX.Net.WebSocket.State State { get { return WebSocket.Proxy.State; } }
	private Coroutine m_dispatchCoroutine;

	protected WebSocketClient()
	{
		this.Dispatcher = new MessageDispatcher<ProtoBuf.IExtensible>();
		this.Dispatcher.StaticRegister();
	}

	public IEnumerator Open(string url)
	{
		WebSocket.Open(url);
		while (true)
		{
			if (this.State != WebSocket.State.Connecting)
				break;
			yield return new WaitForEndOfFrame();
		}
	}

	public void Close()
	{
		WebSocket.Proxy.Close();
	}

	public bool Send(ProtoBuf.IExtensible message)
	{
		return WebSocket.Send(message);
	}

	public void Send(params ProtoBuf.IExtensible[] message)
	{
		WebSocket.Send(message);
	}

	public void SendToMe(ProtoBuf.IExtensible message)
	{
		IEnumerator coroutine;
		Dispatcher.Dispatch(message, out coroutine);
		if (coroutine != null)
			StartCoroutine(coroutine);
	}

	void Start()
	{
		m_dispatchCoroutine = StartCoroutine(Dispatch());
	}

	IEnumerator Dispatch()
	{
		while (Application.isPlaying)
		{
			yield return null;
			foreach (var message in WebSocket.Receive())
			{
				IEnumerator coroutine;
				if (Dispatcher.Dispatch(message, out coroutine) == false)
					Debug.LogWarning(string.Format("未处理的消息: {0}\n{1}", message.GetType(), message.Dump()));
				if (coroutine != null)
				{
					Singleton.Instance.StartCoroutine(coroutine);
				}
			}
		}
	}

	protected override void OnDestroy()
	{
		StopCoroutine(m_dispatchCoroutine);
		WebSocket.Proxy.Close();
		WebSocket.Proxy = null;

		base.OnDestroy();
	}
}
#endif