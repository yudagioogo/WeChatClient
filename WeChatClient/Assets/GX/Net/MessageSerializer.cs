#if GX_PROTOBUF
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace GX.Net
{
	/// <summary>
	/// <see cref="ProtoBuf.IExtensible"/>对象和字节流间的编码器
	/// </summary>
	/// <remarks>性能分析参见：http://www.servicestack.net/benchmarks/NorthwindDatabaseRowsSerialization.100000-times.2010-08-17.html </remarks>
	public abstract class MessageSerializer : IEnumerable<KeyValuePair<Type, MessageType>>
	{
		private readonly Dictionary<Type, MessageType> tableType2ID = new Dictionary<Type, MessageType>();
		private readonly Dictionary<MessageType, Type> tableID2Type = new Dictionary<MessageType, Type>();

		public MessageType this[Type type]
		{
			get 
			{
				MessageType id;
				return tableType2ID.TryGetValue(type, out id) ? id : MessageType.Empty;
			}
		}

		public Type this[MessageType id]
		{
			get
			{
				Type ret;
				return tableID2Type.TryGetValue(id, out ret) ? ret : null;
			}
		}

		#region Serialize
		public byte[] Serialize(ProtoBuf.IExtensible message)
		{
			using (var mem = new MemoryStream())
			{
				SerializeTo(mem, message);
				return mem.ToArray();
			}
		}

		public byte[] Serialize(IEnumerable<ProtoBuf.IExtensible> message)
		{
			using (var mem = new MemoryStream())
			{
				foreach (var m in message)
					SerializeTo(mem, m);
				return mem.ToArray();
			}
		}

		public abstract void SerializeTo(Stream stream, ProtoBuf.IExtensible message);

		#endregion

		#region Deserialize

		public ProtoBuf.IExtensible Deserialize(byte[] messagePackageData)
		{
			using(var mem = new MemoryStream(messagePackageData, 0, messagePackageData.Length))
			{
				return Deserialize(mem);
			}
		}

		public ProtoBuf.IExtensible Deserialize(byte[] messagePackageData, int offset, int count)
		{
			using(var mem = new MemoryStream(messagePackageData, offset, count))
			{
				return Deserialize(mem);
			}
		}

		public abstract ProtoBuf.IExtensible Deserialize(Stream stream);

		#endregion

		#region Register
		/// <summary>注册可被解析的消息类型</summary>
		/// <typeparam name="T">可被解析的消息类型ID</typeparam>
		/// <param name="messageTypeID"><typeparamref name="T"/>对应的<see cref="ProtoBuf.IExtensible"/>类型</param>
		public virtual void Register<T>(MessageType messageTypeID) where T : ProtoBuf.IExtensible
		{
			// 反序列化预编译
			ProtoBuf.Serializer.PrepareSerializer<T>();

			// 注册
			tableType2ID[typeof(T)] = messageTypeID;
			tableID2Type[messageTypeID] = typeof(T);			
		}

		/// <summary>注册可被解析的消息类型</summary>
		/// <param name="messageTypeID">可被解析的消息类型ID</param>
		/// <param name="messageType"><paramref name="messageTypeID"/>对应的<see cref="ProtoBuf.IExtensible"/>类型</param>
		/// <remarks>对泛型重载Register&lt;T&gt;的非泛型包装</remarks>
		public void Register(MessageType messageTypeID, Type messageType)
		{
			// Call Register<messageType>(messageTypeID) by refelect.
			this.GetType().GetRuntimeMethod("Register", typeof(MessageType))
				.MakeGenericMethod(messageType)
				.Invoke(this, new object[] { messageTypeID });
		}

		#endregion

		public override string ToString()
		{
			var sb = new StringBuilder();
			foreach (var pair in tableType2ID)
			{
				sb.AppendLine(pair.Value + " " + pair.Key);
			}
			return sb.ToString();
		}

		#region IEnumerable<KeyValuePair<Type,MessageType>> 成员

		public IEnumerator<KeyValuePair<Type, MessageType>> GetEnumerator()
		{
			return tableType2ID.GetEnumerator();
		}

		#endregion

		#region IEnumerable 成员

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		#endregion
	}
}
#endif