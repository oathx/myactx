using System.Collections.Generic;
using UnityEngine;
using System;
using SLua;

[CustomLuaClass]
public class XBoxComponent : XActorComponent
{
    protected XBoxRect _bodyBox;
    protected XBoxRect _attackBox;
    protected XBoxRect _attackWariningBox;
    protected List<XBoxRect> _receiveDamageBoxes;
    protected XBoxAttackWarningData _attackWarningData;
    protected XBoxRect _transBox;

    protected Collider _bodyCollider;
    protected Vector2 _colliderSize = new Vector2(0.5f, 2f);

    protected Transform _trans;
    protected bool _isSquat = false;
    
    protected XBodyBoxConfigObject _bodyBoxConf;
    protected XBodyBoxConfigObject _lastBodyBoxConf;
    protected XBoxHugConfigObject _hugConfigConf;
    protected XBoxHugRect _hugBox = new XBoxHugRect();

    protected Vector3 _nextPos = new Vector3(1000f, 0, 0);
    protected Vector3 _lastPos = new Vector3(1000f, 0, 0);
    protected bool _isBound;
    protected int _lastTransPosX = 1000;
    protected int _lastTransPosY = 0;

    protected Vector3 _nextTransPos = new Vector3(1000f, 0, 0);
    protected Vector3 _lastTransPos = new Vector3(1000f, 0, 0);

    protected XBodyMoveRectBox _transMoveBox;
    protected XBodyMoveRectBox _bodyMoveBox;

    protected XBoxAttackCheckPointObject _attackCheckData;
    protected System.Action<int[], Vector3[]> _attackCallback;
    protected int _lastPosX = 1000;
    protected int _lastPosY = 0;
    protected bool _isPlayer = false;
    protected bool _isVisible = true;
    protected float _groundHeight = .0f;

    protected XCameraFight _camera;
    protected XSmoothAnimator _smooth;
    protected AnimationClip _camSkillReaction;
    protected float _animatorSpeed = 1.0f;
    protected Vector3 _lastPosition;

    protected int _freezeFrame = 0;
    protected int _shakeFrame = 0;
    protected bool _freezeSpecialCam = false;
    protected int _freezeSpecialCamFrame = 0;
    protected int _freezeDuration = 0;
    protected bool _shakePosition = false;
    protected bool _shakeOffset = false;
    protected Vector3 _originalPosition = Vector3.zero;
    protected float _shakeDelta = 0.03f;
    protected bool _isShakeY = false;
    protected float _originalCamera = 0;

    protected int _currentAnimatorState = 0;
    Dictionary<int, List<XAudioComponent>> 
        _animatorFrameSounds = new Dictionary<int, List<XAudioComponent>>();

    public bool _isShowBodyBox = false;
    public XBodyBoxConfigDataTemple<float> _bodyBoxShow;
    public XBoxAttackData _boxAttackData;

    protected string _dustEffect;
    protected float moveJudge = 0.02f;

    public const float FLOAT_CORRECTION = 100f;
    public const float EPS_CORRECTION = 0.01f;
    public const int ADDTIVE_BOX_CORRECTION = 10;
    public const int FRAME_DELTA_TIME = 33;

    public bool IsDrawBox = false;
    public Action<int, Vector3> BodyMeeting;
    public Action<bool> ReceiveAttackWarning;
    public Action<int, int, int> HugOther;
    public Action<int, Vector3> TransMeeting;

    public enum XBoxFlag
    {
        BodyBox = 1,
        HurtBox = 2,
        AttackBox = 4,
    }
    protected byte _boxesFlag = 0xff;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        XBoxAlive aliveData = this.GetComponent<XBoxAlive>();
        if (aliveData)
        {
            _camSkillReaction = aliveData.camSkillReaction;
            _linkConfig = aliveData.LinkConfig;
            _footstepFallAudio = aliveData.footstepFallAudio;
            _squatHeight = aliveData.SquatHeight;
            _nogPoint = aliveData.nogPoint;
        }

        _bodyBoxConf = ScriptableObject.CreateInstance<XBodyBoxConfigObject>();
        if (_bodyBoxConf)
        {
            _bodyBoxConf.BodyBoxWidth = 25;
            _bodyBoxConf.BodyBoxHeight = 120;
        }

        _bodyBox = new XBoxRect();
        _receiveDamageBoxes = new List<XBoxRect>();
        _attackBox = new XBoxRect();
        _attackWariningBox = new XBoxRect();
        _transBox = new XBoxRect();

        _attackWarningData = new XBoxAttackWarningData();

        _animator = GetComponent<Animator>();
        if (_animator != null)
        {
            AnimatorOverrideController overrideClone = UnityEngine.Object.Instantiate(_animator.runtimeAnimatorController) as AnimatorOverrideController;
            _animator.runtimeAnimatorController = overrideClone;

            _animatorSpeed = _animator.speed;
            _animator.Rebind();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        _lastPosition = transform.localPosition;
        _bodyCollider = GetComponent<Collider>();
        _freezeFrame = 0;
        _freezeDuration = 0;
        _trans = transform;
        _smooth = GetComponent<XSmoothAnimator>();
    }
    
    public AnimationClip CamSkillReaction
    {
        get { return _camSkillReaction; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="layer"></param>
    public void SetLayer(int layer)
    {
        foreach (Transform trans in GetComponentsInChildren<Transform>())
        {
            trans.gameObject.layer = layer;
        }
    }

    public void EnableBoxes(XBoxFlag flag)
    {
        _boxesFlag |= (byte)flag;
    }

    public void DisableBoxes(XBoxFlag flag)
    {
        if (GetBoxesFlag(flag))
            _boxesFlag -= (byte)flag;
    }

    [DoNotToLua]
    public bool GetBoxesFlag(XBoxFlag flag)
    {
        return (_boxesFlag & (byte)flag) > 0;
    }

    public Vector3 LastPos
    {
        get
        {
            return _lastPos;
        }
    }


    [DoNotToLua]
    public bool IsBound
    {
        get { return _isBound; }
    }

    public Vector3 LastTransPos
    {
        get
        {
            return _lastTransPos;
        }
    }

    public Transform Trans
    {
        get
        {
            return _trans;
        }
    }

    public Rect GetBoundingRect()
    {
        return GetBoundingRect(_trans, _bodyCollider, _isSquat, _squatHeight);
    }

    public Rect GetBoundingRect(Transform trans, Collider col)
    {
        return GetBoundingRect(trans, col, false, 0);
    }

    private Rect GetBoundingRect(Transform trans, Collider col, bool isSquat, float squatHeight)
    {
        if (!trans || !col)
        {
            return default(Rect);
        }

        float sizeX = Math.Max(col.bounds.size.x * 0.5f, _colliderSize.x);
        float sizeY = isSquat ? squatHeight : Math.Max(col.bounds.size.y, _colliderSize.y);

        float rectX = 0;

        if (_nogPoint.center.lossyScale.x > 0)
            rectX = (trans.position.x - sizeX) * FLOAT_CORRECTION - EPS_CORRECTION;
        else
            rectX = (trans.position.x - sizeX) * FLOAT_CORRECTION + EPS_CORRECTION;

        Rect rect = new Rect(rectX, (Mathf.Max(0, col.bounds.min.y)) * FLOAT_CORRECTION, 
            sizeX * 2f * FLOAT_CORRECTION + EPS_CORRECTION, sizeY * FLOAT_CORRECTION);

        return rect;
    }

    private Vector3 GetFixedPosition()
    {
        if (!_trans)
        {
            _trans = transform;
        }

        if (IsPvp())
            return new Vector3(Mathf.RoundToInt(_nextPos.x * FLOAT_CORRECTION), Mathf.RoundToInt(_nextPos.y * FLOAT_CORRECTION), 0);
        else
            return new Vector3(Mathf.RoundToInt(_trans.position.x * FLOAT_CORRECTION), Mathf.RoundToInt(_trans.position.y * FLOAT_CORRECTION), 0);
    }

    private Vector3 GetNowPosition()
    {
        if (IsPvp())
            return new Vector3(Mathf.RoundToInt(_lastPos.x * FLOAT_CORRECTION * ADDTIVE_BOX_CORRECTION), Mathf.RoundToInt(_lastPos.y * FLOAT_CORRECTION * ADDTIVE_BOX_CORRECTION), 0);
        else
            return Vector3.zero;
    }

    private Vector3 GetFixedNextPosition()
    {
        if (IsPvp())
            return new Vector3(Mathf.RoundToInt(_nextPos.x * FLOAT_CORRECTION * ADDTIVE_BOX_CORRECTION), Mathf.RoundToInt(_nextPos.y * FLOAT_CORRECTION * ADDTIVE_BOX_CORRECTION), 0);
        else
            return Vector3.zero;
    }

    private Vector3 GetFixedAttackPosition()
    {
        if (_attackCheckData.IsAttackBoxFollowBone)
        {
            Transform trans = this.GetComponent<Animator>().GetBoneTransform(_attackCheckData.bone);
            return new Vector3(Mathf.RoundToInt(trans.position.x * FLOAT_CORRECTION), Mathf.RoundToInt(trans.position.y * FLOAT_CORRECTION), 0);
        }
        else
            return GetFixedPosition();
    }

    public List<XBoxRect> GetReceiveDamageBoxesRect()
    {
        try
        {
            if (_bodyBoxConf != null && _bodyBoxConf.ReceiveDamageBoxes.Count > 0 && GetBoxesFlag(XBoxFlag.HurtBox))
            {
                if (_nogPoint == null || (_nogPoint != null && _nogPoint.center == null))
                {
                    _receiveDamageBoxes.Clear();
                    return _receiveDamageBoxes;
                }
                int isFlip = 1;
                if (_nogPoint.center.lossyScale.x < 0)
                    isFlip = -1;

                Vector3 fixedPos = GetFixedPosition();

                if (_bodyBoxConf.ReceiveDamageBoxes.Count < _receiveDamageBoxes.Count)
                {
                    for (int i = _receiveDamageBoxes.Count - 1; i > _bodyBoxConf.ReceiveDamageBoxes.Count - 1; i--)
                    {
                        _receiveDamageBoxes.RemoveAt(i);
                    }
                }

                for (int i = 0; i < _bodyBoxConf.ReceiveDamageBoxes.Count; i++)
                {
                    XBodyBoxConfigObject.XBodyReceiveDamageBox dmgBox = _bodyBoxConf.ReceiveDamageBoxes[i];

                    if (_receiveDamageBoxes.Count - 1 < i)
                    {
                        XBoxRect box = new XBoxRect();
                        _receiveDamageBoxes.Add(box);
                    }

                    _receiveDamageBoxes[i].MinX = Mathf.RoundToInt(fixedPos.x + dmgBox.BoxOffsetX * isFlip - dmgBox.BoxWidth);
                    _receiveDamageBoxes[i].MinY = Mathf.RoundToInt(fixedPos.y + dmgBox.BoxOffsetY);
                    _receiveDamageBoxes[i].Width = dmgBox.BoxWidth * 2;
                    _receiveDamageBoxes[i].Height = dmgBox.BoxHeight;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        return _receiveDamageBoxes;
    }

    [DoNotToLua]
    public void ForceBodyMoveCallback(int forceX, XBoxComponent opponent)
    {
        if (!IsPvp())
            return;

        _nextPos = new Vector3(_lastPosX / 1000f, _nextPos.y, _nextPos.z);

        forceX = GetBoxesFlag(XBoxFlag.BodyBox) ? forceX : -1;

        if (BodyMeeting != null)
            BodyMeeting(forceX, _nextPos);
    }

    [DoNotToLua]
    public void LastBodyMove(int forceX, XBoxComponent opponent)
    {
        if (!IsPvp())
            return;

        int fixedForceX = 0;
        float boxBackParam = 0.51f;

        if (opponent != null)
        {
            if (forceX >= 0 && !_isBound)
            {
                fixedForceX = forceX;
                if (_lastPos.x < opponent.LastPos.x)
                {
                    fixedForceX = -forceX;
                }
                else if (_lastPos.x == opponent.LastPos.x && _nogPoint.center.localScale.x > 0)
                {
                    fixedForceX = -forceX;
                }
            }


            if (forceX >= 0 && opponent.IsBound)
            {
                boxBackParam = 1.01f;

                if (_isBound)
                {
                    if (_lastPos.x > 0 && _nogPoint.center.localScale.x < 0)
                        fixedForceX = -forceX;
                    else if (_lastPos.x < 0 && _nogPoint.center.localScale.x > 0)
                        fixedForceX = forceX;
                }
            }

            // precision limit, when intersect length is just 0.005, ignore it.
            if (forceX < 5)
                boxBackParam = 0f;

            _lastPosX = _lastPosX + (int)(fixedForceX * boxBackParam);
        }
    }

    private void GenBodyMoveBox(Vector3 now, Vector3 next)
    {
        if (_bodyMoveBox == null)
            _bodyMoveBox = new XBodyMoveRectBox();

        Vector3 move = (next - now) * FLOAT_CORRECTION * ADDTIVE_BOX_CORRECTION;

        _bodyMoveBox.MoveX = Mathf.RoundToInt(move.x);
        _bodyMoveBox.MoveY = Mathf.RoundToInt(move.y);
    }

    [DoNotToLua]
    private XBoxRect GetTransBoxRect(System.Func<Vector3> getPos)
    {
        if (_bodyBoxConf != null && GetBoxesFlag(XBoxFlag.BodyBox))
        {
            if (_nogPoint == null || (_nogPoint != null && _nogPoint.center == null))
            {
                return null;
            }
            int isFlip = 1;
            if (_nogPoint.center.lossyScale.x < 0)
                isFlip = -1;

            Vector3 fixedPos = getPos();

            _lastTransPosX = Mathf.RoundToInt(fixedPos.x);
            _lastTransPosY = Mathf.RoundToInt(fixedPos.y);

            _transBox.MinX = _lastTransPosX + _bodyBoxConf.BodyBoxOffsetX * ADDTIVE_BOX_CORRECTION * isFlip - _bodyBoxConf.BodyBoxWidth * ADDTIVE_BOX_CORRECTION;
            _transBox.MinY = _lastTransPosY + _bodyBoxConf.BodyBoxOffsetY * ADDTIVE_BOX_CORRECTION;
            _transBox.Width = _bodyBoxConf.BodyBoxWidth * 2 * ADDTIVE_BOX_CORRECTION;
            _transBox.Height = _bodyBoxConf.BodyBoxHeight * ADDTIVE_BOX_CORRECTION;

            return _transBox;
        }

        return null;
    }

    [DoNotToLua]
    public XBodyMoveRectBox GetTransMoveBox()
    {
        if (_transMoveBox == null)
            return GetNowTransMoveBoxRect();

        return _transMoveBox;
    }

    private Vector3 GetNowTransPosition()
    {
        if (IsPvp())
            return new Vector3(Mathf.RoundToInt(_lastTransPos.x * FLOAT_CORRECTION * ADDTIVE_BOX_CORRECTION), 
                Mathf.RoundToInt(_lastTransPos.y * FLOAT_CORRECTION * ADDTIVE_BOX_CORRECTION), 0);
        else
            return Vector3.zero;
    }

    [DoNotToLua]
    public XBodyMoveRectBox GetNowTransMoveBoxRect()
    {
        XBoxRect nowBox = GetTransBoxRect(GetNowTransPosition);
        if (nowBox == null)
            return null;

        if (_transMoveBox == null)
            _transMoveBox = new XBodyMoveRectBox();

        _transMoveBox.Copy(nowBox);
        return _transMoveBox;
    }

  
    [DoNotToLua]
    private XBoxRect GetBodyBoxRect(System.Func<Vector3> getPos)
    {
        if (_bodyBoxConf != null && GetBoxesFlag(XBoxFlag.BodyBox))
        {
            if (_nogPoint == null || (_nogPoint != null && _nogPoint.center == null))
            {
                return null;
            }
            int isFlip = 1;
            if (_nogPoint.center.lossyScale.x < 0)
                isFlip = -1;

            Vector3 fixedPos = getPos();

            _lastPosX = Mathf.RoundToInt(fixedPos.x);
            _lastPosY = Mathf.RoundToInt(fixedPos.y);

            _bodyBox.MinX = _lastPosX + _bodyBoxConf.BodyBoxOffsetX * ADDTIVE_BOX_CORRECTION * isFlip - _bodyBoxConf.BodyBoxWidth * ADDTIVE_BOX_CORRECTION;
            _bodyBox.MinY = _lastPosY + _bodyBoxConf.BodyBoxOffsetY * ADDTIVE_BOX_CORRECTION;
            _bodyBox.Width = _bodyBoxConf.BodyBoxWidth * 2 * ADDTIVE_BOX_CORRECTION;
            _bodyBox.Height = _bodyBoxConf.BodyBoxHeight * ADDTIVE_BOX_CORRECTION;

            return _bodyBox;
        }

        return null;
    }

    [DoNotToLua]
    public XBodyMoveRectBox GetNowBodyMoveBoxRect()
    {
        XBoxRect nowBox = GetBodyBoxRect(GetNowPosition);
        if (nowBox == null)
            return null;

        if (_bodyMoveBox == null)
            _bodyMoveBox = new XBodyMoveRectBox();

        _bodyMoveBox.Copy(nowBox);
        return _bodyMoveBox;
    }

    public void SetNextMovePos(Vector3 nextPos, bool isBound, bool isForceMove)
    {
        if (isForceMove || Mathf.Abs(nextPos.x - _nextPos.x) > 50f)
        {
            _lastPos = nextPos;
        }
        else
        {
            _lastPos = _nextPos;
        }

        _nextPos = nextPos;
        _isBound = isBound;

        GenBodyMoveBox(_lastPos, _nextPos);
        GetNowBodyMoveBoxRect();
    }

    [DoNotToLua]
    public void SetLastDeltaPos(int posX, int posY)
    {
        _lastPosX += posX;
        _lastPosY += posY;
    }

    [DoNotToLua]
    public void AdjustBodyMoveBox()
    {
        if (_bodyBoxConf == null)
            return;

        if (_nogPoint == null || (_nogPoint != null && _nogPoint.center == null))
        {
            return;
        }
        int isFlip = 1;
        if (_nogPoint.center.lossyScale.x < 0)
            isFlip = -1;

        _bodyMoveBox.MinX = _lastPosX + _bodyBoxConf.BodyBoxOffsetX * ADDTIVE_BOX_CORRECTION * isFlip - Mathf.Max(0, _bodyBoxConf.BodyBoxWidth) * ADDTIVE_BOX_CORRECTION;
        _bodyMoveBox.MinY = _lastPosY + _bodyBoxConf.BodyBoxOffsetY * ADDTIVE_BOX_CORRECTION;
    }

    [DoNotToLua]
    public XBodyMoveRectBox GetBodyMoveBox()
    {
        if (_bodyMoveBox == null)
            return GetNowBodyMoveBoxRect();

        return _bodyMoveBox;
    }

    [DoNotToLua]
    public XBoxRect GetAttackBoxRect()
    {
        if (_attackCheckData != null && GetBoxesFlag(XBoxFlag.AttackBox))
        {
            if (_nogPoint == null || (_nogPoint != null && _nogPoint.center == null))
            {
                return null;
            }
            int isFlip = 1;
            if (_nogPoint.center.lossyScale.x < 0)
                isFlip = -1;

            Vector3 fixedPos = GetFixedAttackPosition();

            _attackBox.MinX = Mathf.RoundToInt(fixedPos.x + _attackCheckData.AttackBoxOffsetX * isFlip - _attackCheckData.AttackBoxWidth);
            _attackBox.MinY = Mathf.RoundToInt(fixedPos.y + _attackCheckData.AttackBoxOffsetY);
            _attackBox.Width = _attackCheckData.AttackBoxWidth * 2;
            _attackBox.Height = _attackCheckData.AttackBoxHeight;

            return _attackBox;
        }

        return null;
    }

    private void GenTransMoveBox(Vector3 now, Vector3 next)
    {
        if (_transMoveBox == null)
            _transMoveBox = new XBodyMoveRectBox();

        Vector3 move = (next - now) * FLOAT_CORRECTION * ADDTIVE_BOX_CORRECTION;
        _transMoveBox.MoveX = Mathf.RoundToInt(move.x);
        _transMoveBox.MoveY = Mathf.RoundToInt(move.y);
    }

    public void SetNextTransMovePos(Vector3 nextPos, bool isBound, bool isForceMove)
    {
        if (isForceMove || Mathf.Abs(nextPos.x - _nextTransPos.x) > 50f)
        {
            _lastTransPos = nextPos;
        }
        else
        {
            _lastTransPos = _nextTransPos;
        }

        _nextTransPos = nextPos;

        GenTransMoveBox(_lastTransPos, _nextTransPos);
        GetNowTransMoveBoxRect();
    }

    [DoNotToLua]
    public void ForceTransMoveCallback(int forceX, XBoxComponent opponent)
    {
        if (!IsPvp())
            return;
        _nextTransPos = new Vector3(_lastTransPosX / 1000f, _nextTransPos.y, _nextTransPos.z);
        forceX = GetBoxesFlag(XBoxFlag.BodyBox) ? forceX : -1;

        if (TransMeeting != null)
            TransMeeting(forceX, _nextTransPos);
    }


    [DoNotToLua]
    public void LastTransMove(int forceX, XBoxComponent opponent)
    {
        if (!IsPvp())
            return;

        int fixedForceX = 0;
        float boxBackParam = 0.51f;

        if (opponent != null)
        {
            if (forceX >= 0 && !_isBound)
            {
                fixedForceX = forceX;
                if (_lastTransPos.x < opponent.LastTransPos.x)
                {
                    fixedForceX = -forceX;
                }
                else if (_lastTransPos.x == opponent.LastTransPos.x && _nogPoint.center.localScale.x > 0)
                {
                    fixedForceX = -forceX;
                }
            }


            if (forceX >= 0 && opponent.IsBound)
            {
                boxBackParam = 1.01f;

                if (_isBound)
                {
                    if (_lastTransPos.x > 0 && _nogPoint.center.localScale.x < 0)
                        fixedForceX = -forceX;
                    else if (_lastTransPos.x < 0 && _nogPoint.center.localScale.x > 0)
                        fixedForceX = forceX;
                }
            }

            if (forceX < 5)
                boxBackParam = 0f;

            _lastTransPosX = _lastTransPosX + (int)(fixedForceX * boxBackParam);
        }
    }

    [DoNotToLua]
    public void SetLastTransDeltaPos(int posX, int posY)
    {
        _lastTransPosX += posX;
        _lastTransPosY += posY;
    }

    [DoNotToLua]
    public void AdjustTransMoveBox()
    {
        if (_nogPoint == null || (_nogPoint != null && _nogPoint.center == null))
        {
            return;
        }
        int isFlip = 1;
        if (_nogPoint.center.lossyScale.x < 0)
            isFlip = -1;

        _transMoveBox.MinX = _lastTransPosX + _bodyBoxConf.BodyBoxOffsetX * ADDTIVE_BOX_CORRECTION * isFlip - Mathf.Max(0, _bodyBoxConf.BodyBoxWidth) * ADDTIVE_BOX_CORRECTION;
        _transMoveBox.MinY = _lastTransPosY + _bodyBoxConf.BodyBoxOffsetY * ADDTIVE_BOX_CORRECTION;
    }

    [DoNotToLua]
    public XBoxRect GetAttackWarningBoxRect()
    {
        if (_attackWarningData != null && _attackWarningData.Valid() && IsGameObjectExist())
        {
            if (_nogPoint == null || (_nogPoint != null && _nogPoint.center == null))
            {
                return null;
            }
            int isFlip = 1;
            if (_nogPoint.center.lossyScale.x < 0)
                isFlip = -1;

            Vector3 fixedPos = GetFixedPosition();

            XBoxConfigObject warningBox = _attackWarningData.Box;
            
            _attackWariningBox.MinX = Mathf.RoundToInt(fixedPos.x + warningBox.OffsetX * isFlip - warningBox.Width);
            _attackWariningBox.MinY = Mathf.RoundToInt(fixedPos.y + warningBox.OffsetY);
            _attackWariningBox.Width = warningBox.Width * 2;
            _attackWariningBox.Height = warningBox.Height;

            return _attackWariningBox;
        }
        return null;
    }

    private bool IsGameObjectExist()
    {
        return _nogPoint != null && _nogPoint.center != null;
    }

    public bool IsColliderHitDamageBoxes(Vector3 pos, BoxCollider col)
    {
        if (col == null)
            return false;

        Vector3 fixedPos = new Vector3(Mathf.RoundToInt(pos.x * FLOAT_CORRECTION), Mathf.RoundToInt(pos.y * FLOAT_CORRECTION), 0);
        XBoxRect b = new XBoxRect();
        b.MinX = Mathf.RoundToInt(fixedPos.x - Mathf.RoundToInt(col.size.x * 0.51f * FLOAT_CORRECTION) + Mathf.RoundToInt(col.center.x * FLOAT_CORRECTION));
        b.MinY = Mathf.RoundToInt(fixedPos.y + Mathf.RoundToInt(col.center.y * FLOAT_CORRECTION));
        b.Width = Mathf.RoundToInt(col.size.x * FLOAT_CORRECTION);
        b.Height = Mathf.RoundToInt(col.size.y * FLOAT_CORRECTION);

        List<XBoxRect> damageBoxes = GetReceiveDamageBoxesRect();
        if (damageBoxes != null && damageBoxes.Count > 0)
        {
            for (int i = 0; i < damageBoxes.Count; i++)
            {
                XBoxRect overlap = XBoxRect.Overlap(b, damageBoxes[i]);
                if (overlap.Width > 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    [DoNotToLua]
    public XBoxHugRect GetHugBoxRect()
    {
        if (_hugConfigConf != null)
        {
            if (_nogPoint == null || (_nogPoint != null && _nogPoint.center == null))
            {
                return null;
            }
            int isFlip = 1;
            if (_nogPoint.center.lossyScale.x < 0)
                isFlip = -1;

            Vector3 fixedPos = GetFixedPosition();

            _hugBox.MinX = Mathf.RoundToInt(fixedPos.x + _hugConfigConf.OffsetX * isFlip - _hugConfigConf.Width);
            _hugBox.MinY = Mathf.RoundToInt(fixedPos.y + _hugConfigConf.OffsetY);
            _hugBox.Width = _hugConfigConf.Width * 2;
            _hugBox.Height = _hugConfigConf.Height;
            _hugBox.HugType = _hugConfigConf.HugType;
            return _hugBox;
        }

        return null;
    }

    [DoNotToLua]
    public XBoxRect GetNextBodyBoxRect()
    {
        return GetBodyBoxRect(GetFixedNextPosition);
    }

    void HugEnd()
    {
        _hugConfigConf = null;
    }

    public void HugCallback(int eventType, XBoxComponent source, XBoxComponent receiver)
    {
        if (eventType == 0)
        {
            HugEnd();
            return;
        }

        if (HugOther != null)
            HugOther(eventType, (int)_hugConfigConf.HugType, receiver == null ? -1 : (int)receiver.cid);
    }

    private void AttackStart(XBoxAttackCheckPointObject checkData, System.Action<int[], Vector3[]> callback)
    {
        _attackCheckData = checkData;
        _attackCallback = callback;

        XBoxSystem.GetSingleton().CheckAttackWhenBoxesCome(this);
    }

    public override AnimatorStateInfo GetCurrentAnimatorState()
    {
        Animator anim = _animator;
        if (_smooth != null)
        {
            anim = _smooth.GetEventAnimator();
        }

        if (anim != null)
        {
            if (anim.IsInTransition(0))
                return anim.GetNextAnimatorStateInfo(0);

            return anim.GetCurrentAnimatorStateInfo(0);
        }

        return default(AnimatorStateInfo);
    }

#if UNITY_EDITOR || UNITY_STANDALONE
    void SetAttackRecoverFrame(float curTime)
    {
        if (!_isPlayer || _attackStartNormalTime <= .0f)
            return;

        AnimatorStateInfo stateInfo = GetCurrentAnimatorState();

        if (curTime == .0f)
            curTime = stateInfo.normalizedTime * stateInfo.length;

        float endTime = stateInfo.length;

        _selfAttackRecoverFrame = Mathf.RoundToInt((endTime - curTime) * 30);
        _attackStartNormalTime = 0;
    }

    void OnAnimationAttackStart(AnimationEvent e)
    {
        _attackStartNormalTime = e.time;
    }


    void OnAnimationAttackEnd(AnimationEvent e)
    {
        SetAttackRecoverFrame(e.time);
    }
#endif

    public void AttackWarningEventCallback(bool enter)
    {
        if (ReceiveAttackWarning != null)
            ReceiveAttackWarning(enter);
    }

    public void AnimationAttackWarning(AnimationEvent e)
    {
        _attackWarningData.CreateState = e.animatorStateInfo.fullPathHash;
        _attackWarningData.Box = e.objectReferenceParameter as XBoxConfigObject;
    }

    public void AttackWarningEnd()
    {
        _attackWarningData.Clear();
    }

    public Transform GetAttackCheckPointTransform(XBoxAttackCheckPointObject checkData)
    {
        Transform bone = _nogPoint.center;
        if (_checkPoint == null)
            _checkPoint = new GameObject("CheckPoint");

        Transform point = _checkPoint.transform;
        point.SetParent(bone, false);
        point.localRotation = checkData.rotation;
        point.localPosition = checkData.position;

        return point;
    }

    public bool IsHitEventHits(XBoxAttackCheckPointObject checkData, GameObject target, System.Action<int[], Vector3[]> callback)
    {
        AttackStart(checkData, callback);

        return false;
    }

    public void AttackEventCallback(int[] receiveHeroes, XBoxRect[] boxes)
    {
        if (receiveHeroes.Length < 1 || boxes.Length < 1)
            return;

        Vector3[] hitPoses = new Vector3[boxes.Length];
        if (_attackCheckData != null)
        {
            float flip = _nogPoint.center.localScale.x > 0 ? 1f : -1f;

            Vector3 hitPos = Vector3.zero;
            if (boxes.Length > 0)
            {
                for (int i = 0; i < boxes.Length; i++)
                {
                    hitPoses[i] = new Vector3((boxes[i].MinX + boxes[i].Width * 0.5f) / FLOAT_CORRECTION, (boxes[i].MinY + boxes[i].Height * 0.5f) / FLOAT_CORRECTION, 0f);
                }
                hitPos = hitPoses[hitPoses.Length - 1];
            }

            if (_attackCheckData.blur && _isVisible)
                XPostEffectManager.Instance.StartBlur(hitPos, _attackCheckData.blurDist, _attackCheckData.blurStrength, _attackCheckData.blurFrame);

            _attackCheckData.hitReact.hitPointRot = Quaternion.Euler(new Vector3(0, 0, 90f * (flip - 1))); ;
            _attackCheckData.hitReact.hitPoint = hitPos;

            if (_attackCheckData.dmgCurrent == _attackCheckData.dmgTotal)
                _attackWarningData.Clear();
        }

        if (_attackCallback != null)
        {
            int[] cids = new int[receiveHeroes.Length];
            for (int i = 0; i < receiveHeroes.Length; i++)
                cids[i] = receiveHeroes[i];
            _attackCallback(cids, hitPoses);
        }

        AttackEnd();
    }


    public void AttackEnd()
    {
        _attackCheckData = null;
        _attackCallback = null;

#if UNITY_EDITOR || UNITY_STANDALONE
        SetAttackRecoverFrame(.0f);
#endif
    }

    void RemoveCameraModifier(string name)
    {
        XCameraFight fight = XCameraFight.GetSingleton();
        if (fight)
            fight.RemoveModifier(name);
    }

    void AddCameraModifierForCharge(int modNum, string name, int priority, float loopDuration)
    {
        XCameraFight fight = XCameraFight.GetSingleton();
        if (fight)
            fight.AddModifierForCharge(modNum, name, priority, loopDuration);
    }

    public void FixedFrameUpdate(float deltaTime)
    {
        if (_freezeDuration > 0)
        {
            _freezeFrame++;

            if (_shakePosition && deltaTime > 0)
            {
                Vector3 deltaPos = Vector3.zero;
                if (_freezeFrame % 2 == 0)
                {
                    if (_shakeOffset)
                    {
                        deltaPos.x = _shakeDelta;
                        if (_isShakeY)
                            deltaPos.y = _shakeDelta;
                    }
                    else
                    {
                        deltaPos.x = - _shakeDelta;
                        if (_isShakeY)
                            deltaPos.y = - _shakeDelta;
                    }
                    _shakeOffset = !_shakeOffset;
                }

                _nogPoint.center.transform.localPosition = _originalPosition + deltaPos;
            }

            _freezeDuration -= (int)(deltaTime * 1000);
            if (_freezeDuration <= 0)
            {
                Unfreeze();
                RemoveCameraModifier("modSpecialCam2");
            }
        }

        if (_freezeSpecialCamFrame > 0)
        {
            _freezeSpecialCamFrame--;
            if (_freezeSpecialCamFrame <= 0)
            {
                _freezeSpecialCamFrame = 0;

                RemoveCameraModifier("modSpecialCam1");
                AddCameraModifierForCharge(8, "modSpecialCam2", 9, 0);
            }
        }

        if (_shakeFrame > 0)
        {
            _shakeFrame--;

            float deltaCam = _shakeFrame % 2 == 0 ? 0.05f : -0.05f;
            Camera.main.fieldOfView = _originalCamera + deltaCam;
            if (_shakeFrame <= 0)
            {
                _shakeFrame = 0;
                Camera.main.fieldOfView = _originalCamera;
            }
        }
    }

    public void LateFrameUpdate(float deltaTime) {
        if( _animator != null )
        {
            int animatorState = GetCurrentAnimatorState().fullPathHash;
                
            if( _currentAnimatorState != animatorState )
            {
                OnAnimatorStateChange(_currentAnimatorState, animatorState);
                _currentAnimatorState = animatorState;
            }
        }

        Vector3 delta = transform.localPosition - _lastPosition;
        delta.y = .0f;
        
        if( delta.magnitude >= moveJudge && !string.IsNullOrEmpty(_dustEffect) && transform.localPosition.y < (_groundHeight+0.01f) )
        {
            XEffectManager.GenerateAsync(gameObject, _lastPosition, Quaternion.identity, _dustEffect, 0, null);
        }
        
        _lastPosition = transform.localPosition;
    }

    public void Unfreeze()
    {
        if (_animator == null)
            return;

        XSmoothAnimator smooth = GetComponent<XSmoothAnimator>();
        if (smooth != null)
        {
            smooth.speed = _animatorSpeed;
        }
        else
        {
            _animator.speed = _animatorSpeed;
        }

        _freezeDuration = 0;
        _freezeFrame = 0;

        if (_shakePosition)
            _nogPoint.center.transform.localPosition = _originalPosition;

        int currentState = GetCurrentAnimatorState().fullPathHash;
        FreezeFrameEffect(currentState, false);
    }

    public void SetAnimatorSpeed(float speed)
    {
        XSmoothAnimator smooth = GetComponent<XSmoothAnimator>();
        if (smooth != null)
        {
            smooth.speed = speed;
            _animatorSpeed = speed;
        }
        else if (_animator != null)
        {
            _animator.speed = speed;
            _animatorSpeed = speed;
        }
    }

    void FreezeFrameEffect(int state, bool freeze)
    {
        List<XEffectComponent> list = null;
        _animatorFrameEffects.TryGetValue(state, out list);

        if (list != null)
        {
            for (int i = 0; i < list.Count; i++)
            {
                XEffectComponent xeffect = list[i];
                if (xeffect != null && xeffect.gameObject.activeSelf && xeffect.freezeByAnimation)
                    xeffect.Pause(freeze);
            }
        }
    }

    public void Freeze(int frameCount, bool shakePos, bool specialCam, float shakeDelta, bool shakeY)
    {
        if (_animator == null)
            return;

        XSmoothAnimator smooth = GetComponent<XSmoothAnimator>();
        if (smooth != null)
        {
            smooth.speed = 0.0f;
        }
        else
        {
            _animator.speed = 0.0f;
        }

        _freezeDuration = FRAME_DELTA_TIME * (frameCount + 1);
        _freezeFrame = 0;
        _freezeSpecialCam = specialCam;
        _shakePosition = shakePos;
        _isShakeY = shakeY;
    
        if (_shakePosition)
        {
            _shakeFrame = 4;
            _shakeDelta = shakeDelta > 0.03f ? shakeDelta : 0.03f;
            _originalPosition = Vector3.zero;
            _originalCamera = Camera.main.fieldOfView;
        }

        if (_freezeSpecialCam)
        {
            _freezeSpecialCamFrame = 4;
            AddCameraModifierForCharge(7, "modSpecialCam1", 9, 0);
        }

        int currentState = GetCurrentAnimatorState().fullPathHash;
        FreezeFrameEffect(currentState, true);
    }

    public void Freeze(int frameCount, bool shakePos, bool specialCam)
    {
        Freeze(frameCount, shakePos, specialCam, 0, false);
    }

    public void AnimationBodyBox(AnimationEvent e)
    {
        if (e.objectReferenceParameter != null)
        {
            _bodyBoxConf = e.objectReferenceParameter as XBodyBoxConfigObject;
            if (_bodyMoveBox != null)
            {
                _bodyMoveBox.Width = _bodyBoxConf.BodyBoxWidth * 2 * ADDTIVE_BOX_CORRECTION;
                _bodyMoveBox.Height = _bodyBoxConf.BodyBoxHeight * ADDTIVE_BOX_CORRECTION;
            }
        }
    }

    public void AnimationHug(AnimationEvent e)
    {
        _hugConfigConf = e.objectReferenceParameter as XBoxHugConfigObject;
    }

    public void AnimationHugEnd(AnimationEvent e)
    {
        HugEnd();
    }

    public void AddFrameSound(int status, XAudioComponent xaudio)
    {
        List<XAudioComponent> list = null;
        _animatorFrameSounds.TryGetValue(status, out list);
        if (list == null)
        {
            list = new List<XAudioComponent>();
            _animatorFrameSounds.Add(status, list);
        }

        list.Add(xaudio);
    }

    void OnAnimatorStateChange(int previous, int current)
    {
        InterruptAllPreEffects(previous, current);

        foreach (KeyValuePair<int, List<XAudioComponent>> item in _animatorFrameSounds)
        {
            if (item.Key != current)
            {
                List<XAudioComponent> list = item.Value;
                for (int i = 0; i < list.Count; i++)
                {
                    XAudioComponent xaudio = list[i];
                    if (xaudio != null)
                        xaudio.Stop();
                }
                list.Clear();
            }
        }

        if (_attackCheckData != null)
            AttackEnd();

        if (_attackWariningBox != null && _attackWarningData.CreateState == previous)
            _attackWarningData.Clear();

        if (_hugConfigConf != null)
            HugEnd();

#if UNITY_EDITOR || UNITY_STANDALONE
        if (_isPlayer)
        {
            _selfAttackRecoverFrame = 0;
        }
        else
        {
            AnimatorStateInfo stateInfo = GetCurrentAnimatorState();
            if (stateInfo.tagHash == Animator.StringToHash("hit_react") || stateInfo.tagHash == Animator.StringToHash("hit_down") ||
                stateInfo.tagHash == Animator.StringToHash("block"))
            {

                if (IsDrawBox && _selfAttackRecoverFrame != 0)
                {
                    AnimatorClipInfo[] clips = _animator.GetCurrentAnimatorClipInfo(0);
                    if (clips.Length > 0)
                    {
                        _otherHitReactionFrame = Mathf.RoundToInt(clips[0].clip.length * 30);
                        GLog.LogError("[ADV FRAME] {0}", _otherHitReactionFrame - _selfAttackRecoverFrame);
                    }
                }
            }
            else
                _otherHitReactionFrame = 0;
        }
#endif
    }

    public override void SetAnimatorOverrideAnimation(string name, AnimationClip clip)
    {
        if (_animator == null) return;

        XSmoothAnimator smooth = GetComponent<XSmoothAnimator>();
        if (smooth)
        {
            Animator eventAnimator = smooth.GetEventAnimator();
            if (eventAnimator)
            {
                AnimatorOverrideController ctrl = eventAnimator.runtimeAnimatorController as AnimatorOverrideController;
                if (ctrl && clip)
                {
                    ctrl[name] = clip;
                    eventAnimator.Update(0.0f);
                }
            }

            Animator avatarAnimator = smooth.GetAvatarAnimator();
            if (avatarAnimator)
            {
                AnimatorOverrideController ctrl = avatarAnimator.runtimeAnimatorController as AnimatorOverrideController;
                if (ctrl && clip)
                {
                    ctrl[name] = clip;
                    avatarAnimator.Update(0.0f);
                }
            }
        }

        AnimatorOverrideController controller = _animator.runtimeAnimatorController as AnimatorOverrideController;
        if (controller != null && clip != null)
        {
            controller[name] = clip;
            _animator.Update(.0f);
        }
    }

    public override void SetAnimatorOverrideAnimation(string name, string clip)
    {
        if (_animator == null)
            return;
        AnimatorOverrideController controller = _animator.runtimeAnimatorController as AnimatorOverrideController;
        SetAnimatorOverrideAnimation(name, controller[clip]);
    }

    private bool IsIntersect(Vector2 start, Vector2 end, Rect rect)
    {
        if (IsPointInRect(start, rect) || IsPointInRect(end, rect))
            return true;

        return IsLinesIntersect(start, end, rect);
    }

    private bool IsPointInRect(Vector2 point, Rect rect)
    {
        return rect.Contains(point);
    }

    private bool IsLinesIntersect(Vector2 start, Vector2 end, Rect rect)
    {
        Vector2 topLeft = new Vector2(rect.xMin, rect.yMax);
        Vector2 bottomRight = new Vector2(rect.xMax, rect.yMin);

        Vector2 topRight = new Vector2(rect.xMax, rect.yMax);
        Vector2 bottomLeft = new Vector2(rect.xMin, rect.yMin);

        return IsSegmentIntersect(start, end, topLeft, bottomRight) || IsSegmentIntersect(start, end, topRight, bottomLeft);
    }

    private float PointsMulti(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        return (p1.x - p0.x) * (p2.y - p0.y) - (p2.x - p0.x) * (p1.y - p0.y);
    }

    private bool IsSegmentIntersect(Vector2 start, Vector2 end, Vector2 segmentStart, Vector2 segmentEnd)
    {
        float eps = 0.001f;

        if (PointsMulti(start, segmentStart, segmentEnd) * PointsMulti(end, segmentStart, segmentEnd) > eps)
            return false;

        if (PointsMulti(segmentStart, start, end) * PointsMulti(segmentEnd, start, end) > eps)
            return false;
        return true;
    }

    public void SetSquat(bool isSquat)
    {
        _isSquat = isSquat;
    }

    public Vector3 GetMiddlePoint()
    {
        return Vector3.Lerp(Vector3.Lerp(_nogPoint.leftFoot.position, _nogPoint.rightFoot.position, 0.5f),
            _nogPoint.head.position, 0.5f);
    }

    public void SetPlayer(bool flag)
    {
        _isPlayer = flag;
    }

    virtual public void ApplyGroundHeight(float height)
    {
        _groundHeight = height;
    }

    public float GetGroundHeight()
    {
        return _groundHeight;
    }

    public AnimationClip[] GetAnimationsByAttackLevelName(string name)
    {
        if (_linkConfig != null && _linkConfig.Links != null)
        {
            for (int i = 0; i < _linkConfig.Links.Count; i++)
            {
                if (_linkConfig.Links[i].AttackLevelName == name)
                    return _linkConfig.Links[i].Animations.ToArray();
            }
        }
        return null;
    }

    void OnAnimationEffect(AnimationEvent e)
    {
        if (!_isVisible && e.intParameter != 1)
            return;

        if (e.objectReferenceParameter != null)
        {
            HandleAnimationEffect(e);
        }
        else
        {
            GLog.Log("<color=red>[OnAnimationEffect] {0} Missing effect config object! ani:[{1}]</color>", gameObject.name, e.animatorClipInfo.clip.name);
        }
    }

#if UNITY_EDITOR
    Ray hitRay = new Ray(Vector3.zero, Vector3.right);
    void OnDrawGizmos()
    {
        if (_nogPoint == null)
            return;

        if (_nogPoint.center.lossyScale.x > 0)
            Gizmos.color = Color.green;
        else
            Gizmos.color = Color.red;

        Gizmos.DrawCube(hitRay.origin, new Vector3(0.1f, 0.1f, 0.1f));
        Gizmos.DrawRay(hitRay);

        if (_isShowBodyBox && _bodyBoxShow == null && _bodyBoxConf != null)
        {
            _bodyBoxShow = new XBodyBoxConfigDataTemple<float>();
            _bodyBoxShow.BodyBoxOffset = new Vector2(_bodyBoxConf.BodyBoxOffsetX / FLOAT_CORRECTION, _bodyBoxConf.BodyBoxOffsetY / FLOAT_CORRECTION);
            _bodyBoxShow.BodyBoxHeight = _bodyBoxConf.BodyBoxHeight / FLOAT_CORRECTION;
            _bodyBoxShow.BodyBoxWidth = _bodyBoxConf.BodyBoxWidth / FLOAT_CORRECTION;

            for (int i = 0; i < _bodyBoxConf.ReceiveDamageBoxes.Count; i++)
            {
                XBodyBoxConfigDataTemple<float>.XBodyReceiveDamageBox box = new XBodyBoxConfigDataTemple<float>.XBodyReceiveDamageBox();
                box.BoxOffset = new Vector2(_bodyBoxConf.ReceiveDamageBoxes[i].BoxOffsetX / FLOAT_CORRECTION, _bodyBoxConf.ReceiveDamageBoxes[i].BoxOffsetY / FLOAT_CORRECTION);
                box.BoxHeight = _bodyBoxConf.ReceiveDamageBoxes[i].BoxHeight / FLOAT_CORRECTION;
                box.BoxWidth = _bodyBoxConf.ReceiveDamageBoxes[i].BoxWidth / FLOAT_CORRECTION;

                _bodyBoxShow.ReceiveDamageBoxes.Add(box);
            }
        }

        if (_bodyBoxShow != null)
        {
            Vector3 centerPos = new Vector3(_trans.position.x + _bodyBoxShow.BodyBoxOffset.x, _trans.position.y + _bodyBoxShow.BodyBoxOffset.y);
            Vector3 minPoint = new Vector3(centerPos.x - _bodyBoxShow.BodyBoxWidth, centerPos.y, 0);
            Vector3 topLeft = new Vector3(centerPos.x - _bodyBoxShow.BodyBoxWidth, centerPos.y + _bodyBoxShow.BodyBoxHeight, 0f);
            Vector3 bottomRight = new Vector3(centerPos.x + _bodyBoxShow.BodyBoxWidth, centerPos.y, 0);
            Vector3 topRight = new Vector3(centerPos.x + _bodyBoxShow.BodyBoxWidth, centerPos.y + _bodyBoxShow.BodyBoxHeight, 0f);
            Gizmos.color = Color.yellow;

            Gizmos.DrawLine(minPoint, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, minPoint);
            Gizmos.DrawLine(minPoint, topRight);
            Gizmos.DrawLine(topLeft, bottomRight);

            for (int i = 0; i < _bodyBoxShow.ReceiveDamageBoxes.Count; i++)
            {
                XBodyBoxConfigDataTemple<float>.XBodyReceiveDamageBox box = _bodyBoxShow.ReceiveDamageBoxes[i];

                centerPos = new Vector3(_trans.position.x + box.BoxOffset.x, _trans.position.y + box.BoxOffset.y);
                minPoint = new Vector3(centerPos.x - box.BoxWidth, centerPos.y, 0);
                topLeft = new Vector3(centerPos.x - box.BoxWidth, centerPos.y + box.BoxHeight, 0f);
                bottomRight = new Vector3(centerPos.x + box.BoxWidth, centerPos.y, 0);
                topRight = new Vector3(centerPos.x + box.BoxWidth, centerPos.y + box.BoxHeight, 0f);

                Gizmos.color = Color.green;

                Gizmos.DrawLine(minPoint, bottomRight);
                Gizmos.DrawLine(bottomRight, topRight);
                Gizmos.DrawLine(topRight, topLeft);
                Gizmos.DrawLine(topLeft, minPoint);
                Gizmos.DrawLine(minPoint, topRight);
                Gizmos.DrawLine(topLeft, bottomRight);
            }

            _bodyBoxShow = null;
        }

        if (_boxAttackData != null)
        {
            Transform trans = _trans;
            if (_boxAttackData.IsFollowBone)
            {
                trans = this.GetComponent<Animator>().GetBoneTransform(_boxAttackData.FollowBone);
            }

            Vector3 centerPos = new Vector3(trans.position.x + _boxAttackData.Offset.x, trans.position.y + _boxAttackData.Offset.y);
            Vector3 minPoint = new Vector3(centerPos.x - _boxAttackData.Width, centerPos.y, 0);
            Vector3 topLeft = new Vector3(centerPos.x - _boxAttackData.Width, centerPos.y + _boxAttackData.Height, 0f);
            Vector3 bottomRight = new Vector3(centerPos.x + _boxAttackData.Width, centerPos.y, 0);
            Vector3 topRight = new Vector3(centerPos.x + _boxAttackData.Width, centerPos.y + _boxAttackData.Height, 0f);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(minPoint, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, minPoint);
            Gizmos.DrawLine(minPoint, topRight);
            Gizmos.DrawLine(topLeft, bottomRight);
        }

        if (_attackWarningData.Valid())
        {
            Transform trans = _trans;

            XBoxConfigObject warningBox = _attackWarningData.Box;
            Vector3 centerPos = new Vector3(trans.position.x + warningBox.OffsetX / FLOAT_CORRECTION, trans.position.y + warningBox.OffsetY / FLOAT_CORRECTION);
            Vector3 minPoint = new Vector3(centerPos.x - warningBox.Width / FLOAT_CORRECTION, centerPos.y, 0);
            Vector3 topLeft = new Vector3(centerPos.x - warningBox.Width / FLOAT_CORRECTION, centerPos.y + warningBox.Height / FLOAT_CORRECTION, 0f);
            Vector3 bottomRight = new Vector3(centerPos.x + warningBox.Width / FLOAT_CORRECTION, centerPos.y, 0);
            Vector3 topRight = new Vector3(centerPos.x + warningBox.Width / FLOAT_CORRECTION, centerPos.y + warningBox.Height / FLOAT_CORRECTION, 0f);
            Gizmos.color = new Color(1.0f, 0.5f, 0f);
            Gizmos.DrawLine(minPoint, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, minPoint);
            Gizmos.DrawLine(minPoint, topRight);
            Gizmos.DrawLine(topLeft, bottomRight);
        }

        if (_hugConfigConf != null)
        {
            Transform trans = _trans;

            Vector3 centerPos = new Vector3(trans.position.x + _hugConfigConf.OffsetX / FLOAT_CORRECTION, trans.position.y + _hugConfigConf.OffsetY / FLOAT_CORRECTION);
            Vector3 minPoint = new Vector3(centerPos.x - _hugConfigConf.Width / FLOAT_CORRECTION, centerPos.y, 0);
            Vector3 topLeft = new Vector3(centerPos.x - _hugConfigConf.Width / FLOAT_CORRECTION, centerPos.y + _hugConfigConf.Height / FLOAT_CORRECTION, 0f);
            Vector3 bottomRight = new Vector3(centerPos.x + _hugConfigConf.Width / FLOAT_CORRECTION, centerPos.y, 0);
            Vector3 topRight = new Vector3(centerPos.x + _hugConfigConf.Width / FLOAT_CORRECTION, centerPos.y + _hugConfigConf.Height / FLOAT_CORRECTION, 0f);
            Gizmos.color = new Color(0.0f, 0f, 0.7f);
            Gizmos.DrawLine(minPoint, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, minPoint);
            Gizmos.DrawLine(minPoint, topRight);
            Gizmos.DrawLine(topLeft, bottomRight);
        }
    }
#endif

    /// <summary>
    /// 
    /// </summary>
    public void DrawGL()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        DrawGLDebug();
#endif
    }

#if UNITY_EDITOR || UNITY_STANDALONE
    private Material _bodyBoxMat;
    private Material _attackBoxMat;
    private Material _attackWarningBoxMat;
    private Material _bodyReceiveDamageMat;
    private Material _hugMat;

    private Mesh _bodyMesh;
    private Mesh _attackMesh;
    private Mesh _attackWarningMesh;
    private Mesh _hugMesh;
    private List<Mesh> _boxesMesh;

    private void InitBodyBoxShowStuff()
    {
        _bodyBoxMat = new Material(Shader.Find("PostEffect/Effect/QuadColor"));
        _bodyBoxMat.SetColor("_Color", new Color(1f, 0.92f, 0.016f, 0.4f));

        _attackBoxMat = new Material(Shader.Find("PostEffect/Effect/QuadColor"));
        _attackBoxMat.SetColor("_Color", new Color(1f, 0f, 0f, 0.4f));

        _attackWarningBoxMat = new Material(Shader.Find("PostEffect/Effect/QuadColor"));
        _attackWarningBoxMat.SetColor("_Color", new Color(1f, 0.5f, 0f, 0.4f));

        _bodyReceiveDamageMat = new Material(Shader.Find("PostEffect/Effect/QuadOutLine"));
        _bodyReceiveDamageMat.SetColor("_Color", new Color(0f, 1f, 0.5f, 0f));

        _hugMat = new Material(Shader.Find("PostEffect/Effect/QuadColor"));
        _hugMat.SetColor("_Color", new Color(0f, 0f, 0.7f, 0.4f));

        _boxesMesh = new List<Mesh>();
    }

    private void DestroyBodyBoxShowStuff()
    {
        if (_bodyBoxMat != null)
            Destroy(_bodyBoxMat);
        if (_attackBoxMat != null)
            Destroy(_attackBoxMat);
        if (_bodyReceiveDamageMat != null)
            Destroy(_bodyReceiveDamageMat);
        if (_attackWarningBoxMat != null)
            Destroy(_attackWarningBoxMat);
        if (_hugMat != null)
            Destroy(_hugMat);

        _bodyBoxMat = null;
        _attackBoxMat = null;
        _bodyReceiveDamageMat = null;
        _attackWarningBoxMat = null;
        _hugMat = null;

        if (_boxesMesh != null)
        {
            for (int i = 0; i < _boxesMesh.Count; i++)
            {
                Destroy(_boxesMesh[i]);
            }
            _boxesMesh.Clear();
        }
    }

    private void DrawGLDebug()
    {
        if (_bodyBoxConf == null)
            return;

        if (_bodyBoxMat == null)
            InitBodyBoxShowStuff();

        float width;
        float height;

        float minx;
        float miny;

        XBoxRect r = GetNextBodyBoxRect();

        if (r != null)
        {
            width = r.Width / (FLOAT_CORRECTION * ADDTIVE_BOX_CORRECTION);
            height = r.Height / (FLOAT_CORRECTION * ADDTIVE_BOX_CORRECTION);

            minx = r.MinX / (FLOAT_CORRECTION * ADDTIVE_BOX_CORRECTION);
            miny = r.MinY / (FLOAT_CORRECTION * ADDTIVE_BOX_CORRECTION);

            if (_bodyMesh == null)
                _bodyMesh = new Mesh();
            _bodyMesh.Clear();
            _bodyMesh.vertices = new Vector3[] { new Vector3(minx, miny, 0), new Vector3(minx + width, miny, 0), new Vector3(minx + width, miny + height, 0), new Vector3(minx, miny + height, 0), };
            _bodyMesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
            _bodyMesh.name = "BodyBoxMesh";
            Graphics.DrawMesh(_bodyMesh, Vector3.zero, Quaternion.identity, _bodyBoxMat, 0);
        }
        List<XBoxRect> receiveRects = GetReceiveDamageBoxesRect();
        if (receiveRects != null)
        {
            for (int i = 0; i < receiveRects.Count; i++)
            {
                r = receiveRects[i];
                width = r.Width / FLOAT_CORRECTION;
                height = r.Height / FLOAT_CORRECTION;

                minx = r.MinX / FLOAT_CORRECTION;
                miny = r.MinY / FLOAT_CORRECTION;

                if (i >= _boxesMesh.Count)
                {
                    Mesh m = new Mesh();
                    _boxesMesh.Add(m);
                }

                _boxesMesh[i].Clear();
                _boxesMesh[i].vertices = new Vector3[] { new Vector3(minx, miny, 0), new Vector3(minx + width, miny, 0), new Vector3(minx + width, miny + height, 0), new Vector3(minx, miny + height, 0), };
                _boxesMesh[i].triangles = new int[] { 0, 2, 1, 0, 3, 2 };
                _boxesMesh[i].uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };
                _boxesMesh[i].name = "ReceiveRectsBox" + i;

                Graphics.DrawMesh(_boxesMesh[i], Vector3.zero, Quaternion.identity, _bodyReceiveDamageMat, 0);
            }
        }

        if (_attackMesh == null)
            _attackMesh = new Mesh();
        _attackMesh.Clear();

        r = GetAttackBoxRect();

        if (r != null)
        {
            width = r.Width / FLOAT_CORRECTION;
            height = r.Height / FLOAT_CORRECTION;

            minx = r.MinX / FLOAT_CORRECTION;
            miny = r.MinY / FLOAT_CORRECTION;

            if (_attackMesh == null)
                _attackMesh = new Mesh();
            _attackMesh.Clear();
            _attackMesh.vertices = new Vector3[] { new Vector3(minx, miny, 0), new Vector3(minx + width, miny, 0), new Vector3(minx + width, miny + height, 0), new Vector3(minx, miny + height, 0), };
            _attackMesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
            _attackMesh.name = "AttackBoxMesh";
            Graphics.DrawMesh(_attackMesh, Vector3.zero, Quaternion.identity, _attackBoxMat, 0);
        }

        if (_attackWarningMesh == null)
            _attackWarningMesh = new Mesh();
        _attackWarningMesh.Clear();

        r = GetAttackWarningBoxRect();
        if (r != null)
        {
            width = r.Width / FLOAT_CORRECTION;
            height = r.Height / FLOAT_CORRECTION;

            minx = r.MinX / FLOAT_CORRECTION;
            miny = r.MinY / FLOAT_CORRECTION;

            if (_attackWarningMesh == null)
                _attackWarningMesh = new Mesh();
            _attackWarningMesh.Clear();
            _attackWarningMesh.vertices = new Vector3[] { new Vector3(minx, miny, 0), new Vector3(minx + width, miny, 0), new Vector3(minx + width, miny + height, 0), new Vector3(minx, miny + height, 0), };
            _attackWarningMesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
            _attackWarningMesh.name = "AttackWarningBoxMesh";
            Graphics.DrawMesh(_attackWarningMesh, Vector3.zero, Quaternion.identity, _attackWarningBoxMat, 0);
        }

        r = GetHugBoxRect();
        if (r != null)
        {
            width = r.Width / FLOAT_CORRECTION;
            height = r.Height / FLOAT_CORRECTION;

            minx = r.MinX / FLOAT_CORRECTION;
            miny = r.MinY / FLOAT_CORRECTION;

            if (_hugMesh == null)
                _hugMesh = new Mesh();
            _hugMesh.Clear();
            _hugMesh.vertices = new Vector3[] { new Vector3(minx, miny, 0), new Vector3(minx + width, miny, 0), new Vector3(minx + width, miny + height, 0), new Vector3(minx, miny + height, 0), };
            _hugMesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
            _hugMesh.name = "HugMesh";
            Graphics.DrawMesh(_hugMesh, Vector3.zero, Quaternion.identity, _hugMat, 0);
        }
    }

    GUIStyle guiStyle_ = new GUIStyle();
    static float _attackStartNormalTime = 0;
    static int _selfAttackRecoverFrame = 0;
    static int _otherHitReactionFrame = 0;

    void OnGUI()
    {
        if (!_isPlayer || !IsDrawBox) return;
        guiStyle_.fontSize = 20;

        int frame = 0;
        if (_selfAttackRecoverFrame > 0)
            frame = _otherHitReactionFrame - _selfAttackRecoverFrame;

        if (frame < 0)
            guiStyle_.normal.textColor = Color.red;
        else
            guiStyle_.normal.textColor = Color.green;

        GUI.Label(new Rect(Screen.width / 2 - 20, Screen.height / 4, 40, 40), string.Format("{0}", frame), guiStyle_);
    }
#endif
}
