//#if !UNITY
#if false
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityEngine
{
	/// <summary>
	/// Class containing methods to ease debugging while developing a game.
	/// </summary>
	public sealed partial class Debug
	{
		public static void Log(object message)
		{
			Console.WriteLine(message);
		}
		public static void Log(object message, Object context)
		{
			Console.WriteLine(message);
			if (context != null)
				Console.WriteLine("    " + context);
		}

		public static void LogWarning(object message)
		{
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.WriteLine(message);
			Console.ResetColor();
		}
		public static void LogWarning(object message, Object context)
		{
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.WriteLine(message);
			if (context != null)
				Console.WriteLine("    " + context);
			Console.ResetColor();
		}

		public static void LogError(object message)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(message);
			Console.ResetColor();
		}
		public static void LogError(object message, Object context)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(message);
			if (context != null)
				Console.WriteLine("    " + context);
			Console.ResetColor();
		}

		public static void LogException(Exception exception)
		{
			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.WriteLine(exception);
			Console.ResetColor();
		}
		public static void LogException(Exception exception, Object context)
		{
			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.WriteLine(exception);
			if (context != null)
				Console.WriteLine("    " + context);
			Console.ResetColor();
		}
	}
}

#endif