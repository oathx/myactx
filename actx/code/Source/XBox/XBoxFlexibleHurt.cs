using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SLua;

[CustomLuaClass]
public class XBoxFlexibleHurt : MonoBehaviour
{
    public XBoxConfigObject HurtBoxConfig;
    private XBoxRect _hurtRectBox = new XBoxRect();
    private Transform _trans;
    private XActiveType _activeType = XActiveType.None;
    private int _activeId;

    public Transform Trans
    {
        get { return _trans; }
    }

    void Awake()
    {
        _trans = transform;
        RegisterInBoxes();
    }

    void OnDestroy()
    {
        UnRegisterFromBoxes();
    }

    public XBoxRect GetFlexibleHurtBox()
    {
        if (_trans == null)
            return null;
        _hurtRectBox.MinX = Mathf.RoundToInt(_trans.position.x * XBoxComponent.FLOAT_CORRECTION) + HurtBoxConfig.OffsetX - HurtBoxConfig.Width;
        _hurtRectBox.MinY = Mathf.RoundToInt(_trans.position.y * XBoxComponent.FLOAT_CORRECTION) + HurtBoxConfig.OffsetY;
        _hurtRectBox.Width = HurtBoxConfig.Width * 2;
        _hurtRectBox.Height = HurtBoxConfig.Height;

        return _hurtRectBox;
    }

    public void RegisterInBoxes()
    {
        XBoxSystem.GetSingleton().RegisterFlexibleHurtBox(this);
    }

    public void UnRegisterFromBoxes()
    {
        XBoxSystem.GetSingleton().UnRegisterFlexibleHurtBox(this);
    }

    public void SetActiveTypeInLua(int activeType)
    {
        _activeType = (XActiveType)activeType;
    }

    public XActiveType GetActiveTypeInLua()
    {
        return _activeType;
    }

    public void SetActiveIdInLua(int activeId)
    {
        _activeId = activeId;
    }

    public int GetActiveId()
    {
        return _activeId;
    }

    public enum XActiveType
    {
        None = 0,
        Interactive,
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (HurtBoxConfig == null)
            return;

        if (_trans == null)
            _trans = transform;

        Gizmos.color = Color.green;
        GetFlexibleHurtBox();
        Gizmos.DrawWireCube(new Vector3((_hurtRectBox.MinX + HurtBoxConfig.Width) / XBoxComponent.FLOAT_CORRECTION, (_hurtRectBox.MinY + HurtBoxConfig.Height / 2f) / XBoxComponent.FLOAT_CORRECTION, 0f), 
            new Vector3(HurtBoxConfig.Width * 2 / XBoxComponent.FLOAT_CORRECTION, HurtBoxConfig.Height / XBoxComponent.FLOAT_CORRECTION, 1.0f));
    }
#endif
}
