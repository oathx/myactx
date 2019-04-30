using SLua;
using UnityEngine;

[CustomLuaClass]
[System.Serializable]
public class XMoveCurve : ScriptableObject
{
    public float endTime        = 0;
    public AnimationCurve curve = new AnimationCurve();
}
