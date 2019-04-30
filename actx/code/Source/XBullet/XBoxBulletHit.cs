using UnityEngine;
using System.Collections;

public class XBoxBulletHit : MonoBehaviour
{
    public XBoxConfigObject HitBoxConfig;
    public XBoxConfigObject WarningBoxConfig;
    private XBoxRect _hitRectBox = new XBoxRect();
    private XBoxRect _warningRectBox = new XBoxRect();
    private Transform _trans;

    void Awake()
    {
        _trans = transform;
    }

    public XBoxRect GetFixedHitBox(int flip)
    {
        if (_trans == null)
            return null;
        _hitRectBox.MinX = Mathf.RoundToInt(_trans.position.x * XBoxComponent.FLOAT_CORRECTION) + HitBoxConfig.OffsetX * flip - HitBoxConfig.Width;
        _hitRectBox.MinY = Mathf.RoundToInt(_trans.position.y * XBoxComponent.FLOAT_CORRECTION) + HitBoxConfig.OffsetY;
        _hitRectBox.Width = HitBoxConfig.Width * 2;
        _hitRectBox.Height = HitBoxConfig.Height;

        return _hitRectBox;
    }

    public XBoxRect GetFixedWarningBox(int flip)
    {
        if (WarningBoxConfig == null || _trans == null)
            return null;
        _warningRectBox.MinX = Mathf.RoundToInt(_trans.position.x * XBoxComponent.FLOAT_CORRECTION) + WarningBoxConfig.OffsetX * flip - WarningBoxConfig.Width;
        _warningRectBox.MinY = Mathf.RoundToInt(_trans.position.y * XBoxComponent.FLOAT_CORRECTION) + WarningBoxConfig.OffsetY;
        _warningRectBox.Width = WarningBoxConfig.Width * 2;
        _warningRectBox.Height = WarningBoxConfig.Height;

        return _warningRectBox;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (HitBoxConfig == null)
            return;
        if (_trans == null)
            _trans = transform;
        Gizmos.color = Color.red;
        GetFixedHitBox(1);
        Gizmos.DrawWireCube(new Vector3((_hitRectBox.MinX + HitBoxConfig.Width) / XBoxComponent.FLOAT_CORRECTION, 
            (_hitRectBox.MinY + HitBoxConfig.Height / 2f) / XBoxComponent.FLOAT_CORRECTION, 0f), 
            new Vector3(HitBoxConfig.Width * 2 / XBoxComponent.FLOAT_CORRECTION, HitBoxConfig.Height / XBoxComponent.FLOAT_CORRECTION, 1.0f));

        Gizmos.color = Color.yellow;
        XBoxRect box = GetFixedWarningBox(1);
        if (box != null)
            Gizmos.DrawWireCube(new Vector3((_warningRectBox.MinX + WarningBoxConfig.Width) / XBoxComponent.FLOAT_CORRECTION,
                (_warningRectBox.MinY + WarningBoxConfig.Height / 2f) / XBoxComponent.FLOAT_CORRECTION, 0f), 
                new Vector3(WarningBoxConfig.Width * 2 / XBoxComponent.FLOAT_CORRECTION, WarningBoxConfig.Height / XBoxComponent.FLOAT_CORRECTION, 1.0f));
    }

#endif
}
