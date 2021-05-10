#if GX_WEBSOCKET
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using UnityEngine;
using Pmd;

namespace GX.Net
{
	/// <summary>
	/// 基于`WebSocket`的`json`通信
	/// </summary>
	public partial class JsonSocket : IDisposable
	{
		private string url;
		private WebSocket4Net.WebSocket socket;
		public MessageDispatcher<object> Dispatcher { get; private set; }

		private List<string> sendCache = new List<string>();
		private readonly object sendCacheLock = new object();

		private List<string> received = new List<string>();
		private readonly object receivedLock = new object();
		private Coroutine m_dispatchCoroutine;

		public JsonSocket(string url)
		{
			this.url = url;
			this.socket = new WebSocket4Net.WebSocket(url);
			this.Dispatcher = new MessageDispatcher<object>();
			this.Dispatcher.Register(this);

			Debug.Log("<color=magenta>[WS OPENING]</color> " + url);

			this.socket.Opened += (s, e) =>
			{
				Debug.Log("<color=magenta>[WS OPEN]</color> " + url);
				lock (sendCacheLock)
				{
					if (this.sendCache != null)
					{
						foreach (var str in this.sendCache)
						{
							this.socket.Send(str);
							Debug.Log("<color=green>[WS SEND CACHE]</color> " + str);
						}
						this.sendCache = null;
					}
				}
			};

			this.socket.MessageReceived += (s, e) =>
			{
				Debug.Log("<color=cyan>[WS RECV]</color> " + e.Message);
				lock (receivedLock)
				{
					received.Add(e.Message);
				}
			};

			this.socket.Error += (s, e) =>
			{
				Debug.LogError("<color=red>[WS ERROR]</color> " + url);
			};

			this.socket.Closed += (s, e) =>
			{
				Debug.Log("<color=magenta>[WS CLOSE]</color> " + url);
				this.socket = null;
			};

			this.m_dispatchCoroutine = Singleton.Instance.StartCoroutine(this.Dispatch());
			this.socket.Open();
		}

		private IEnumerator Dispatch()
		{
			while (Application.isPlaying)
			{
				yield return null;
				List<string> messages = null;
				lock (receivedLock)
				{
					if (received.Count == 0)
						continue;
					messages = received;
					received = new List<string>();
				}

				List<object> msgs;
				IEnumerator coroutine;
				foreach (var json in messages)
				{
					try
					{
						msgs = Deserialize(json).ToList();
					}
					catch (Exception ex)
					{
						Debug.LogError("[WS PARSE ERROR] " + ex.Message + "\n" + json);
						continue;
					}
					foreach (var msg in msgs)
					{
						if (Dispatcher.Dispatch(msg, out coroutine) == false)
							Debug.LogWarning(string.Format("未处理的消息: {0}\n{1}", msg.GetType(), json));
						if (coroutine != null)
						{
							while (coroutine.MoveNext())
								yield return coroutine.Current;
						}
					}
				}

			}
		}

		public void Send(object message, bool loopback = false)
		{
			if (message == null)
				return;
			var str = Serialize(message);
			if (string.IsNullOrEmpty(str))
				return;

			if (loopback)
			{
				lock (receivedLock)
				{
					received.Add(str);
				}
				return;
			}


			if (this.sendCache != null)
			{
				lock (sendCacheLock)
				{
					if (this.sendCache != null)
					{
						this.sendCache.Add(str);
						Debug.Log("<color=yellow>[WS CACHE]</color> " + str);
						return;
					}
				}
			}

			if (this.socket == null)
			{
				Debug.LogError("<color=red>[WS SEND ERROR]</color> can't send message when websocket closed: " + str);
				return;
			}

			this.socket.Send(str);
			Debug.Log("<color=green>[WS SEND]</color> " + str);
		}

		public void Close()
		{
			if (this.socket != null)
			{
				Debug.Log("<color=magenta>[WS CLOSING]</color> " + this.url);
				this.sendCache = null;
				this.socket.Close();
				this.socket = null;
			}
		}

		#region IDisposable 成员

		public void Dispose()
		{
			Close();
			this.Dispatcher.UnRegister(this);
		}

		#endregion

		#region Serialization
		private const string GatewayWrapperName = "Pmd.UserJsMessageForwardUserPmd_CS";

		private static string Serialize(object message)
		{
			var messageType = message.GetType().FullName;
			// 所有不在`Pmd.`下面的消息需要采用`UserJsMessageForwardUserPmd_CS`包装，以方便网关服务器直接转发
			if (messageType.Substring(0, 4) != "Pmd.")
			{
				return Json.Serialize(new Dictionary<string, object>()
				{
					{"cmd_name", GatewayWrapperName},
					{"msg", Json.Serialize(new Dictionary<string, object>()
						{
							{"do", messageType},
							{"data", message},
						})},
				});
			}
			else
			{
				var str = Json.Serialize(message);
				if (str.StartsWith("{") == false)
				{
					Debug.LogError("[WS SEND ERROR] unsupported message format: " + str);
					return string.Empty;
				}

				var cmd_name = string.Format("\"cmd_name\":\"{0}\"", messageType);
				if (str == "{}")
					return "{" + cmd_name + "}";
				else
					return str.Insert(1, cmd_name + ",");
			}
		}

		private static IEnumerable<object> Deserialize(string json)
		{
			///消息格式：
			// {
			//     "cmd_name" : "Cmd.TestJsonsocket",
			//     "testMsg" : "abc",
			//     "errno" : "0"
			// }
			///或：
			// {
			//     "cmd_name" : "Pmd.UserJsMessageForwardUserPmd_CS",
			//     "errno" : "0",
			//     "msg" : "{\"errno\":\"0\",\"st\":1447813514,\"data\":{\"testMsg\":\"abc\",\"errno\":\"0\"},\"do\":\"Cmd.TestJsonsocket\"}"
			// }
			///或：
			// [
			// {
			//     "cmd_name" : "Pmd.UserJsMessageForwardUserPmd_CS",
			//     "msg" : "{\"errno\":\"0\",\"st\":1447813514,\"data\":{\"testMsg\":\"abc\",\"errno\":\"0\"},\"do\":\"Cmd.TestJsonsocket\"}"
			// },
			// {
			//     "cmd_name" : "Cmd.TestJsonsocket",
			//     "testMsg" : "abc",
			//     "errno" : "0"
			// }
			// ]
			if (string.IsNullOrEmpty(json))
				yield break;
			var content = json.First() == '[' ? json : "[" + json + "]";
			foreach (var dic in Json.Deserialize<Dictionary<string, object>[]>(content))
			{
				if (HttpRecvError(dic) != null)
				{
					Debug.LogError("[WS RUN ERROR] " + HttpRecvError(dic) + "\n" + json);
					continue;
				}
				var cmd_name = (string)dic["cmd_name"];
				// 采用 Pmd.UserJsMessageForwardUserPmd_CS 包装的下行消息
				if (cmd_name == GatewayWrapperName)
				{
					///消息格式：
					// {
					//     "do" : "Cmd.TestJsonsocket",
					//     "errno" : "0",
					//     "st" : 1447813514,
					//     "data" : {
					//         "testMsg" : "abc",
					//         "errno" : "0"
					//     },
					// }
					var msg = Json.Deserialize<Dictionary<string, object>>((string)dic["msg"]);
					if (HttpRecvError(dic) != null)
					{
						Debug.LogError("[WS RUN ERROR] " + HttpRecvError(dic) + "\n" + json);
						continue;
					}
					var type = (string)msg["do"];
					var data = (string)Json.Serialize(msg["data"]);
					yield return Json.Deserialize(type, data);
				}
				else
				{
					///消息格式：
					// {
					//     "cmd_name" : "Cmd.TestJsonsocket",
					//     "testMsg" : "abc",
					//     "errno" : "0"
					// }
					yield return Json.Deserialize(cmd_name, json);
				}
			}
		}

		private static string HttpRecvError(Dictionary<string, object> dic)
		{
			if (dic == null)
				return "<null>";
			object errno;
			if (dic.TryGetValue("errno", out errno) == false)
				return null;
			var e = errno.ToString();
			if (e == "" || e == "0")
				return null;
			return e;
		}
		#endregion

		#region unilight 弱联网 转 强联网
		/// <summary>
		/// 由弱联网转强联网链接
		/// </summary>
		/// <param name="http"></param>
		/// <param name="success"></param>
		/// <param name="error"></param>
		public static void CreateAsync(JsonHttp http, Action<JsonSocket, UserLoginReturnOkLoginUserPmd_S> success, Action<string, UserLoginReturnFailLoginUserPmd_S> error = null)
		{
			ulong accountid = 0;
			if (http.UID.TryParse(out accountid) == false)
			{
				var msg = string.Format("[WS ERROR] Parse HTTP.UID error: {0}", http.UID);
				if (error == null) Debug.LogError(msg); else error(msg, null);
				return;
			}
			http.SendAsync(new WebSocketForwardUserPmd_C() { accountid = accountid },
				(WebSocketForwardUserPmd_S recv) =>
				{
					var websocket = new JsonSocket(recv.jsongatewayurl);
					websocket.Dispatcher.StaticRegister();
					new WebSocketCreateProxy().Register(websocket, success, error);
					// websocket登陆消息
					var cmd = new UserLoginTokenLoginUserPmd_C()
					{
						gameid = http.GameID,
						zoneid = http.ZoneID,
						accountid = recv.accountid,
						logintempid = recv.logintempid,
						timestamp = DateTime.Now.ToUnixTime(),
						compress = http.Compress.ToString(),
						encrypt = null,
						encryptkey = null,
					};
					cmd.tokenmd5 = MD5.ComputeHashString(cmd.accountid.ToString() + cmd.logintempid.ToString() + cmd.timestamp.ToString() + recv.tokenid.ToString());
					websocket.Send(cmd);
				},
				error == null ? null : new Action<string>(s => error(s, null)));
		}

		private static readonly List<JsonSocket> keeplive = new List<JsonSocket>();

		public class WebSocketCreateProxy
		{
			private JsonSocket target;
			private Action<JsonSocket, UserLoginReturnOkLoginUserPmd_S> success;
			private Action<string, UserLoginReturnFailLoginUserPmd_S> error;

			public WebSocketCreateProxy Register(JsonSocket target, Action<JsonSocket, UserLoginReturnOkLoginUserPmd_S> success, Action<string, UserLoginReturnFailLoginUserPmd_S> error)
			{
				this.target = target;
				this.success = success;
				this.error = error;

				keeplive.Add(this.target);
				this.target.Dispatcher.Register(this);
				return this;
			}

			public void UnRegister()
			{
				keeplive.Remove(this.target);
				this.target.Dispatcher.UnRegister(this);
			}

			[Execute]
			public void WebSocketLoginReturnOK(UserLoginReturnOkLoginUserPmd_S cmd)
			{
				UnRegister();
				this.target.url = cmd.gatewayurl;
				if (success != null)
					success(target, cmd);

			}
			[Execute]
			public void WebSocketLoginReturnError(UserLoginReturnFailLoginUserPmd_S cmd)
			{
				UnRegister();
				if (error != null)
					error("http to websocket error", cmd);
			}
		}

		[Execute]
		public static void WebSocketDisconnect(ReconnectKickoutLoginUserPmd_S cmd)
		{
			Debug.LogError("wesocket disconnect");
		}
		/// <summary>
		/// 时间心跳
		/// </summary>
		/// <param name="cmd"></param>
		[Execute]
		public void Execute(TickRequestNullUserPmd_CS cmd)
		{
			this.Send(new TickReturnNullUserPmd_CS()
			{
				requesttime = cmd.requesttime,
				mytime = DateTime.Now.ToUnixTime(),
			});
		}
		#endregion
	}

	[Obsolete("Please use JsonSocket instead")]
	public partial class WebSocketJson : JsonSocket
	{
		public WebSocketJson(string url)
			: base(url)
		{
		}
	}
}
#endif