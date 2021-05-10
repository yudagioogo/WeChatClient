#if UNITY
using UnityEngine;
using System.Collections;
using System;

public class OnDestroyAction : MonoBehaviour
{
	public Action Action { get; set; }
	void OnDestroy()
	{
		if (Action != null)
			Action();
	}
}
#endif
