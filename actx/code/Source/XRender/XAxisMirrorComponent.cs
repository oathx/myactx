using UnityEngine;
using System.Collections;

public class XAxisMirrorComponent : MonoBehaviour
{
    public bool IsRotateX = true;
    public bool IsRotateY = false;

    [Header("Mirror Local")]
    public bool IsMirrorPositionX = false;
    public bool IsMirrorRotationX = false;
    public bool IsMirrorRotationY = false;
    public bool IsMirrorRotationZ = false;

    [Header("Mirror JUST scale x")]
    public bool IsJustMirrorScaleX = false;

    private Quaternion _initLocalRot;
    private Vector3 _initLocalPos;

    float _rotTransX = 0f;
    float _rotTransY = float.MinValue;

    bool _hasFlip = false;

    Transform _trans;

    void Awake()
    {
        _trans = transform;

        _initLocalRot = _trans.localRotation;
        _initLocalPos = _trans.localPosition;

        _rotTransX = _trans.rotation.eulerAngles.x;

        //        rotTransY = -trans.rotation.eulerAngles.y;
    }

    public void Apply(bool apply)
    {
        if (apply)
        {
            if (IsJustMirrorScaleX)
            {
                _trans.localScale = new Vector3(-1, 1, 1);
                return;
            }

            if (!_hasFlip)
                _rotTransY = -_trans.eulerAngles.y;
            _trans.rotation = Quaternion.Euler(new Vector3(IsRotateX ? -_rotTransX : _rotTransX, IsRotateY ? _rotTransY : _trans.eulerAngles.y, _trans.eulerAngles.z));

            if (!_hasFlip)
                _trans.localPosition = new Vector3(IsMirrorPositionX ? -_initLocalPos.x : _initLocalPos.x, _initLocalPos.y, _initLocalPos.z);

            if (!_hasFlip)
                _trans.localRotation = Quaternion.Euler(
                    new Vector3(IsMirrorRotationX ? -_trans.localEulerAngles.x : _trans.localEulerAngles.x,
                                IsMirrorRotationY ? -_trans.localEulerAngles.y : _trans.localEulerAngles.y,
                                IsMirrorRotationZ ? -_trans.localEulerAngles.z : _trans.localEulerAngles.z));
            _hasFlip = true;
        }
        else
        {
            _hasFlip = false;

            _trans.localRotation = _initLocalRot;
            _trans.localPosition = _initLocalPos;
        }
    }
}
