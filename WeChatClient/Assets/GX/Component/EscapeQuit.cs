#if UNITY
using UnityEngine;
using System.Collections;

/// <summary>
/// 键盘Escape退出游戏
/// </summary>
public class EscapeQuit : MonoBehaviour
{
	public string lastSceneName;

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (string.IsNullOrEmpty(lastSceneName))
				ExitGame();
			else
				Application.LoadLevel(lastSceneName);
		}
	}

	/// <summary>
	/// 能同时适应于真机和编辑器的退出游戏
	/// </summary>
	public static void ExitGame()
	{
		Time.timeScale = 0;
		Input.ResetInputAxes();
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_METRO_8_0 || UNITY_METRO_8_1
		if(OnExitGame != null)
			OnExitGame();
		else
			Application.Quit();
#else
		Application.Quit();
#endif
	}

#if UNITY_METRO_8_0 || UNITY_METRO_8_1
	public static System.Action OnExitGame;
#endif
}
#endif
