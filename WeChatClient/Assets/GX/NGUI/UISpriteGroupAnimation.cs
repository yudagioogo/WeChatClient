#if GX_NGUI
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 支持自定义帧的NGUI序列帧动画。
/// 因<see cref="UISpriteAnimation"/>无法进行非侵入式扩展，故拷贝重写
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(UISprite))]
[AddComponentMenu("NGUI/UI/Sprite Group Animation")]
public class UISpriteGroupAnimation : MonoBehaviour
{
	public int FPS = 30;
	public bool loop = true;

	UISprite mSprite;
	float mDelta = 0f;
	int mIndex = 0;
	bool mActive = true;
	public string[] spriteNames;

	/// <summary>
	/// Number of frames in the animation.
	/// </summary>
	public int frames { get { return spriteNames == null ? 0 : spriteNames.Length; } }

	/// <summary>
	/// Animation framerate.
	/// </summary>
	public int framesPerSecond { get { return FPS; } set { FPS = value; } }

	/// <summary>
	/// Returns is the animation is still playing or not
	/// </summary>
	public bool isPlaying { get { return mActive; } }

	void Start()
	{
		mSprite = GetComponent<UISprite>();
	}

	/// <summary>
	/// Advance the sprite animation process.
	/// </summary>
	void Update()
	{
		if (mActive && frames > 1 && Application.isPlaying && FPS > 0f)
		{
			mDelta += RealTime.deltaTime;
			float rate = 1f / FPS;

			if (rate < mDelta)
			{

				mDelta = (rate > 0f) ? mDelta - rate : 0f;
				if (++mIndex >= spriteNames.Length)
				{
					mIndex = 0;
					mActive = loop;
				}

				if (mActive)
				{
					mSprite.spriteName = spriteNames[mIndex];
					mSprite.MakePixelPerfect();
				}
			}
		}
	}

	/// <summary>
	/// Reset the animation to frame 0 and activate it.
	/// </summary>
	public void Reset()
	{
		mActive = true;
		mIndex = 0;

		if (mSprite != null && frames > 0)
		{
			mSprite.spriteName = spriteNames[mIndex];
			mSprite.MakePixelPerfect();
		}
	}
}
#endif