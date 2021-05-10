#if UNITY && ((!UNITY_WP8 && !UNITY_WINRT) || UNITY_EDITOR)
// need JsonFx.Json.dll
using UnityEngine;
using System.Collections;

namespace GX
{
	class JsonJsonFx : Json.IProxy
	{
		#region IProxy 成员

		public T Deserialize<T>(string value)
		{
#if WINDOWS_PHONE
			return new JsonFx.Json.JsonReader().Read<T>(value);
#else
			return JsonFx.Json.JsonReader.Deserialize<T>(value);
#endif
		}

		public string Serialize(object value)
		{
#if WINDOWS_PHONE
			return new JsonFx.Json.JsonWriter().Write(value);
#else
			return JsonFx.Json.JsonWriter.Serialize(value);
#endif
		}

		#endregion
	}
}
#endif