using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using SLua;

/// <summary>
/// Event trigger listener.
/// </summary>
[CustomLuaClass]
public class XUIEventTriggerListener : UnityEngine.EventSystems.EventTrigger
{
	public delegate void VoidDelegate (LuaTable mod, GameObject go, BaseEventData eventData);

	public VoidDelegate		onClick;
	public VoidDelegate 	onDown;
	public VoidDelegate 	onEnter;
	public VoidDelegate 	onExit;
	public VoidDelegate 	onUp;
	public VoidDelegate 	onSelect;
	public VoidDelegate 	onUpdateSelect;
	public VoidDelegate     onBeginDrag;
	public VoidDelegate     onDrag;
	public VoidDelegate     onEndDrag;

	public LuaTable			luaModule
	{ get; set; }

	/// <summary>
	/// Get the specified go.
	/// </summary>
	/// <param name="go">Go.</param>
	static public XUIEventTriggerListener Get (GameObject go, LuaTable mod)
	{
		XUIEventTriggerListener listener = go.GetComponent<XUIEventTriggerListener>();
		if (!listener) {
			listener = go.AddComponent<XUIEventTriggerListener> ();
		}

		if (listener.luaModule == null)
			listener.luaModule = mod;
		
		return listener;
	}

	/// <summary>
	/// Raises the pointer click event.
	/// </summary>
	/// <param name="eventData">Event data.</param>
	public override void OnPointerClick(PointerEventData eventData)
	{
		if(onClick != null) 	
			onClick(luaModule, gameObject, eventData);
	}

	/// <summary>
	/// Raises the pointer down event.
	/// </summary>
	/// <param name="eventData">Event data.</param>
	public override void OnPointerDown (PointerEventData eventData)
	{
		if(onDown != null) 
			onDown(luaModule, gameObject, eventData);
	}

	/// <summary>
	/// Raises the pointer enter event.
	/// </summary>
	/// <param name="eventData">Event data.</param>
	public override void OnPointerEnter (PointerEventData eventData)
	{
		if(onEnter != null) 
			onEnter(luaModule, gameObject, eventData);
	}

	/// <summary>
	/// Raises the pointer exit event.
	/// </summary>
	/// <param name="eventData">Event data.</param>
	public override void OnPointerExit (PointerEventData eventData)
	{
		if(onExit != null)
			onExit(luaModule, gameObject, eventData);
	}

	/// <summary>
	/// Raises the pointer up event.
	/// </summary>
	/// <param name="eventData">Event data.</param>
	public override void OnPointerUp (PointerEventData eventData)
	{
		if(onUp != null) 
			onUp(luaModule, gameObject, eventData);
	}

	/// <summary>
	/// Raises the select event.
	/// </summary>
	/// <param name="eventData">Event data.</param>
	public override void OnSelect (BaseEventData eventData)
	{
		if(onSelect != null) 
			onSelect(luaModule, gameObject, eventData);
	}

	/// <summary>
	/// Raises the update selected event.
	/// </summary>
	/// <param name="eventData">Event data.</param>
	public override void OnUpdateSelected (BaseEventData eventData)
	{
		if(onUpdateSelect != null) 
			onUpdateSelect(luaModule, gameObject, eventData);
	}

	public override void OnBeginDrag(PointerEventData eventData)
	{
		if (onBeginDrag != null)
			onBeginDrag(luaModule, gameObject, eventData);
	}

	public override void OnDrag(PointerEventData eventData)
	{
		if (onDrag != null)
			onDrag(luaModule, gameObject, eventData);
	}

	public override void OnEndDrag(PointerEventData eventData)
	{
		if (onEndDrag != null)
			onEndDrag(luaModule, gameObject, eventData);
	}
}
