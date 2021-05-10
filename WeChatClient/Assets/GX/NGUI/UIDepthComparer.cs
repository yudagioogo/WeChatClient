#if GX_NGUI
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIDepthComparer : Comparer<Transform>
{
	public override int Compare(Transform x, Transform y)
	{
		if (x == y)
			return 0;
		if (x == null)
			return -1;
		if (y == null)
			return 1;
		var dx = GetDepth(x);
		var dy = GetDepth(y);
		if (dx.Item1 != dy.Item1)
			return dx.Item1 - dy.Item1;
		if (dx.Item2 != dy.Item2)
			return dx.Item2 - dy.Item2;
		return x.GetInstanceID() - y.GetInstanceID();
	}

	/// <summary>
	/// 得到<see cref="UIPanel.depth"/>和<see cref="UIWidget.depth"/>元组
	/// </summary>
	/// <param name="go"></param>
	/// <returns></returns>
	public static Tuple<int, int> GetDepth(Transform go)
	{
		UIPanel panel = null;
		UIWidget widget = null;
		if (go != null)
		{
			widget = go.GetComponent<UIWidget>();
			for (var g = go.transform; g != null; g = g.transform.parent)
			{
				panel = g.GetComponent<UIPanel>();
				if (panel != null)
					break;
			}
		}
		return Tuple.Create(panel != null ? panel.depth : 0, widget != null ? widget.depth : 0);
	}
}
#endif