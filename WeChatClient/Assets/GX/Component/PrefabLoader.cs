#if GX_NGUI
using UnityEngine;
using System.Collections;

public class PrefabLoader : MonoBehaviour
{
	public GameObject[] prefabs;
	public bool Done { get; private set; }

	void Start()
	{
		foreach (var pf in prefabs)
		{
			if (pf == null)
				continue;
			var go = GameObject.Instantiate(pf) as GameObject;
			go.transform.parent = this.transform;
			go.transform.localScale = Vector3.one;
			go.transform.localPosition = Vector3.zero;

			var widget = go.transform.GetComponent<UIWidget>();
			if (widget != null)
				widget.SetAnchor(this.transform);
		}
		Done = true;
	}
}
#endif