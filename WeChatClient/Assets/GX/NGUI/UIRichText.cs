#if GX_NGUI
using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using GX;

/// <summary>
/// 富文本控件
/// </summary>
[RequireComponent(typeof(UIWidget))]
public class UIRichText : MonoBehaviour
{
	/// <summary>
	/// 所用的文本元型(prototype)
	/// </summary>
	public GameObject protoLabel;

	/// <summary>
	/// 超链接采用的BBCode格式修饰
	/// </summary>
	public string LinkDecorate = "[u]{0}[/u]";

	/// <summary>
	/// URL点击事件
	/// </summary>
	public event Action<UIWidget, string> UrlClicked;
	protected void OnUrlClicked(UIWidget sender, string link)
	{
		if (this.UrlClicked != null)
			this.UrlClicked(sender, link);
	}

	// 允许在界面中指定引用关系
	public UIWidget host;

	#region Layout
	private Vector2 m_layout;
	/// <summary>
	/// 用于当前行元素的高度调整
	/// </summary>
	private readonly List<UIWidget> m_line = new List<UIWidget>();
	/// <summary>
	/// 当前行的最大元素高度
	/// </summary>
	private float m_maxLineHeight;

	public void AddNewLine()
	{
		m_layout.x = 0;
		m_layout.y -= NGUIText.finalLineHeight;
		host.height = Mathf.CeilToInt(-m_layout.y);

		m_maxLineHeight = NGUIText.finalLineHeight;
		m_line.Clear();
	}

	public bool IsNewLine()
	{
		return m_line.Count == 0;
	}
	private void Layout(UIWidget widget)
	{
		widget.MakePixelPerfect();
		widget.name = this.transform.childCount.ToString();

		// 记录行元素，用于可能发生的高度调整
		m_line.Add(widget);

		// 本行能放下，直接放
		if (widget.localSize.x <= host.width - m_layout.x)
		{
			widget.gameObject.transform.localPosition = m_layout;
		}
		else // 本行放不下，换行再放
		{
			AddNewLine();
			widget.gameObject.transform.localPosition = m_layout;
		}

		m_layout.x += widget.localSize.x;

		// 调整行高度
		if (widget.localSize.y > m_maxLineHeight)
		{
			var delta = widget.localSize.y - m_maxLineHeight;
			m_maxLineHeight = widget.localSize.y;
			m_layout.y -= delta;
			host.height = Mathf.CeilToInt(-m_layout.y);
			foreach (var c in m_line)
			{
				var pos = c.gameObject.transform.localPosition;
				pos.y -= delta;
				c.gameObject.transform.localPosition = pos;
			}
		}

		// 已经接近行尾则换行
		if (host.width - m_layout.x < NGUIText.finalLineHeight)
			AddNewLine();
		//Debug.Log(string.Format("Layot: {0}: {1}, {2}, {3}", widget.name, m_layout.y, m_layout.x, widget.localSize));
	}
	#endregion

	void Start()
	{
		if (host == null)
		{
			host = this.GetComponent<UIWidget>();
			Clear();
		}
	}

	public void Clear()
	{
		this.transform.DestroyAllChildren();
		m_layout = Vector2.zero;
		AddNewLine();
	}

	/// <summary>
	/// 添加文本
	/// 不支持NGUI的BBCode富文本编码！
	/// </summary>
	protected void AddRawText(string text, ICollection<UIWidget> paragraph)
	{
		if (string.IsNullOrEmpty(text))
			return;
		text = text.Replace("\t", "    ");
		var c = CreateLabel();
		var index = 0;
		while (index < text.Length)
		{
			var cut = c.WrapLine(text, index);
			if (cut == index)
			{
				AddNewLine();
				c.width = host.width;
				continue;
			}
			c.overflowMethod = UILabel.Overflow.ResizeFreely; // 测量结束后，恢复溢出模式
			c.text = text.Substring(index, cut - index);
			if (paragraph != null)
				paragraph.Add(c);
			Layout(c);
			if (cut >= text.Length)
				break;
			c = CreateLabel();
			index = cut;
		}
	}

	/// <summary>
	/// 添加文本
	/// </summary>
	/// <param name="text">要添加的文本。'\n'表示换行，'\t'将被替换为"    "</param>
	/// <param name="paragraph">本次添加生成的所有<see cref="UILabel"/></param>
	public void AddText(string text, ICollection<UIWidget> paragraph = null)
	{
		if (string.IsNullOrEmpty(text))
			return;
		text = NGUIText.StripSymbols(text);
		var lines = text.Split(new char[] { '\n' });
		for (var i = 0; i < lines.Length - 1; i++)
		{
			AddRawText(lines[i], paragraph);
			AddNewLine();
		}
		AddRawText(lines.Last(), paragraph);
	}

	/// <summary>
	/// 添加超链接，超链接点击事件为<see cref="UrlClicked"/>
	/// <paramref name="link"/>为空则退化为普通文本添加，相当于<see cref="AddText"/>
	/// </summary>
	/// <param name="text">要添加的文本。'\n'表示换行，'\t'将被替换为"    "</param>
	/// <param name="link"></param>
	/// <param name="paragraph">本次添加生成的所有<see cref="UILabel"/></param>
	public void AddLink(string text, string link, ICollection<UIWidget> paragraph = null)
	{
		var widgets = new List<UIWidget>();
		AddText(text, widgets);
		foreach (var w in widgets)
		{
			AttachLink(w, link);
			if (paragraph != null)
				paragraph.Add(w);
		}
	}

	protected UIWidget AttachLink(UIWidget widget, string link)
	{
		var collider = widget.GetOrAddComponent<BoxCollider>();
		collider.isTrigger = true;
		widget.ResizeCollider();

		var sender = widget;
		UIEventListener.Get(widget.gameObject).onClick = go => OnUrlClicked(sender, link);

		var label = widget as UILabel;
		if (label != null)
		{
			label.supportEncoding = true;
			label.text = string.Format(LinkDecorate, label.text);
		}

		return widget;
	}

	/// <summary>
	/// 添加图片
	/// </summary>
	/// <param name="atlas">图片atlas的Resouces路径</param>
	/// <param name="sprite">atlas中的name</param>
	/// <returns>失败返回null</returns>
	public UISprite AddSprite(string atlas, string sprite = null)
	{
		var res = Resources.Load<UIAtlas>(atlas);
		if (res == null)
			return null;
		var c = CreateSprite();
		c.atlas = res;
		c.spriteName = sprite;
		Layout(c);
		return c;
	}

	protected UILabel CreateLabel()
	{
		var item = NGUITools.AddChild(this.gameObject, protoLabel);
		var c = item.GetComponent<UILabel>();
		c.text = string.Empty;
		c.supportEncoding = false;
		c.overflowMethod = UILabel.Overflow.ResizeHeight;
		c.width = host.width - Mathf.CeilToInt(m_layout.x);
		c.maxLineCount = 1;
		c.rawPivot = UIWidget.Pivot.BottomLeft;
		return c;
	}

	protected UISprite CreateSprite()
	{
		var item = NGUITools.AddChild(this.gameObject);
		var c = item.AddComponent<UISprite>();
		c.rawPivot = UIWidget.Pivot.BottomLeft;
		return c;
	}
}
#endif