#if UNITY
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GX.Net
{
	[Obsolete("Please use HttpClient2")]
	public partial class HttpClient
	{
		#region Cmd
#pragma warning disable 0649
		class HttpSend
		{
			public int gameid;
			public int zoneid;
			public string uid;

			public string @do;
			public object data;
		}

		class HttpRecv<T>
		{
			public int gameid;
			public int zoneid;
			public string uid;

			public string @do;
			public T data;

			public string errno;
			public uint st;

			public static HttpRecv<T> Create(WWW www)
			{
				if (www == null)
					return null;
				if (!string.IsNullOrEmpty(www.error))
					return null;

				HttpRecv<T> recv;
				try { recv = Json.Deserialize<HttpRecv<T>>(www.text); }
				catch { return null; }

				if (!string.IsNullOrEmpty(recv.errno) && recv.errno != "0")
					return null;

				return recv;
			}
		}

		class RegisterNewAccount
		{
			public const string Name = "register-newaccount";
			public string mid;
		}

		class RegisterNewAccountOK
		{
			public string mid;
			public string sid;
			public string uid;
		}

		class GetZoneGatewayUrl
		{
			public const string Name = "get-zone-gatewayurl";
			public int gameid;
			public int zoneid;
		}

		class GetZoneGatewayUrlOK
		{
			public int gameid;
			public int zoneid;
			public string gatewayurl;
		}
#pragma warning restore 0649
		#endregion

		public event EventHandler<HttpRequestEventArgs> OnRequest;
		public event EventHandler<HttpResponseEventArgs> OnResponse;

		public static readonly Dictionary<string, string> HeaderJson = new Dictionary<string, string>()
		{
			{"Content-Type", "application/json; charset=utf-8"},
		};

		public string UID { get; set; }
		public string SID { get; set; }

		public int GameID { get; set; }
		public int ZoneID { get; set; }

		public string LoginUrl { get; set; }
		public string GatewayUrl { get; private set; }

		public bool HasSession()
		{
			return !string.IsNullOrEmpty(this.UID) && !string.IsNullOrEmpty(this.SID);
		}

		public void ResetSession()
		{
			this.UID = null;
			this.SID = null;
			Serialize();
		}

		/// <summary>
		/// 适应于unilight服务器的底层消息发送
		/// </summary>
		/// <param name="url"></param>
		/// <param name="do"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		protected WWW SendTo(string url, string @do, object data)
		{
			var json = GX.Json.Serialize(new HttpSend()
			{
				@do = @do,
				data = data,
				uid = UID,
				gameid = GameID,
				zoneid = ZoneID,
			});
			if (!string.IsNullOrEmpty(SID))
				url += "?smd=md5&sign=" + GX.MD5.ComputeHashString(json + SID);
			if (OnRequest != null)
				OnRequest(this, new HttpRequestEventArgs(url, json));
			var www = new WWW(url, GX.Encoding.GetBytes(json), HeaderJson);
			return www;
		}

		/// <summary>
		/// 发送消息，可自动填充平台必要字段
		/// </summary>
		/// <param name="do"></param>
		/// <param name="message"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		protected IEnumerator SendMessage(string @do, object message, Action<WWW> callback)
		{
			#region register-newaccount
			if (HasSession() == false)
			{
				Deserialize();
				if (HasSession() == false)
				{
					var www = SendTo(LoginUrl, RegisterNewAccount.Name, new RegisterNewAccount() { mid = SystemInfo.deviceUniqueIdentifier });
					yield return www;
					if (OnResponse != null)
						OnResponse(this, new HttpResponseEventArgs(www));
					var recv = HttpRecv<RegisterNewAccountOK>.Create(www);
					if (recv == null)
					{
						if (OnResponse == null)
							Debug.LogError(string.Format("[WWW] ERROR {0} {1}\n{2}", LoginUrl, www.error, www.text));
						yield break;
					}

					this.GameID = recv.gameid;
					this.ZoneID = recv.zoneid;

					this.UID = recv.data.uid;
					this.SID = recv.data.sid;
					this.Serialize();
				}
			}
			#endregion

			#region get-zone-gatewayurl
			if (string.IsNullOrEmpty(GatewayUrl))
			{
				var www = SendTo(LoginUrl, GetZoneGatewayUrl.Name, new GetZoneGatewayUrl() { gameid = this.GameID, zoneid = this.ZoneID });
				yield return www;
				if (OnResponse != null)
					OnResponse(this, new HttpResponseEventArgs(www));
				var recv = HttpRecv<GetZoneGatewayUrlOK>.Create(www);
				if (recv == null)
				{
					if (OnResponse == null)
						Debug.LogError(string.Format("[WWW] ERROR {0} {1}\n{2}", LoginUrl, www.error, www.text));
					yield break;
				}

				this.GameID = recv.gameid;
				this.ZoneID = recv.zoneid;
				this.GatewayUrl = recv.data.gatewayurl;
			}
			#endregion

			{
				var www = SendTo(GatewayUrl, @do, message);
				yield return www;
				if (OnResponse != null)
					OnResponse(this, new HttpResponseEventArgs(www));
				if (!string.IsNullOrEmpty(www.error))
				{
					if (OnResponse == null)
						Debug.LogError(string.Format("[WWW] ERROR {0} {1}\n{2}", GatewayUrl, www.error, www.text));
					yield break;
				}
				if (callback != null)
					callback(www);
			}
		}

		public IEnumerator SendAsync(string action, Dictionary<string, object> message, Action<WWW> callback = null)
		{
			return SendMessage(action, message, callback);
		}

		private readonly GX.Net.MessageDispatcher<object> dispatcher = new Net.MessageDispatcher<object>();
		public GX.Net.MessageDispatcher<object> Dispatcher { get { return dispatcher; } }

		public IEnumerator SendAsync<TResponse>(object request, Func<TResponse, IEnumerator> callback = null)
		{
			return SendMessage(request.GetType().Name, request, www =>
			{
				var recv = HttpRecv<TResponse>.Create(www);
				if (recv == null)
				{
					Debug.LogError(string.Format("[HTTP RECV ERROR] {0}", www.text));
					return;
				}
				var message = recv.data;
				IEnumerator coroutine;
				if (callback != null)
				{
					coroutine = callback(recv.data);
				}
				else if (Dispatcher.Dispatch(message, out coroutine) == false)
				{
					Debug.LogWarning(string.Format("未处理的消息: {0}\n{1}", message.GetType(), message.Dump()));
					return;
				}
				if (coroutine != null)
				{
					Singleton.Instance.StartCoroutine(coroutine);
				}
			});
		}

		public void Send<TResponse>(object request, Func<TResponse, IEnumerator> callback)
		{
			Singleton.Instance.StartCoroutine(SendAsync<TResponse>(request, callback));
		}

		public void Send<TResponse>(object request, Action<TResponse> callback = null)
		{
			if (callback == null)
				Singleton.Instance.StartCoroutine(SendAsync<TResponse>(request, null));
			else
				Singleton.Instance.StartCoroutine(SendAsync<TResponse>(request, new Func<TResponse, IEnumerator>(response =>
				{
					callback(response);
					return null;
				})));
		}

		#region Serialize
		private void Serialize()
		{
			PlayerPrefs.SetString("HTTP.UID", this.UID);
			PlayerPrefs.SetString("HTTP.SID", this.SID);
		}

		private void Deserialize()
		{
			this.UID = PlayerPrefs.GetString("HTTP.UID");
			this.SID = PlayerPrefs.GetString("HTTP.SID");
		}
		#endregion
	}
}
#endif
