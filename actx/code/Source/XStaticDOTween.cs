using UnityEngine;
using DG.Tweening;
using SLua;
using UnityEngine.Audio;
using System;
using UnityEngine.UI;

[CustomLuaClass]
public enum XEase
{
	Unset = 0,
	Linear = 1,
	InSine = 2,
	OutSine = 3,
	InOutSine = 4,
	InQuad = 5,
	OutQuad = 6,
	InOutQuad = 7,
	InCubic = 8,
	OutCubic = 9,
	InOutCubic = 10,
	InQuart = 11,
	OutQuart = 12,
	InOutQuart = 13,
	InQuint = 14,
	OutQuint = 15,
	InOutQuint = 16,
	InExpo = 17,
	OutExpo = 18,
	InOutExpo = 19,
	InCirc = 20,
	OutCirc = 21,
	InOutCirc = 22,
	InElastic = 23,
	OutElastic = 24,
	InOutElastic = 25,
	InBack = 26,
	OutBack = 27,
	InOutBack = 28,
	InBounce = 29,
	OutBounce = 30,
	InOutBounce = 31,
	INTERNAL_Zero = 32,
	INTERNAL_Custom = 33
}

[CustomLuaClass]
public enum XLoopType
{
	Restart = 0,
	Yoyo = 1,
	Incremental = 2
}

[CustomLuaClass]
public enum XUpdateType
{
	Normal = 0,
	Late = 1,
	Fixed = 2
}

[CustomLuaClass]
public enum XRotateMode
{
	Fast = 0,
	FastBeyond360 = 1,
	WorldAxisAdd = 2,
	LocalAxisAdd = 3
}

[CustomLuaClass]
public enum XAxisConstraint
{
	None = 0,
	X = 2,
	Y = 4,
	Z = 8,
	W = 16
}

[CustomLuaClass]
public enum XScrambleMode
{
	None = 0,
	All = 1,
	Uppercase = 2,
	Lowercase = 3,
	Numerals = 4,
	Custom = 5,
}

[CustomLuaClass]
public class XTween
{
	public Tween tween;

	/// <summary>
	/// Initializes a new instance of the <see cref="XTween"/> class.
	/// </summary>
	public XTween()
	{
		
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="XTween"/> class.
	/// </summary>
	/// <param name="_tween">Tween.</param>
	public XTween(Tween _tween)
	{
		if (_tween != null)
		{
			tween = _tween;
		}
		else
		{
			throw new ArgumentNullException("Tween is null");
		}
	}

	/// <summary>
	/// Sets A.
	/// </summary>
	/// <returns>The A.</returns>
	/// <param name="_tween">Tween.</param>
	public XTween SetAS(XTween _tween)
	{
		tween.SetAs(_tween.tween);
		return this;
	}

	/// <summary>
	/// Sets the auto kill.
	/// </summary>
	/// <returns>The auto kill.</returns>
	/// <param name="autoKillOnCompletion">If set to <c>true</c> auto kill on completion.</param>
	public XTween SetAutoKill(bool autoKillOnCompletion)
	{
		tween.SetAutoKill(autoKillOnCompletion);
		return this;
	}

	/// <summary>
	/// Sets the ease.
	/// </summary>
	/// <returns>The ease.</returns>
	/// <param name="easeType">Ease type.</param>
	public XTween SetEase(int easeType)
	{
		Ease ease = (Ease)easeType;
		tween.SetEase(ease);
		return this;
	}

	/// <summary>
	/// Sets the identifier.
	/// </summary>
	/// <returns>The identifier.</returns>
	/// <param name="id">Identifier.</param>
	public XTween SetId(object id)
	{
		tween.SetId(id);
		return this;
	}

	/// <summary>
	/// Sets the loops.
	/// </summary>
	/// <returns>The loops.</returns>
	/// <param name="loops">Loops.</param>
	/// <param name="loopType">Loop type.</param>
	public XTween SetLoops(int loops,int loopType)
	{
		LoopType loop = (LoopType)loopType;
		tween.SetLoops(loops,loop);
		return this;
	}

	/// <summary>
	/// Sets the recyclable.
	/// </summary>
	/// <returns>The recyclable.</returns>
	/// <param name="recyclable">If set to <c>true</c> recyclable.</param>
	public XTween SetRecyclable(bool recyclable)
	{
		tween.SetRecyclable(recyclable);
		return this;
	}

	/// <summary>
	/// Sets the relative.
	/// </summary>
	/// <returns>The relative.</returns>
	/// <param name="isRelative">If set to <c>true</c> is relative.</param>
	public XTween SetRelative(bool isRelative)
	{
		tween.SetRelative(isRelative);
		return this;
	}

	/// <summary>
	/// Sets the update.
	/// </summary>
	/// <returns>The update.</returns>
	/// <param name="updateType">Update type.</param>
	/// <param name="isIndependentUpdate">If set to <c>true</c> is independent update.</param>
	public XTween SetUpdate(int updateType, bool isIndependentUpdate)
	{
		UpdateType update = (UpdateType)updateType;
		tween.SetUpdate(update, isIndependentUpdate);
		return this;
	}
	/// <summary>
	/// Sets the delay.
	/// </summary>
	/// <returns>The delay.</returns>
	/// <param name="_delay">Delay.</param>
	public XTween SetDelay(float _delay)
	{
		tween.SetDelay(_delay);
		return this;
	}

	/// <summary>
	/// Sets the speed based.
	/// </summary>
	/// <returns>The speed based.</returns>
	/// <param name="isSpeedBased">If set to <c>true</c> is speed based.</param>
	public XTween SetSpeedBased(bool isSpeedBased)
	{
		tween.SetSpeedBased(isSpeedBased);
		return this;
	}

	/// <summary>
	/// From the specified isRelative.
	/// </summary>
	/// <param name="isRelative">If set to <c>true</c> is relative.</param>
	public XTween From(bool isRelative)
	{
		if(tween is Tweener) (tween as Tweener).From(isRelative);
		return this;
	}

	public XTween ChangeStartValue(object newStartValue)
	{
		(tween as Tweener).ChangeStartValue (newStartValue);
		return this;
	}

	public XTween ChangeValues(object start, object end)
	{
		(tween as Tweener).ChangeValues (start, end);
		return this;
	}

	/// <summary>
	/// Changes the end value.
	/// </summary>
	/// <returns>The end value.</returns>
	/// <param name="newEndValue">New end value.</param>
	/// <param name="newDuration">New duration.</param>
	/// <param name="snapStartValue">If set to <c>true</c> snap start value.</param>
	public XTween ChangeEndValue(object newEndValue, float newDuration, bool snapStartValue)
	{
		(tween as Tweener).ChangeEndValue(newEndValue, newDuration, snapStartValue);
		return this;
	}

	/// <summary>
	/// Raises the complete event.
	/// </summary>
	/// <param name="_callback">Callback.</param>
	public XTween OnComplete(LuaFunction _callback)
	{
		TweenCallback callback = new TweenCallback(() =>
			{
				if (_callback != null) _callback.call();
			});
		tween.OnComplete(callback);
		return this;
	}

	/// <summary>
	/// Raises the kill event.
	/// </summary>
	/// <param name="_callback">Callback.</param>
	public XTween OnKill(LuaFunction _callback)
	{
		TweenCallback callback = new TweenCallback(() =>
			{
				if (_callback != null) _callback.call();
			});
		tween.OnKill(callback);
		return this;
	}

	/// <summary>
	/// Raises the play event.
	/// </summary>
	/// <param name="_callback">Callback.</param>
	public XTween OnPlay(LuaFunction _callback)
	{
		TweenCallback callback = new TweenCallback(() =>
			{
				if (_callback != null) _callback.call();
			});
		tween.OnPlay(callback);
		return this;
	}

	/// <summary>
	/// Raises the pause event.
	/// </summary>
	/// <param name="_callback">Callback.</param>
	public XTween OnPause(LuaFunction _callback)
	{
		TweenCallback callback = new TweenCallback(() =>
			{
				if (_callback != null) _callback.call();
			});
		tween.OnPause(callback);
		return this;
	}

	/// <summary>
	/// Raises the rewind event.
	/// </summary>
	/// <param name="_callback">Callback.</param>
	public XTween OnRewind(LuaFunction _callback)
	{
		TweenCallback callback = new TweenCallback(() =>
			{
				if (_callback != null) _callback.call();
			});
		tween.OnRewind(callback);
		return this;
	}

	/// <summary>
	/// Raises the start event.
	/// </summary>
	/// <param name="_callback">Callback.</param>
	public XTween OnStart(LuaFunction _callback)
	{
		TweenCallback callback = new TweenCallback(() =>
			{
				if (_callback != null) _callback.call();
			});
		tween.OnStart(callback);
		return this;
	}

	/// <summary>
	/// Raises the step complete event.
	/// </summary>
	/// <param name="_callback">Callback.</param>
	public XTween OnStepComplete(LuaFunction _callback)
	{
		TweenCallback callback = new TweenCallback(() =>
			{
				if (_callback != null) _callback.call();
			});
		tween.OnStepComplete(callback);
		return this;
	}

	/// <summary>
	/// Raises the update event.
	/// </summary>
	/// <param name="_callback">Callback.</param>
	public XTween OnUpdate(LuaFunction _callback)
	{
		TweenCallback callback = new TweenCallback(() =>
			{
				if (_callback != null) _callback.call();
			});
		tween.OnUpdate(callback);
		return this;
	}

	/// <summary>
	/// Raises the waypoint change event.
	/// </summary>
	/// <param name="_callback">Callback.</param>
	public XTween OnWaypointChange(LuaFunction _callback)
	{
		TweenCallback<int> callback = new TweenCallback<int>((waypointIndex) =>
			{
				if (_callback != null) _callback.call(waypointIndex);
			});
		tween.OnWaypointChange(callback);
		return this;
	}

	/// <summary>
	/// Complete this instance.
	/// </summary>
	public void Complete()
	{
		tween.Complete();
	}

	/// <summary>
	/// Flip this instance.
	/// </summary>
	public void Flip()
	{
		tween.Flip();
	}

	/// <summary>
	/// Goto the specified to and andPlay.
	/// </summary>
	/// <param name="to">To.</param>
	/// <param name="andPlay">If set to <c>true</c> and play.</param>
	public void Goto(float to,bool andPlay)
	{
		tween.Goto(to,andPlay);
	}

	/// <summary>
	/// Kill the specified complete.
	/// </summary>
	/// <param name="complete">If set to <c>true</c> complete.</param>
	public void Kill(bool complete=true)
	{
		tween.Kill(complete);
	}

	/// <summary>
	/// Pause this instance.
	/// </summary>
	public void Pause()
	{
		tween.Pause();
	}

	/// <summary>
	/// Play this instance.
	/// </summary>
	public void Play()
	{
		tween.Play();
	}

	/// <summary>
	/// Plaies the backwards.
	/// </summary>
	public void PlayBackwards()
	{
		tween.PlayBackwards();
	}

	/// <summary>
	/// Plaies the forward.
	/// </summary>
	public void PlayForward()
	{
		tween.PlayForward();
	}

	/// <summary>
	/// Restart the specified includeDelay.
	/// </summary>
	/// <param name="includeDelay">If set to <c>true</c> include delay.</param>
	public void Restart(bool includeDelay)
	{
		tween.Restart(includeDelay);
	}

	/// <summary>
	/// Rewind the specified includeDelay.
	/// </summary>
	/// <param name="includeDelay">If set to <c>true</c> include delay.</param>
	public void Rewind(bool includeDelay)
	{
		tween.Rewind(includeDelay);
	}

	/// <summary>
	/// Smooths the rewind.
	/// </summary>
	public void SmoothRewind()
	{
		tween.SmoothRewind();
	}

	/// <summary>
	/// Toggles the pause.
	/// </summary>
	public void TogglePause()
	{
		tween.TogglePause();
	}

	/// <summary>
	/// Forces the init.
	/// </summary>
	public void ForceInit()
	{
		tween.ForceInit();
	}

	/// <summary>
	/// Gotos the waypoint.
	/// </summary>
	/// <param name="waypointIndex">Waypoint index.</param>
	/// <param name="andPlay">If set to <c>true</c> and play.</param>
	public void GotoWaypoint(int waypointIndex, bool andPlay)
	{
		tween.GotoWaypoint(waypointIndex,andPlay);
	}

	/// <summary>
	/// Fulls the position.
	/// </summary>
	/// <returns>The position.</returns>
	public float fullPosition()
	{
		return tween.fullPosition;
	}

	/// <summary>
	/// Completeds the loops.
	/// </summary>
	/// <returns>The loops.</returns>
	public int CompletedLoops()
	{
		return tween.CompletedLoops();
	}

	/// <summary>
	/// Delay this instance.
	/// </summary>
	public float Delay()
	{
		return tween.Delay();
	}

	/// <summary>
	/// Duration the specified includeLoops.
	/// </summary>
	/// <param name="includeLoops">If set to <c>true</c> include loops.</param>
	public float Duration(bool includeLoops)
	{
		return tween.Duration(includeLoops);
	}

	/// <summary>
	/// Elapsed the specified includeLoops.
	/// </summary>
	/// <param name="includeLoops">If set to <c>true</c> include loops.</param>
	public float Elapsed(bool includeLoops)
	{
		return tween.Elapsed(includeLoops);
	}

	/// <summary>
	/// Elapseds the directional percentage.
	/// </summary>
	/// <returns>The directional percentage.</returns>
	public float ElapsedDirectionalPercentage()
	{
		return tween.ElapsedDirectionalPercentage();
	}

	/// <summary>
	/// Elapseds the percentage.
	/// </summary>
	/// <returns>The percentage.</returns>
	/// <param name="includeLoops">If set to <c>true</c> include loops.</param>
	public float ElapsedPercentage(bool includeLoops = true)
	{
		return tween.ElapsedPercentage(includeLoops);
	}

	/// <summary>
	/// Determines whether this instance is active.
	/// </summary>
	/// <returns><c>true</c> if this instance is active; otherwise, <c>false</c>.</returns>
	public bool IsActive()
	{
		return tween.IsActive();
	}

	/// <summary>
	/// Determines whether this instance is backwards.
	/// </summary>
	/// <returns><c>true</c> if this instance is backwards; otherwise, <c>false</c>.</returns>
	public bool IsBackwards()
	{
		return tween.IsBackwards();
	}

	/// <summary>
	/// Determines whether this instance is complete.
	/// </summary>
	/// <returns><c>true</c> if this instance is complete; otherwise, <c>false</c>.</returns>
	public bool IsComplete()
	{
		return tween.IsComplete();
	}

	/// <summary>
	/// Determines whether this instance is initialized.
	/// </summary>
	/// <returns><c>true</c> if this instance is initialized; otherwise, <c>false</c>.</returns>
	public bool IsInitialized()
	{
		return tween.IsInitialized();
	}

	/// <summary>
	/// Determines whether this instance is playing.
	/// </summary>
	/// <returns><c>true</c> if this instance is playing; otherwise, <c>false</c>.</returns>
	public bool IsPlaying()
	{
		return tween.IsPlaying();
	}

	/// <summary>
	/// Loops this instance.
	/// </summary>
	public int Loops()
	{
		return tween.Loops();
	}
}

[CustomLuaClass]
public class XStaticDOTween
{
	public static XSequence Sequence()
	{
		return new XSequence();
	}

	public static XTween To(LuaFunction callback,float startValue, float endValue, float duration)
	{
		return new XTween(DOTween.To(
			(v) => { if (callback != null) callback.call(v); }, 
			startValue, endValue, duration));
	}

	public static XTween Punch(LuaFunction getter, LuaFunction setter, Vector3 direction, float duration, int vibrato, float elasticity)
	{
		return new XTween(DOTween.Punch(
			() => { return (Vector3)getter.call(); },
			(v) => { setter.call(v); },
			direction, duration, vibrato, elasticity));
	}

	public static XTween Shake(LuaFunction getter, LuaFunction setter, float duration, float strength, int vibrato, float randomness, bool ignoreZAxis)
	{
		return new XTween(DOTween.Shake(
			() => { return (Vector3)getter.call(); },
			(v) => { setter.call(v); },
			duration, strength, vibrato, randomness, ignoreZAxis));
	}

	public static XTween ToAlpha(LuaFunction getter, LuaFunction setter, float to, float duration)
	{
		return new XTween(DOTween.ToAlpha(
			() => { return (Color)getter.call(); },
			(v) => { setter.call(v); },
			to, duration));
	}

	public static XTween ToArray(LuaFunction getter, LuaFunction setter, Vector3[] to, float[] duration)
	{
		return new XTween(DOTween.ToArray(
			() => { return (Vector3)getter.call(); },
			(v) => { setter.call(v); },
			to, duration));
	}
	
	public static XTween DOMove(object obj, Vector3 to, float duration,bool snapping)
	{
		if (obj is Transform) return new XTween((obj as Transform).DOMove(to, duration, snapping));
		return null;
	}

	public static XTween DOMoveX(object obj, float to, float duration,bool snapping)
	{
		if (obj is Transform) return new XTween((obj as Transform).DOMoveX(to, duration, snapping));
		return null;
	}

	public static XTween DOMoveY(object obj, float to, float duration, bool snapping)
	{
		if (obj is Transform) return new XTween((obj as Transform).DOMoveY(to, duration, snapping));
		return null;
	}

	public static XTween DOMoveZ(object obj, float to, float duration, bool snapping)
	{
		if (obj is Transform) return new XTween((obj as Transform).DOMoveZ(to, duration, snapping));
		return null;
	}

	public static XTween DOLocalMove(object obj, Vector3 to, float duration, bool snapping)
	{
		if (obj is Transform) return new XTween((obj as Transform).DOLocalMove(to, duration, snapping));
		return null;
	}

	public static XTween DOLocalMoveX(object obj, float to, float duration, bool snapping)
	{
		if (obj is Transform) return new XTween((obj as Transform).DOLocalMoveX(to, duration, snapping));
		return null;
	}

	public static XTween DOLocalMoveY(object obj, float to, float duration, bool snapping)
	{
		if (obj is Transform) return new XTween((obj as Transform).DOLocalMoveY(to, duration, snapping));
		return null;
	}

	public static XTween DOLocalMoveZ(object obj, float to, float duration, bool snapping)
	{
		if (obj is Transform) return new XTween((obj as Transform).DOLocalMoveZ(to, duration, snapping));
		return null;
	}

	public static XTween DOJump(object obj, Vector3 endValue, float jumpPower, int numJumps, float duration, bool snapping)
	{
		if (obj is Transform) return new XTween((obj as Transform).DOJump(endValue,jumpPower,numJumps, duration, snapping));
		return null;
	}

	public static XTween DOLocalJump(object obj, Vector3 endValue, float jumpPower, int numJumps, float duration, bool snapping)
	{
		if (obj is Transform) return new XTween((obj as Transform).DOLocalJump(endValue, jumpPower, numJumps, duration, snapping));
		return null;
	}

	public static XTween DOFieldOfView(object obj, float to, float duration)
	{
		if (obj is Camera) return new XTween((obj as Camera).DOFieldOfView(to, duration));
		return null;
	}

	public static XTween DORotate(object obj, Vector3 to, float duration, int mode)
	{
		RotateMode md = (RotateMode)mode;
		if (obj is Transform) return new XTween((obj as Transform).DORotate(to, duration, md));
		return null;
	}

	public static XTween DOLocalRotate(object obj, Vector3 to, float duration, int mode)
	{
		RotateMode md = (RotateMode)mode;
		if (obj is Transform) return new XTween((obj as Transform).DOLocalRotate(to, duration, md));
		return null;
	}

	public static XTween DOLookAt(object obj, Vector3 towards, float duration, int axisConstraint, Vector3 up)
	{
		AxisConstraint md = (AxisConstraint)axisConstraint;
		if (obj is Transform) return new XTween((obj as Transform).DOLookAt(towards, duration, md, up));
		return null;
	}
	
	public static XTween DOScaleV(object obj, Vector3 to, float duration)
	{
		if (obj is Transform) return new XTween((obj as Transform).DOScale(to, duration));
		return null;
	}

	public static XTween DOScale(object obj, float to, float duration)
	{
		if (obj is Transform) return new XTween((obj as Transform).DOScale(to, duration));
		return null;
	}

	public static XTween DOScaleX(object obj, float to, float duration)
	{
		if (obj is Transform) return new XTween((obj as Transform).DOScaleX(to, duration));
		return null;
	}

	public static XTween DOScaleY(object obj, float to, float duration)
	{
		if (obj is Transform) return new XTween((obj as Transform).DOScaleY(to, duration));
		return null;
	}

	public static XTween DOScaleZ(object obj, float to, float duration)
	{
		if (obj is Transform) return new XTween((obj as Transform).DOScaleZ(to, duration));
		return null;
	}

	public static XTween DOPunchPosition(object obj, Vector3 punch, float duration, int vibrato, float elasticity, bool snapping)
	{
		if (obj is Transform) return new XTween((obj as Transform).DOPunchPosition(punch, duration, vibrato, elasticity, snapping));
		return null;
	}

	public static XTween DOPunchRotation(object obj, Vector3 punch, float duration, int vibrato, float elasticity)
	{
		if (obj is Transform) return new XTween((obj as Transform).DOPunchRotation(punch, duration, vibrato, elasticity));
		return null;
	}

	public static XTween DOPunchScale(object obj, Vector3 punch, float duration, int vibrato, float elasticity)
	{
		if (obj is Transform) return new XTween((obj as Transform).DOPunchScale(punch, duration, vibrato, elasticity));
		return null;
	}

	public static XTween DOShakePosition(object obj, float duration, Vector3 strength, int vibrato, float randomness)
	{
		if (obj is Camera) return new XTween((obj as Camera).DOShakePosition(duration, strength, vibrato, randomness));
		return null;
	}

	public static XTween DOShakeRotation(object obj, float duration, Vector3 strength, int vibrato, float randomness)
	{
		if (obj is Transform) return new XTween((obj as Transform).DOShakeRotation(duration, strength, vibrato, randomness));
		return null;
	}

	public static XTween DOShakeRotation(object obj, float duration, float strength, int vibrato, float randomness)
	{
		if (obj is Transform) return new XTween((obj as Transform).DOShakeRotation(duration, strength, vibrato, randomness));
		return null;
	}

	public static XTween DOShakeScale(object obj, float duration, Vector3 strength, int vibrato, float randomness)
	{
		if (obj is Transform) return new XTween((obj as Transform).DOShakeScale(duration, strength, vibrato, randomness));
		return null;
	}

	public static XTween DOShakeScale(object obj, float duration, float strength, int vibrato, float randomness)
	{
		if (obj is Transform) return new XTween((obj as Transform).DOShakeScale(duration, strength, vibrato, randomness));
		return null;
	}

	public static XTween DOBlendableMoveBy(object obj, Vector3 by, float duration, bool snapping)
	{
		if (obj is Transform) return new XTween((obj as Transform).DOBlendableMoveBy(by, duration, snapping));
		return null;
	}

	public static XTween DOBlendableLocalMoveBy(object obj, Vector3 by, float duration, bool snapping)
	{
		if (obj is Transform) return new XTween((obj as Transform).DOBlendableLocalMoveBy(by, duration, snapping));
		return null;
	}

	public static XTween DOBlendableRotateBy(object obj, Vector3 by, float duration, int mode)
	{
		RotateMode md = (RotateMode)mode;
		if (obj is Transform) return new XTween((obj as Transform).DOBlendableRotateBy(by, duration, md));
		return null;
	}

	public static XTween DOBlendableLocalRotateBy(object obj, Vector3 by, float duration, int mode)
	{
		RotateMode md = (RotateMode)mode;
		if (obj is Transform) return new XTween((obj as Transform).DOBlendableLocalRotateBy(by, duration, md));
		return null;
	}

	public static XTween DOBlendableScaleBy(object obj, Vector3 by, float duration)
	{
		if (obj is Transform) return new XTween((obj as Transform).DOBlendableScaleBy(by, duration));
		return null;
	}
	
	public static XTween DOColor(object obj, Color to, float duration)
	{
		if (obj is Camera) return new XTween((obj as Camera).DOColor(to, duration));
		if (obj is Light) return new XTween((obj as Light).DOColor(to, duration));
		if (obj is Material) return new XTween((obj as Material).DOColor(to, duration));
		if (obj is SpriteRenderer) return new XTween((obj as SpriteRenderer).DOColor(to, duration));
		if (obj is Graphic) return new XTween((obj as Graphic).DOColor(to, duration));
		if (obj is Image) return new XTween((obj as Image).DOColor(to, duration));
		if (obj is Outline) return new XTween((obj as Outline).DOColor(to, duration));
		if (obj is Text) return new XTween((obj as Text).DOColor(to, duration));
		return null;
	}

	public static XTween DOFade(object obj, float to, float duration)
	{
		if (obj is AudioSource) return new XTween((obj as AudioSource).DOFade(to, duration));
		if (obj is Material) return new XTween((obj as Material).DOFade(to, duration));
		if (obj is SpriteRenderer) return new XTween((obj as SpriteRenderer).DOFade(to, duration));
		if (obj is CanvasGroup) return new XTween((obj as CanvasGroup).DOFade(to, duration));
		if (obj is Graphic) return new XTween((obj as Graphic).DOFade(to, duration));
		if (obj is Image) return new XTween((obj as Image).DOFade(to, duration));
		if (obj is Outline) return new XTween((obj as Outline).DOFade(to, duration));
		if (obj is Text) return new XTween((obj as Text).DOFade(to, duration));
		return null;
	}

	public static XTween DOText(object obj,string to, float duration, bool richTextEnabled, int scrambleMode, string scrambleChars)
	{
		ScrambleMode md = (ScrambleMode)scrambleMode;
		if (obj is Text) return new XTween((obj as Text).DOText(to, duration, richTextEnabled, md, scrambleChars));
		return null;
	}

	public static XTween DOBlendableColor(object obj, Color to, float duration)
	{
		if (obj is Text) return new XTween((obj as Text).DOBlendableColor(to, duration));
		return null;
	}

	public static XTween DOFillAmount(object obj, float to, float duration)
	{
		if (obj is Image) return new XTween((obj as Image).DOFillAmount(to, duration));
		return null;
	}

	public static XTween DOAnchorPos(object obj,Vector2 to, float duration, bool snapping)
	{
		if (obj is RectTransform) return new XTween((obj as RectTransform).DOAnchorPos(to, duration, snapping));
		return null;
	}

	public static XTween DOAnchorPosX(object obj, float to, float duration, bool snapping)
	{
		if (obj is RectTransform) return new XTween((obj as RectTransform).DOAnchorPosX(to, duration, snapping));
		return null;
	}

	public static XTween DOAnchorPosY(object obj, float to, float duration, bool snapping)
	{
		if (obj is RectTransform) return new XTween((obj as RectTransform).DOAnchorPosY(to, duration, snapping));
		return null;
	}

	public static XTween DOAnchorPos3D(object obj, Vector3 to, float duration, bool snapping)
	{
		if (obj is RectTransform) return new XTween((obj as RectTransform).DOAnchorPos3D(to, duration, snapping));
		return null;
	}

	public static XTween DOJumpAnchorPos(object obj, Vector2 endValue, float jumpPower, int numJumps, float duration, bool snapping)
	{
		if (obj is RectTransform) return new XTween((obj as RectTransform).DOJumpAnchorPos(endValue, jumpPower, numJumps, duration, snapping));
		return null;
	}

	public static XTween DOPunchAnchorPos(object obj, Vector2 punch, float duration, int vibrato, float elasticity, bool snapping)
	{
		if (obj is RectTransform) return new XTween((obj as RectTransform).DOPunchAnchorPos(punch, duration, vibrato, elasticity, snapping));
		return null;
	}

	public static XTween DOShakeAnchorPos(object obj, float duration, Vector3 strength, int vibrato, float randomness, bool snapping)
	{
		if (obj is RectTransform) return new XTween((obj as RectTransform).DOShakeAnchorPos(duration, strength, vibrato, randomness, snapping));
		return null;
	}

	public static XTween DOShakeAnchorPos(object obj, float duration, float strength, int vibrato, float randomness, bool snapping)
	{
		if (obj is RectTransform) return new XTween((obj as RectTransform).DOShakeAnchorPos(duration, strength, vibrato, randomness, snapping));
		return null;
	}

	public static XTween DOSizeDelta(object obj, Vector2 to, float duration, bool snapping)
	{
		if (obj is RectTransform) return new XTween((obj as RectTransform).DOSizeDelta( to, duration, snapping));
		return null;
	}

}

[CustomLuaClass]
public class XSequence : XTween
{
	public XSequence()
	{
		tween = DOTween.Sequence();
	}

	public XSequence Append(XTween t)
	{
		(tween as Sequence).Append(t.tween);
		return this;
	}

	public XSequence AppendCallback(LuaFunction _callback)
	{
		TweenCallback callback = new TweenCallback(() =>
			{
				if (_callback != null) _callback.call();
			});
		(tween as Sequence).AppendCallback(callback);
		return this;
	}

	public XSequence AppendInterval(float interval)
	{
		(tween as Sequence).AppendInterval(interval);
		return this;
	}

	public XSequence Insert(float atPosition, XTween t)
	{
		(tween as Sequence).Insert(atPosition, t.tween);
		return this;
	}

	public XSequence InsertCallback(float atPosition, LuaFunction _callback)
	{
		TweenCallback callback = new TweenCallback(() =>
			{
				if (_callback != null) _callback.call();
			});
		(tween as Sequence).InsertCallback(atPosition, callback);
		return this;
	}

	public XSequence Join(XTween t)
	{
		(tween as Sequence).Join(t.tween);
		return this;
	}

	public XSequence Prepend(XTween t)
	{
		(tween as Sequence).Prepend(t.tween);
		return this;
	}

	public XSequence PrependCallback(LuaFunction _callback)
	{
		TweenCallback callback = new TweenCallback(() =>
			{
				if (_callback != null) _callback.call();
			});
		(tween as Sequence).PrependCallback(callback);
		return this;
	}

	public XSequence PrependInterval(float interval)
	{
		(tween as Sequence).PrependInterval(interval);
		return this;
	}
}