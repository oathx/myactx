using UnityEngine;
using System.Collections;
using SLua;

[CustomLuaClass]
public class XEffectConfigObject : ScriptableObject
{
    public string[] effectFiles = new string[] { string.Empty };

    public XBoxComponent.BoneNogType linkBone = XBoxComponent.BoneNogType.Center;
    public XEffectComponent.EffectFollowType follow = XEffectComponent.EffectFollowType.Both;
    public XEffectComponent.InterruptType interrupt = XEffectComponent.InterruptType.Destory;
    public bool freezeByAnimation = true;
    public bool isIgnoreMorror = false;
    public bool isBoss = false;
    public Vector3 scaleParam = Vector3.one;
    public Vector3 offsetParam = Vector3.zero;
    public int maxSameEffectCount = 0;
    public bool isDecideByStar = false;
    public bool isDecideByEquipStar = false;

    public string getRes(int index)
    {
        return effectFiles[index];
    }

    void OnEnable()
    {
        if (maxSameEffectCount > 0)
        {
            for (int i = 0; i < effectFiles.Length; i++)
            {
                XEffectManager.Instance.SetMaxEffectCount(effectFiles[i], maxSameEffectCount);
            }
        }
    }
}
