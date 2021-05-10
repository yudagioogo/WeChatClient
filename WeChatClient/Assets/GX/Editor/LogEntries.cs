#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GX.Editor
{
	public static class LogEntries
	{
		static readonly Type type = Assembly.GetAssembly(typeof(UnityEditor.ActiveEditorTracker)).GetType("UnityEditorInternal.LogEntries");

		/// <summary>
		/// http://answers.unity3d.com/questions/10580/editor-script-how-to-clear-the-console-output-wind.html
		/// </summary>
		public static void Clear()
		{
			type.GetMethod("Clear").Invoke(new object(), null);
		}
	}
}
#endif
