using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(XAlive))]
public class XDebugAlive : MonoBehaviour {
#if UNITY_EDITOR
    public delegate void AliveEvent(Transform t, AnimationEvent e);

    /// <summary>
    /// 
    /// </summary>
    public AliveEvent _attackStartMethod;
    public AliveEvent _bodyBoxMethod;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    void OnAnimationAttackStart(AnimationEvent e)
    {
        Debug.LogError("OnAnimationAttackStart");
        if (_attackStartMethod != null)
        {
            _attackStartMethod.Invoke(this.transform, e);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    void OnAnimationBodyBox(AnimationEvent e)
    {
        if (_bodyBoxMethod != null)
        {
            _bodyBoxMethod.Invoke(this.transform, e);
        }
    }
#endif
}
