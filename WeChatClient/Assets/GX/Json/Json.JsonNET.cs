#if GX_JSONNET
using UnityEngine;
using System.Collections;

namespace GX
{
	class JsonJsonNET : Json.IProxy
	{
		#region IProxy 成员
		public T Deserialize<T>(string value)
		{
			return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value);
		}

		public string Serialize(object value)
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject(value);
		}
		#endregion
	}
}
#endif