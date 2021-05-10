#if UNITY
using UnityEngine;
using System.Collections;
using System.Linq;

/// <summary>
/// 编辑器下自动场景跳转。可跳转到默认场景(如:登陆界面)或其他指定场景，方便开发
/// </summary>
public class EditorSceneLoader : MonoBehaviour
{
#if UNITY_EDITOR
	public string targetSceneName;

	public static bool IsDone { get; private set; }
	IEnumerator Start()
	{
		if (IsDone)
			yield break;
		var current = UnityEditor.EditorApplication.currentScene;
		var target = GetTarget() ?? current;
		if (current != target)
		{
			Debug.Log(string.Format("Auto jump: {0} -> {1}", current, target));
			var ao = Application.LoadLevelAsync(System.IO.Path.GetFileNameWithoutExtension(target));
			while (ao.isDone == false)
				yield return ao;
		}
		IsDone = true;
	}

	private string GetTarget()
	{
		if (string.IsNullOrEmpty(targetSceneName) == false)
			return targetSceneName;
		var start = UnityEditor.EditorBuildSettings.scenes.FirstOrDefault(s => s.enabled);
		if (start != null)
			return start.path;
		return null;
	}
#else
	public static bool IsDone { get{ return true; } }
#endif
}
#endif
