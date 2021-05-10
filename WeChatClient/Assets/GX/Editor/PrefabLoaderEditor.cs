#if GX_NGUI
using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(PrefabLoader))]
public class PrefabLoaderEditor : Editor
{
	public SerializedObject prefabLoader;
	public SerializedProperty prefabs;
	public PrefabLoader Target { get { return (PrefabLoader)target; } }

	private static GUILayoutOption buttonWidth = GUILayout.MaxWidth(20f);
	private static GUIContent insert = new GUIContent("+", "duplicate");
	private static GUIContent delete = new GUIContent("-", "delete");

	void OnEnable()
	{
		prefabLoader = new SerializedObject(target);
		prefabs = prefabLoader.FindProperty("prefabs");
	}

	public override void OnInspectorGUI()
	{
		prefabLoader.Update();

		if (GUILayout.Button(insert, EditorStyles.miniButtonLeft, buttonWidth))
		{
			prefabs.arraySize++;
		}

		for (int i = 0; i < prefabs.arraySize; i++)
		{
			SerializedProperty s = prefabs.GetArrayElementAtIndex(i);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(s);
			if (GUILayout.Button(insert, EditorStyles.miniButtonLeft, buttonWidth))
			{
				prefabs.InsertArrayElementAtIndex(i);
			}
			if (GUILayout.Button(delete, EditorStyles.miniButtonRight, buttonWidth))
			{
				prefabs.DeleteArrayElementAtIndex(i);
			}
			EditorGUILayout.EndHorizontal();
		}
		prefabLoader.ApplyModifiedProperties();
	}
}
#endif