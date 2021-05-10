using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GX
{
	/// <summary>
	/// 跨平台的反射支持
	/// </summary>
	public static class Reflection
	{
		#region Field
		public static FieldInfo GetRuntimeField(this Type type, string name)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return RuntimeReflectionExtensions.GetRuntimeField(type, name);
#else
			return type.GetField(name);
#endif
		}
		public static IEnumerable<FieldInfo> GetRuntimeFields(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return RuntimeReflectionExtensions.GetRuntimeFields(type);
#else
			return type.GetFields();
#endif
		}
		#endregion

		#region Property
		public static PropertyInfo GetRuntimeProperty(this Type type, string name)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return RuntimeReflectionExtensions.GetRuntimeProperty(type, name);
#else
			return type.GetProperty(name);
#endif
		}
		public static IEnumerable<PropertyInfo> GetRuntimeProperties(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return RuntimeReflectionExtensions.GetRuntimeProperties(type);
#else
			return type.GetProperties();
#endif
		}
		#endregion

		#region Event
		public static EventInfo GetRuntimeEvent(this Type type, string name)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return RuntimeReflectionExtensions.GetRuntimeEvent(type, name);
#else
			return type.GetEvent(name);
#endif
		}
		public static IEnumerable<EventInfo> GetRuntimeEvents(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return RuntimeReflectionExtensions.GetRuntimeEvents(type);
#else
			return type.GetEvents();
#endif
		}
		#endregion

		#region Method
		public static MethodInfo GetRuntimeMethod(this Type type, string name, params Type[] parameters)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return RuntimeReflectionExtensions.GetRuntimeMethod(type, name, parameters);
#else
			return type.GetMethod(name, parameters);
#endif
		}

		public static MethodInfo GetMethodInfo(this Delegate func)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return RuntimeReflectionExtensions.GetMethodInfo(func);
#else
			return func.Method;
#endif
		}

		public static IEnumerable<MethodInfo> GetRuntimeMethods(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return RuntimeReflectionExtensions.GetRuntimeMethods(type);
#else
			return type.GetMethods();
#endif
		}
		#endregion

		#region Delegate
		public static Delegate CreateDelegate(this MethodInfo method, Type delegateType, object target = null)
		{
			if (method == null)
				return null;
#if UNITY_METRO && !UNITY_EDITOR
			return target == null ? method.CreateDelegate(delegateType) : method.CreateDelegate(delegateType, target);
#else
			return target == null ? Delegate.CreateDelegate(delegateType, method) : Delegate.CreateDelegate(delegateType, target, method);
#endif
		}
		#endregion

		#region Attribute
		public static Attribute GetCustomAttribute(this MemberInfo element, Type attributeType)
		{
			if (element == null)
				return null;
#if UNITY_METRO && !UNITY_EDITOR
			return CustomAttributeExtensions.GetCustomAttribute(element, attributeType);
#else
			return Attribute.GetCustomAttribute(element, attributeType);
#endif
		}

		public static T GetCustomAttribute<T>(this MemberInfo element)
			where T : Attribute
		{
			return GetCustomAttribute(element, typeof(T)) as T;
		}
		#endregion

		#region Assembly
		public static Assembly GetExecutingAssembly()
		{
#if UNITY_METRO && !UNITY_EDITOR
			return typeof(Reflection).GetTypeInfo().Assembly;
#else
			return typeof(Reflection).Assembly;
#endif
		}
		#endregion

		#region Module
		public static IEnumerable<Module> GetAllModules(this Assembly asm)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return asm.Modules;
#else
			return asm.GetModules();
#endif
		}
		#endregion

		#region Type
		public static IEnumerable<Type> GetAllTypes(this Assembly asm)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return from i in asm.DefinedTypes select i.AsType();
#else
			return asm.GetTypes();
#endif
		}

		public static bool IsAbstract(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsAbstract;
#else
			return type.IsAbstract;
#endif
		}
		public static bool IsAnsiClass(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsAnsiClass;
#else
			return type.IsAnsiClass;
#endif
		}
		public static bool IsArray(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsArray;
#else
			return type.IsArray;
#endif
		}
		public static bool IsAutoClass(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsAutoClass;
#else
			return type.IsAutoClass;
#endif
		}
		public static bool IsAutoLayout(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsAutoLayout;
#else
			return type.IsAutoClass;
#endif
		}
		public static bool IsByRef(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsByRef;
#else
			return type.IsByRef;
#endif
		}
		public static bool IsClass(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsClass;
#else
			return type.IsClass;
#endif
		}
		public static bool IsEnum(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsEnum;
#else
			return type.IsEnum;
#endif
		}
		public static bool IsExplicitLayout(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsExplicitLayout;
#else
			return type.IsExplicitLayout;
#endif
		}
		public static bool IsGenericParameter(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsGenericParameter;
#else
			return type.IsGenericParameter;
#endif
		}
		public static bool IsGenericType(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsGenericType;
#else
			return type.IsGenericType;
#endif
		}
		public static bool IsGenericTypeDefinition(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsGenericTypeDefinition;
#else
			return type.IsGenericTypeDefinition;
#endif
		}
		public static bool IsImport(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsImport;
#else
			return type.IsImport;
#endif
		}
		public static bool IsInterface(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsInterface;
#else
			return type.IsInterface;
#endif
		}
		public static bool IsLayoutSequential(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsLayoutSequential;
#else
			return type.IsLayoutSequential;
#endif
		}
		public static bool IsMarshalByRef(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsMarshalByRef;
#else
			return type.IsMarshalByRef;
#endif
		}
		public static bool IsNested(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsNested;
#else
			return type.IsNested;
#endif
		}
		public static bool IsNestedAssembly(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsNestedAssembly;
#else
			return type.IsNestedAssembly;
#endif
		}
		public static bool IsNestedFamANDAssem(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsNestedFamANDAssem;
#else
			return type.IsNestedFamANDAssem;
#endif
		}
		public static bool IsNestedFamily(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsNestedFamily;
#else
			return type.IsNestedFamily;
#endif
		}
		public static bool IsNestedFamORAssem(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsNestedFamORAssem;
#else
			return type.IsNestedFamORAssem;
#endif
		}
		public static bool IsNestedPrivate(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsNestedPrivate;
#else
			return type.IsNestedPrivate;
#endif
		}
		public static bool IsNestedPublic(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsNestedPublic;
#else
			return type.IsNestedPublic;
#endif
		}
		public static bool IsNotPublic(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsNotPublic;
#else
			return type.IsNotPublic;
#endif
		}
		public static bool IsPointer(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsPointer;
#else
			return type.IsPointer;
#endif
		}
		public static bool IsPrimitive(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsPrimitive;
#else
			return type.IsPrimitive;
#endif
		}
		public static bool IsPublic(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsPublic;
#else
			return type.IsPublic;
#endif
		}
		public static bool IsSealed(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsSealed;
#else
			return type.IsSealed;
#endif
		}
		public static bool IsSerializable(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsSerializable;
#else
			return type.IsSerializable;
#endif
		}
		public static bool IsSpecialName(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsSpecialName;
#else
			return type.IsSpecialName;
#endif
		}
		public static bool IsUnicodeClass(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsUnicodeClass;
#else
			return type.IsUnicodeClass;
#endif
		}
		public static bool IsValueType(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsValueType;
#else
			return type.IsValueType;
#endif
		}
		public static bool IsVisible(this Type type)
		{
#if UNITY_METRO && !UNITY_EDITOR
			return type.GetTypeInfo().IsVisible;
#else
			return type.IsVisible;
#endif
		}

		#endregion
	}
}
