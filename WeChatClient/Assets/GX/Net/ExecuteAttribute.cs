using System.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using GX;

namespace GX.Net
{
	/// <summary>
	/// 标志是一个可以响应消息的方法
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class ExecuteAttribute : Attribute
	{
		#region 具有ExecuteAttribute特性函数的自动提取
		/// <summary>
		/// 得到类中所有具有<see cref="ExecuteAttribute"/>特性的静态方法
		/// </summary>
		public static IEnumerable<MethodInfo> GetStaticExecuteMethod(Type type)
		{
			return from m in GetExecuteMethod(type) where m.IsStatic select m;
		}

		/// <summary>
		/// 得到所给汇编中所有具有<see cref="ExecuteAttribute"/>特性的静态方法
		/// </summary>
		public static IEnumerable<MethodInfo> GetStaticExecuteMethod(Assembly assembly)
		{
			return (
				from type in assembly.GetAllTypes()
				select GetStaticExecuteMethod(type))
				.SelectMany(s => s);
		}

		/// <summary>
		/// 得到对象中所有具有<see cref="ExecuteAttribute"/>特性的方法
		/// </summary>
		public static IEnumerable<MethodInfo> GetInstanceExecuteMethod(Type targetType)
		{
			return from m in GetExecuteMethod(targetType) where m.IsStatic == false select m;
		}

		private static IEnumerable<MethodInfo> GetExecuteMethod(Type targetType)
		{
#if UNITY_EDITOR
			var flags = BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
			var errors = (
				from method in targetType.GetMethods(flags)
				let tag = method.GetCustomAttribute<ExecuteAttribute>()
				where tag != null
				select method).ToList();
			if (errors.Any())
			{
				var sb = new StringBuilder();
				sb.AppendLine("ExecuteAttribute 必须应用于 public 方法之上：");
				foreach (var m in errors)
					sb.Append(m.ToString()).Append(" @ ").AppendLine(m.ReflectedType.FullName);
				UnityEngine.Debug.LogError(sb.ToString());
			}
#endif
			return
				from method in GX.Reflection.GetRuntimeMethods(targetType)
				let tag = method.GetCustomAttribute<ExecuteAttribute>() 
				where tag != null
				select method;
		}
		#endregion
	}
}
