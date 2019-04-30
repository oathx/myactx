using SLua;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CustomLuaClass]
public class XLuaBehaviourScript : MonoBehaviour
{
	protected LuaFunction m_luaAwake 		            = null;
	protected LuaFunction m_luaStart 		            = null;
	protected LuaFunction m_luaUpdate 		            = null;
	protected LuaFunction m_luaLateUpdate 	            = null;
	protected LuaFunction m_luaFixedUpdate      	    = null;
	protected LuaFunction m_luaOnDestroy 	            = null;
	protected LuaFunction m_luaEnable		            = null;
	protected LuaFunction m_luaDisable 		            = null;
	protected LuaFunction m_luaAnimationAttackStart     = null;
    protected LuaFunction m_luaAnimationAttackEnd       = null;
    protected LuaFunction m_luaAnimationSound           = null;
	protected LuaFunction m_luaAnimationEffect          = null;
	protected LuaFunction m_luaAnimationEvent           = null;
	protected LuaFunction m_luaAnimationForwadStart     = null;
	protected LuaFunction m_luaAnimationForwadEnd       = null;
    protected LuaFunction m_luaAnimationDispatchEvent   = null;
    protected LuaFunction m_luaAnimatorMove             = null;
    protected LuaFunction m_luaAnimatorBodyBox          = null;
    protected LuaFunction m_luaAnimatorStateEnter       = null;
    protected LuaFunction m_luaAnimatorStateExit        = null;
    protected LuaFunction m_luaAnimationJumpStart       = null;
    protected LuaFunction m_luaAnimationJumpEnd         = null;
    protected LuaFunction m_luaAnimationBackwardStart   = null;
    protected LuaFunction m_luaAnimationBackwardEnd     = null;
    

    /// <summary>
    /// The m d lua func.
    /// </summary>
    protected Dictionary<string, LuaFunction> 
		m_dLuaFunc = new Dictionary<string, LuaFunction>();

    /// <summary>
    /// 
    /// </summary>
    private List<AnimationEvent> 
        m_frameAnimationEvents = new List<AnimationEvent>();

    /// <summary>
    /// 
    /// </summary>
    private Dictionary<string, int> m_animationEventsWeight = new Dictionary<string, int>()
    {
        {"OnAnimationAttackWarningEnd", 0},
        {"OnAnimationAttack", 0},
    };

    /// <summary>
    /// Gets the lua module.
    /// </summary>
    /// <value>The lua module.</value>
    public LuaTable 		LuaModule
	{ get; private set; }

	/// <summary>
	/// Gets the lua component.
	/// </summary>
	/// <returns>The lua component.</returns>
	/// <param name="go">Go.</param>
	public static LuaTable 	GetLuaBehaviourScript(GameObject go)
	{
		var luaCom = go.GetComponent<XLuaBehaviourScript> ();
		if (null == luaCom) {
			return null;
		}

		return luaCom.LuaModule;
	}

	/// <summary>
	/// Adds the lua component.
	/// </summary>
	/// <returns>The lua component.</returns>
	/// <param name="go">Go.</param>
	/// <param name="luaTbl">Lua tbl.</param>
	public static LuaTable 	AddLuaBehaviourScript(GameObject go, LuaTable luaTbl)
	{
		var luaComp = go.AddComponent<XLuaBehaviourScript>();
		luaComp.Initilize(luaTbl);

		return luaComp.LuaModule;
	}

	/// <summary>
	/// Initilize the specified luaTbl.
	/// </summary>
	/// <param name="luaTbl">Lua tbl.</param>
	private void 			Initilize(LuaTable luaTbl)
	{
		object chunk = luaTbl;
		if (chunk != null && chunk is LuaTable) {
			LuaModule = (LuaTable)chunk;

			LuaModule ["this"] 			= this;
			LuaModule ["transform"] 	= transform;
			LuaModule ["gameObject"] 	= gameObject;

			// lua call unity behaviour function
			m_luaAwake 			        = LuaModule ["Awake"] as LuaFunction;
			m_luaStart 			        = LuaModule ["Start"] as LuaFunction;
			m_luaUpdate 		        = LuaModule ["Update"] as LuaFunction;
			m_luaLateUpdate 	        = LuaModule ["LateUpdate"] as LuaFunction;
			m_luaFixedUpdate	        = LuaModule ["FixedUpdate"] as LuaFunction;
			m_luaOnDestroy 		        = LuaModule ["OnDestroy"] as LuaFunction;
			m_luaEnable 		        = LuaModule ["OnEnable"] as LuaFunction;
			m_luaDisable 		        = LuaModule ["OnDisable"] as LuaFunction;

            m_luaAnimationAttackStart   = LuaModule ["OnAnimationAttackStart"] as LuaFunction;
            m_luaAnimationAttackEnd     = LuaModule ["OnAnimationAttackEnd"] as LuaFunction;
            m_luaAnimationForwadStart   = LuaModule ["OnAnimationForwardStart"] as LuaFunction;
            m_luaAnimationForwadEnd     = LuaModule ["OnAnimationForwardEnd"] as LuaFunction;
            m_luaAnimationSound         = LuaModule ["OnAnimationSound"] as LuaFunction;
            m_luaAnimationEffect        = LuaModule ["OnAnimationEffect"] as LuaFunction;
            m_luaAnimationEvent         = LuaModule ["OnAnimationEvent"] as LuaFunction;
            m_luaAnimatorBodyBox        = LuaModule ["OnAnimationBodyBox"] as LuaFunction;
            m_luaAnimationDispatchEvent = LuaModule ["OnAnimationDispatchEvent"] as LuaFunction;
            m_luaAnimationJumpStart     = LuaModule["OnAnimationJumpStart"] as LuaFunction;
            m_luaAnimationJumpEnd       = LuaModule["OnAnimationJumpEnd"] as LuaFunction;

            m_luaAnimatorMove	        = LuaModule ["OnAnimatorMove"] as LuaFunction;
            m_luaAnimatorStateEnter     = LuaModule ["OnAnimatorStateEnter"] as LuaFunction;
            m_luaAnimatorStateExit      = LuaModule ["OnAnimatorStateExit"] as LuaFunction;
        }

		if (null == LuaModule) {
			Debug.LogError ("LuaComponent.Initilize: initilize with nil lua table");
		} else {
			if (m_luaAwake != null)
				m_luaAwake.call (LuaModule);
		}
	}

	/// <summary>
	/// Gets the lua function.
	/// </summary>
	/// <returns>The lua function.</returns>
	/// <param name="funcName">Func name.</param>
	private LuaFunction 	GetLuaFunction(string funcName)
	{
		if (LuaModule == null)
			return null;

		LuaFunction func = null;
		m_dLuaFunc.TryGetValue (funcName, out func);
		if (func == null) {
			func = LuaModule [funcName] as LuaFunction;
			m_dLuaFunc.Add (funcName, func);
		}

		return func;
	}

	[DoNotToLua]
	public object 			CallLuaFunction(string funcName)
	{
		LuaFunction func = GetLuaFunction (funcName);

		if (func != null)
			return func.call (LuaModule);

		if (func == null)
			Debug.LogError ("do not find lua function:" + funcName);

		return null;
	}

	[DoNotToLua]
	public object 			CallLuaFunction(string funcName, object obj)
	{
		LuaFunction func = GetLuaFunction (funcName);

		if (func != null)
			return func.call (LuaModule, obj);

		return null;
	}

	[DoNotToLua]
	public object 			GetProperty(string propertyName)
	{
		if (LuaModule == null)
			return null;

		return LuaModule [propertyName];
	}

	[DoNotToLua]
	public void 			SetProperty(string propertyName, object value)
	{
		if (LuaModule != null)
			LuaModule [propertyName] = value;
	}

    public AnimationEvent[] GetFrameEvents()
    {
        if (m_frameAnimationEvents.Count > 0)
        {
            m_frameAnimationEvents.Sort(delegate (AnimationEvent x, AnimationEvent y)
            {
                int xWeight = m_animationEventsWeight.ContainsKey(x.functionName) ? m_animationEventsWeight[x.functionName] : int.MaxValue;
                int yWeight = m_animationEventsWeight.ContainsKey(y.functionName) ? m_animationEventsWeight[y.functionName] : int.MaxValue;

                return xWeight - yWeight;
            });

            AnimationEvent[] events = m_frameAnimationEvents.ToArray();
            m_frameAnimationEvents.Clear();

            return events;
        }

        return null;
    }

    /// <summary>
    /// Start this instance.
    /// </summary>
    IEnumerator Start()
	{
		if (m_luaStart != null) {
			object ret = m_luaStart.call (LuaModule);
			if (ret is IEnumerator)
				yield return StartCoroutine ((IEnumerator)ret);
			else
				yield return ret;
		}
	}

	// MonoBehaviour callback
	void Update()
	{
		if( m_luaUpdate != null )
			m_luaUpdate.call(LuaModule);
	}

	// MonoBehaviour callback
	void LateUpdate()
	{
		if( m_luaLateUpdate != null )
			m_luaLateUpdate.call(LuaModule);
	}

	// MonoBehaviour callback
	void FixedUpdate()
	{
		if( m_luaFixedUpdate != null )
			m_luaFixedUpdate.call(LuaModule);
	}

	// MonoBehaviour callback
	void OnDestroy()
	{
		if( m_luaOnDestroy != null )
			m_luaOnDestroy.call(LuaModule);
		
		m_dLuaFunc.Clear();

		if( LuaModule != null )
			LuaModule.Dispose();
	}

	void OnEnable()
	{
		if (m_luaEnable != null)
			m_luaEnable.call (LuaModule);
	}

	void OnDisable()
	{
		if (m_luaDisable != null)
			m_luaDisable.call (LuaModule);	
	}

	void OnAnimatorMove()
	{
		if (m_luaAnimatorMove != null)
			m_luaAnimatorMove.call (LuaModule);
	}

	void OnAnimationEvent(AnimationEvent evt)
	{
		#if UNITY_EDITOR
		GLog.Log(string.Format("<color=orange> OnAnimationEvent func={0} int={1} float={2} string={3} </color>", 
			evt.functionName, evt.intParameter, evt.floatParameter, evt.stringParameter));
		#endif

        if (m_luaAnimationDispatchEvent != null)
        {
            m_frameAnimationEvents.Add(evt);
        }
        else
        {
            if (m_luaAnimationEvent != null)
                m_luaAnimationEvent.call(LuaModule, evt);
        }
	}

	void OnAnimationAttackStart(AnimationEvent evt)
	{
#if UNITY_EDITOR
        GLog.Log(string.Format("<color=orange> OnAnimationAttackStart func={0} int={1} float={2} string={3} </color>", 
			evt.functionName, evt.intParameter, evt.floatParameter, evt.stringParameter));
#endif
        if (m_luaAnimationDispatchEvent != null)
        {
            m_frameAnimationEvents.Add(evt);
        }
        else
        {
            if (m_luaAnimationAttackStart != null)
                m_luaAnimationAttackStart.call(LuaModule, evt);
        }
	}

    void OnAnimationAttackEnd(AnimationEvent evt)
    {
#if UNITY_EDITOR
        GLog.Log(string.Format("<color=orange> OnAnimationAttackEnd func={0} int={1} float={2} string={3} </color>",
            evt.functionName, evt.intParameter, evt.floatParameter, evt.stringParameter));
#endif
        if (m_luaAnimationDispatchEvent != null)
        {
            m_frameAnimationEvents.Add(evt);
        }
        else
        {
            if (m_luaAnimationAttackEnd != null)
                m_luaAnimationAttackEnd.call(LuaModule, evt);
        }
    }


    void OnAnimationForwardStart(AnimationEvent evt)
	{
#if UNITY_EDITOR
        GLog.Log(string.Format("<color=orange> OnAnimationForwardStart func={0} int={1} float={2} string={3} </color>", 
			evt.functionName, evt.intParameter, evt.floatParameter, evt.stringParameter));
#endif

        if (m_luaAnimationDispatchEvent != null)
        {
            m_frameAnimationEvents.Add(evt);
        }
        else
        {
            if (m_luaAnimationForwadStart != null)
                m_luaAnimationForwadStart.call(LuaModule, evt);
        }	
	}

	void OnAnimationForwardEnd(AnimationEvent evt)
	{
#if UNITY_EDITOR
        GLog.Log(string.Format("<color=orange> OnAnimationForwardEnd func={0} int={1} float={2} string={3} </color>", 
			evt.functionName, evt.intParameter, evt.floatParameter, evt.stringParameter));
#endif

        if (m_luaAnimationDispatchEvent != null)
        {
            m_frameAnimationEvents.Add(evt);
        }
        else
        {
            if (m_luaAnimationForwadEnd != null)
                m_luaAnimationForwadEnd.call(LuaModule, evt);
        }	
	}

	void OnAnimationSound(AnimationEvent evt)
	{
#if UNITY_EDITOR
        GLog.Log(string.Format("<color=orange> OnAnimationSound func={0} int={1} float={2} string={3} </color>", 
			evt.functionName, evt.intParameter, evt.floatParameter, evt.stringParameter));
#endif

        if (m_luaAnimationDispatchEvent != null)
        {
            m_frameAnimationEvents.Add(evt);
        }
        else
        {
            if (m_luaAnimationSound != null)
                m_luaAnimationSound.call(LuaModule, evt);
        }
	}

	void OnAnimationEffect(AnimationEvent evt)
	{
#if UNITY_EDITOR
        GLog.Log(string.Format("<color=orange> OnAnimationEffect func={0} int={1} float={2} string={3} </color>", 
			evt.functionName, evt.intParameter, evt.floatParameter, evt.stringParameter));
#endif

        if (m_luaAnimationDispatchEvent != null)
        {
            m_frameAnimationEvents.Add(evt);
        }
        else
        {
            if (m_luaAnimationEffect != null)
                m_luaAnimationEffect.call(LuaModule, evt);
        }	
	}

    void OnAnimationBodyBox(AnimationEvent evt)
    {
#if UNITY_EDITOR
        GLog.Log(string.Format("<color=orange> OnAnimationBodyBox func={0} int={1} float={2} string={3} </color>",
            evt.functionName, evt.intParameter, evt.floatParameter, evt.stringParameter));
#endif
        if (m_luaAnimationDispatchEvent != null)
        {
            m_frameAnimationEvents.Add(evt);
        }
        else
        {
            if (m_luaAnimatorBodyBox != null)
                m_luaAnimatorBodyBox.call(LuaModule, evt, evt.objectReferenceParameter as XBodyBoxConfigObject);
        }
    }

    void OnAnimationJumpStart(AnimationEvent evt)
    {
#if UNITY_EDITOR
        GLog.Log(string.Format("<color=orange> OnAnimationJumpStart func={0} int={1} float={2} string={3} </color>",
            evt.functionName, evt.intParameter, evt.floatParameter, evt.stringParameter));
#endif
        if (m_luaAnimationDispatchEvent != null)
        {
            m_frameAnimationEvents.Add(evt);
        }
        else
        {
            if (m_luaAnimationJumpStart != null)
                m_luaAnimationJumpStart.call(LuaModule, evt, evt.objectReferenceParameter as XBodyBoxConfigObject);
        }
    }

    void OnAnimationJumpEnd(AnimationEvent evt)
    {
#if UNITY_EDITOR
        GLog.Log(string.Format("<color=orange> OnAnimationJumpEnd func={0} int={1} float={2} string={3} </color>",
            evt.functionName, evt.intParameter, evt.floatParameter, evt.stringParameter));
#endif
        if (m_luaAnimationDispatchEvent != null)
        {
            m_frameAnimationEvents.Add(evt);
        }
        else
        {
            if (m_luaAnimationJumpEnd != null)
                m_luaAnimationJumpEnd.call(LuaModule, evt, evt.objectReferenceParameter as XBodyBoxConfigObject);
        }
    }

    void OnAnimationBackwardStart(AnimationEvent evt)
    {
#if UNITY_EDITOR
        GLog.Log(string.Format("<color=orange> OnAnimationBackwardStart func={0} int={1} float={2} string={3} </color>",
            evt.functionName, evt.intParameter, evt.floatParameter, evt.stringParameter));
#endif
        if (m_luaAnimationDispatchEvent != null)
        {
            m_frameAnimationEvents.Add(evt);
        }
        else
        {
            if (m_luaAnimationBackwardStart != null)
                m_luaAnimationBackwardStart.call(LuaModule, evt, evt.objectReferenceParameter as XBodyBoxConfigObject);
        }
    }

    void OnAnimationBackwardEnd(AnimationEvent evt)
    {
#if UNITY_EDITOR
        GLog.Log(string.Format("<color=orange> OnAnimationBackwardEnd func={0} int={1} float={2} string={3} </color>",
            evt.functionName, evt.intParameter, evt.floatParameter, evt.stringParameter));
#endif
        if (m_luaAnimationDispatchEvent != null)
        {
            m_frameAnimationEvents.Add(evt);
        }
        else
        {
            if (m_luaAnimationBackwardEnd != null)
                m_luaAnimationBackwardEnd.call(LuaModule, evt, evt.objectReferenceParameter as XBodyBoxConfigObject);
        }
    }

    public void OnAnimatorStateEnter(int state, int tag)
    {
#if UNITY_EDITOR
        GLog.Log(string.Format("<color=orange> OnAnimatorStateExit state=%d tag=%d </color>", state, tag));
#endif
        if (m_luaAnimatorStateEnter != null)
            m_luaAnimatorStateEnter.call(LuaModule, state, tag);
    }

    public void OnAnimatorStateExit(int state, int tag)
    {
#if UNITY_EDITOR
        GLog.Log(string.Format("<color=orange> OnAnimatorStateExit state=%d tag=%d </color>", state, tag));
#endif
        if (m_luaAnimatorStateExit != null)
            m_luaAnimatorStateExit.call(LuaModule, state, tag);
    }

    void OnApplicationFocus(bool isFocus)
    {

    }

    void OnApplicationPause(bool isPause)
    {

    }

    void OnTriggerEnter(Collider collider)
    {

    }

    void OnTriggerStay(Collider collider)
    {

    }

    void OnTriggerExit(Collider collider)
    {

    }
}