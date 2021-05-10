#if GX_NGUI
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 自动移动到UIroot，解决主界面scrollview背景图的zorder问题
/// </summary>
class AutoMoveToUIRoot : MonoBehaviour
{
	void Awake()
	{
		var rootObj = GameObject.Find("UI Root");
		if (rootObj == null)
		{
			Debug.Log("未找到UI Root");
			return;
		}

		var srcLocalScale = this.transform.localScale;
		this.transform.SetParent(rootObj.transform);
		this.transform.localScale = srcLocalScale;
	}
}
#endif
