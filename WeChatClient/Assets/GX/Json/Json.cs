using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;


namespace GX
{
	public class Json
	{
		public interface IProxy
		{
			T Deserialize<T>(string value);
			string Serialize(object value);
		}

		public static IProxy Proxy { get; set; }

		static Json()
		{
#if UNITY && ((!UNITY_WP8 && !UNITY_WINRT) || UNITY_EDITOR)
			Proxy = new JsonJsonFx();
#endif
			deserializeMethod = Reflection.GetRuntimeMethod(typeof(Json), "Deserialize", typeof(string));
		}

		public static string Serialize(object value) { return Proxy.Serialize(value); }
		public static T Deserialize<T>(string json) { return Proxy.Deserialize<T>(json); }

		public static T Clone<T>(T value)
		{
			return Deserialize<T>(Serialize(value));
		}

		private static readonly MethodInfo deserializeMethod;
		public static object Deserialize(Type type, string json)
		{
			var deserialize = deserializeMethod.MakeGenericMethod(type);
			return deserialize.Invoke(null, new object[] { json });
		}
		public static object Deserialize(string type, string json)
		{
			var t = Reflection.GetExecutingAssembly().GetType(type);
			return Deserialize(t, json);
		}
	}
}