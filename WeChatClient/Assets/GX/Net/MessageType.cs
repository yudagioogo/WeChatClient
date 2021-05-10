#if GX_PROTOBUF || GX_WEBSOCKET
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GX.Net
{
	/// <summary>
	/// 用数字表示的<see cref="ProtoBuf.IExtensible"/>消息类型
	/// </summary>
	public struct MessageType : IComparable<MessageType>, IEquatable<MessageType>
	{
		public static readonly MessageType Empty = new MessageType();
			 
		public uint Cmd { get; set; }
		public uint Param { get; set; }

		#region Equatable
		public static bool operator ==(MessageType a, MessageType b)
		{
			if (System.Object.ReferenceEquals(a, b))
				return true;
			if (((object)a == null) || ((object)b == null))
				return false;
			return a.Cmd == b.Cmd && a.Param == b.Param;
		}
		public static bool operator !=(MessageType a, MessageType b)
		{
			return !(a == b);
		}

		#region IEquatable<MessageType> 成员

		public bool Equals(MessageType other)
		{
			return this == other;
		}

		#endregion

		public override bool Equals(object obj)
		{
			return obj is MessageType? this == (MessageType)obj : false;
		}

		public override int GetHashCode()
		{
			return ((int)this.Cmd << 16) | ((int)this.Param & 0x0000FFFF);
		}
		#endregion

		#region IComparable<MessageType> 成员

		public int CompareTo(MessageType other)
		{
			if (this.Cmd > other.Cmd)
				return 1;
			else if (this.Cmd < other.Cmd)
				return -1;

			if (this.Param > other.Param)
				return 1;
			else if (this.Param < other.Param)
				return -1;

			return 0;
		}

		#endregion

		public override string ToString()
		{
			return string.Format("0x{0:X8}", this.GetHashCode());
		}
	}
}
#endif