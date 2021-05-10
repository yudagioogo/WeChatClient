#if GX_NGUI
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 挂在PopList的父节点(popList应该有一个专职父节点，否则弹出框会错位)。
/// popList的扩展，解决NGUI的popList存在的各种问题。
/// </summary>
public class UIPopListExtension : MonoBehaviour
{
	public UIPopupList PopListContorl;
	public UIWidget WidgetPos;
	public BoxCollider TouchMask;

	void Awake()
	{
		//CreateBoxCollider();
	}

	void LateUpdate()
	{
		TouchMask.enabled = PopListContorl.isOpen;
	}

	/// <summary>
	/// 动态生成一个box colider，解决下拉框选择时，点击事件“穿透”bug。实际上不是穿透，是下拉列表提前销毁了。
	/// </summary>
	private void CreateBoxCollider()
	{
		var boxCollider = this.gameObject.AddComponent<BoxCollider>();
		boxCollider.size = new Vector3(1, 1, 0);
		WidgetPos.autoResizeBoxCollider = true;
	}
}
#endif
