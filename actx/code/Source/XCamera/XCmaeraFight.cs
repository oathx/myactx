using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SLua;


/// <summary>
/// 
/// </summary>
public class XCameraUpdater
{
    private static Animator player;
    private static Animator enemy;

    private static Vector3 sourcePos;
    private static Vector3 targetPos;
    private static List<Transform> transformList;
    private static Vector3 mid;
    private static Vector3 spar;

    public static XCameraType type = XCameraType.PVP;
    public static float hBias;
    public static float vBias;
    public static float offsetHeight;
    public static float offsetFocus;
    public static float followMinHeight;
    public static float followMaxHeight;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pvp"></param>
    public static void SetType(XCameraType cameraType)
    {
        type = cameraType;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="playerGo"></param>
    public static void SetPlayer(GameObject playerGo)
    {
        if (playerGo)
        {
            XSmoothAnimator sync = playerGo.GetComponent<XSmoothAnimator>();
            if (sync)
            {
                player = sync.GetAvatarAnimator();
            }
            else
            {
                player = playerGo.GetComponent<Animator>();
            }

            hBias = 0.5f;
            vBias = 0.5f;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="enemyGo"></param>
    public static void SetEnemy(GameObject enemyGo)
    {
        if (enemyGo)
        {
            XSmoothAnimator sync = enemyGo.GetComponent<XSmoothAnimator>();
            if (sync)
            {
                enemy = sync.GetAvatarAnimator();
            }
            else
            {
                enemy = enemyGo.GetComponent<Animator>();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool Update(bool shouldResetTwoPos, float restTwoPosX)
    {
        sourcePos = Vector3.zero;
        targetPos = Vector3.zero;

        if (player)
        {
            Transform head = player.GetBoneTransform(HumanBodyBones.Head);
            Transform leftFoot = player.GetBoneTransform(HumanBodyBones.LeftFoot);
            Transform rightFoot = player.GetBoneTransform(HumanBodyBones.RightFoot);

            if (head && leftFoot && rightFoot)
            {
                Vector3 middleFootPosition = Vector3.Lerp(leftFoot.position,
                    rightFoot.position, hBias);
                Vector3 centerPosition = Vector3.Lerp(head.position, middleFootPosition, vBias);

                sourcePos = player.transform.position;
                if (type == XCameraType.PVP)
                {
                    sourcePos.y = centerPosition.y;
                }
            }
        }

        if (shouldResetTwoPos)
        {
            if (player && player.transform.gameObject.activeSelf)
            {
                targetPos = player.transform.position;
                targetPos.x = targetPos.x + restTwoPosX;
                targetPos.y = offsetHeight;
                targetPos.z = 0f;
            }
        }
        else
        {
            if (enemy)
            {
                Transform head = enemy.GetBoneTransform(HumanBodyBones.Head);
                Transform leftFoot = enemy.GetBoneTransform(HumanBodyBones.LeftFoot);
                Transform rightFoot = enemy.GetBoneTransform(HumanBodyBones.RightFoot);

                if (head && leftFoot && rightFoot)
                {
                    Vector3 middleFootPosition = Vector3.Lerp(leftFoot.position,
                        rightFoot.position, hBias);
                    Vector3 centerPosition = Vector3.Lerp(head.position, middleFootPosition, vBias);

                    targetPos = enemy.transform.position;
                    if (type == XCameraType.PVP)
                    {
                        targetPos.y = centerPosition.y;
                    }
                }
                else
                {
                    targetPos = enemy.transform.position;
                    targetPos.y = sourcePos.y;
                }
            }
            else
            {
                int monsterCount = transformList.Count;
                if (monsterCount == 1)
                {
                    Transform trans = transformList[0];
                    if (trans)
                    {
                        targetPos = trans.position;
                        targetPos.y = offsetHeight;
                        targetPos.z = 0f;
                    }
                }
                else if (monsterCount > 1)
                {
                    int minNormalIndex = 0;
                    bool isNormalPoint = false;

                    for (int i = 0; i < monsterCount; i++)
                    {
                        Transform trans = transformList[i];
                        if (trans)
                        {
                            if (!isNormalPoint && (sourcePos.x < trans.position.x))
                            {
                                isNormalPoint = true;
                                minNormalIndex = i;

                                break;
                            }
                        }
                    }

                    if (isNormalPoint)
                    {
                        Vector3 nearPoint = transformList[minNormalIndex].position;
                        Vector3 farPoint = transformList[minNormalIndex].position;

                        if (minNormalIndex < (transformList.Count - 1))
                        {
                            for (int k = minNormalIndex + 1; k < transformList.Count; k++)
                            {
                                if (sourcePos.x < transformList[k].position.x)
                                {
                                    Vector3 thePointPosition = transformList[k].position;
                                    float distance1 = Mathf.Abs(sourcePos.x - nearPoint.x);
                                    float distance2 = Mathf.Abs(sourcePos.x - farPoint.x);

                                    float distance = Mathf.Abs(sourcePos.x - thePointPosition.x);

                                    if (distance < distance1)
                                    {
                                        nearPoint = thePointPosition;
                                    }
                                    else if (distance >= distance2)
                                    {
                                        farPoint = thePointPosition;
                                    }

                                }
                            }
                        }

                        targetPos.x = (nearPoint.x + farPoint.x) * 0.5f;
                        targetPos.y = offsetHeight;
                        targetPos.z = 0;
                    }
                    else
                    {
                        Vector3 nearPoint = transformList[0].position;
                        Vector3 farPoint = transformList[0].position;

                        for (int i = 1; i < transformList.Count; i++)
                        {
                            Vector3 thePointPosition = transformList[i].position;
                            float distance1 = Mathf.Abs(sourcePos.x - nearPoint.x);
                            float distance2 = Mathf.Abs(sourcePos.x - farPoint.x);

                            float distance = Mathf.Abs(sourcePos.x - thePointPosition.x);

                            if (distance < distance1)
                            {
                                nearPoint = thePointPosition;
                            }
                            else if (distance >= distance2)
                            {
                                farPoint = thePointPosition;
                            }
                        }

                        targetPos.x = (nearPoint.x + farPoint.x) * 0.5f;
                        targetPos.y = offsetHeight;
                        targetPos.z = 0;
                    }
                }
                else
                {
                    targetPos = player.transform.position;
                    targetPos.x = targetPos.x + restTwoPosX;
                    targetPos.y = offsetHeight;
                    targetPos.z = 0;
                }
            }
        }

        float curY = offsetHeight;
        float minHeight = offsetHeight + followMinHeight;
        float maxHeight = offsetHeight + followMaxHeight;
        if (sourcePos.y > minHeight)
        {
            if (sourcePos.y > maxHeight)
                sourcePos.y = maxHeight;
        }
        else
        {
            sourcePos.y = curY;
        }

        if (targetPos.y > minHeight)
        {
            if (targetPos.y > maxHeight)
                targetPos.y = maxHeight;
        }
        else
        {
            targetPos.y = curY;
        }

        if (Vector3.Distance(sourcePos, targetPos) <= 0.1)
        {
            return false;
        }

        float offsetY = Mathf.Max(sourcePos.y, targetPos.y);

        mid = Vector3.Lerp(sourcePos, targetPos, hBias);
        if (type == XCameraType.PVP)
        {
            mid.y = offsetY;
        }
        else
        {
            mid = sourcePos;

            if (player)
            {
                mid.y = player.transform.position.y + offsetHeight;
            }
        }

        if (type != XCameraType.PVP)
        {
            mid.x += offsetFocus;
        }

        spar = sourcePos - targetPos;

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    public static Vector3 midPosition
    {
        get { return mid; }
    }

    /// <summary>
    /// 
    /// </summary>
    public static Vector3 diffPos
    {
        get { return spar; }
    }

    /// <summary>
    /// 
    /// </summary>
    public static Vector3 sourcePosition
    {
        get { return sourcePos; }
    }

    /// <summary>
    /// 
    /// </summary>
    public static Vector3 targetPosition
    {
        get { return targetPos; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="trans"></param>
    public static void AddTransform(Transform trans)
    {
        transformList.Add(trans);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="trans"></param>
    public static void RemoveTransform(Transform trans)
    {
        transformList.Remove(trans);
    }

    /// <summary>
    /// 
    /// </summary>
    public static void ResetTransformList()
    {
        transformList = new List<Transform>();
        transformList.Clear();
    }
}

/// <summary>
/// 
/// </summary>
public class XCameraModifier
{
    public Vector2 focusMod = Vector2.zero;
    public float heightMod;
    public string name = string.Empty;
    public int priority;
    public float rangeMod;
    public float yawMod;
    public float wallBufferMod;
    public float fieldOfView;
    public XCmaeraModLoopType loopType;
    public float loopDuration;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="theName"></param>
    /// <param name="thePriority"></param>
    /// <param name="theFocusMod"></param>
    /// <param name="theHeightMod"></param>
    /// <param name="theRangeMod"></param>
    /// <param name="theYawMod"></param>
    /// <param name="theWallBufferMod"></param>
    /// <param name="theFieldOfView"></param>
    /// <param name="theLoopType"></param>
    /// <param name="theLoopDuration"></param>
    public XCameraModifier(string theName, int thePriority, Vector2 theFocusMod, float theHeightMod,
        float theRangeMod, float theYawMod, float theWallBufferMod,
        float theFieldOfView, XCmaeraModLoopType theLoopType, float theLoopDuration)
    {
        name = theName;
        priority = thePriority;
        focusMod = theFocusMod;
        heightMod = theHeightMod;
        rangeMod = theRangeMod;
        yawMod = theYawMod;
        wallBufferMod = theWallBufferMod;
        fieldOfView = theFieldOfView;
        loopType = theLoopType;
        loopDuration = theLoopDuration;
    }

    /// <summary>
    /// 
    /// </summary>
    public void ClearValue()
    {
        name = string.Empty;
        rangeMod = 0f;
        yawMod = 0f;
        heightMod = 0f;
        wallBufferMod = 0f;
        priority = -1;
        focusMod = Vector2.zero;
        fieldOfView = 0;
        loopType = XCmaeraModLoopType.Always;
        loopDuration = 0f;
    }
}

/// <summary>
/// 
/// </summary>
[CustomLuaClass]
public enum XCameraType
{
    PVP,
    PVE,
}

/// <summary>
/// 
/// </summary>
[CustomLuaClass]
[RequireComponent(typeof(XCameraShake))]
public class XCameraFight : MonoBehaviour
{
    public float _fov = 50;
    public float _restFocusHeight = 1f;
    public float _maxHeight = 2f;
    public float _minHeight = 0;
    public float _offsetHeight = 0;
    public float _maxRange = 5f;
    public float _tempMaxRange = 5f;
    public float _minRange = 1f;
    public float _focusMoveSpeed = 1f;
    public float _positionMoveSpeed = 1f;
    public float _posHeightMod = 1f;
    public float _sideBuffer = 0.7f;
    public float _offsetFocus = 2f;
    public float _ryA = 0;
    public float _ryB = 0;
    public float _beginMaxDistanceModValue = 7f;
    public float _endMaxDistanceModValue = 5f;
    public float _beginMinDistanceModValue = 3f;
    public float _endMinDistanceModValue = 3.5f;
    public float _followMinHeight = 0f;
    public float _followMaxHeight = 1f;
    public float _resetTwoPosX = 5f;
    public float _beginResetTwoPosX = 0.1f;
    public float _beginChangeSpeedValue = 50.0f;
    public float _focusFastMoveSpeed = 0.8f;
    public float _positionFastMoveSpeed = 0.8f;
    public float _fieldOfViewMoveSpeed = 0.5f;
    public float _wallBuffer = 2f;
    public float _yDiffBoostMult = 1f;
    public float _range = 0f;
    public float _ryLerpSpeed = 0.1f;
    public float _leftBoundary = -100f;
    public float _rightBoundary = 100f;
    public float _airborneHeight = 1f;

    public bool _shouldResetTwoPos = false;
    public bool _debug = false;
    public bool _paused = false;
    public bool _force = false;
    public bool _isRLock = false;
    public bool _isLLock = false;
    public bool _ignoreBoundary = false;

    /// <summary>
    /// 
    /// </summary>
    public Vector3 _foucsOffset = Vector3.zero;

    /// <summary>
    /// 
    /// </summary>
    public XCameraType _cameraType = XCameraType.PVP;
    /// <summary>
    /// 
    /// </summary>
    private bool _inMinCameraDistanceMod = false;
    private bool _inMaxCameraDistanceMod = false;

    /// <summary>
    /// 
    /// </summary>
    private Camera _camera;
    private Vector3 _t0 = Vector3.zero;
    private Vector3 _t1 = Vector3.zero;
    private XCameraModifier _activeModifier;
    private List<XCameraModifier> 
        _activeModifierList = new List<XCameraModifier>();

    private float _loopTimeCounter = 0f;

    /// <summary>
    /// 
    /// </summary>
    private static XCameraFight _instance;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static XCameraFight GetSingleton()
    {
        return _instance;
    }

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        _instance = this;

        ApplyConfigure(XCameraHelper.confPvp);

        _camera = GetComponent<Camera>();
        if (!_camera)
            throw new System.NullReferenceException();

        _camera.fieldOfView = _fov;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="conf"></param>
    private void ApplyConfigure(XCameraConfigure conf)
    {
        _fov = conf.fov;
        _restFocusHeight = conf.restFocusHeight;
        _maxHeight = conf.maxHeight;
        _minHeight = conf.minHeight;
        _maxRange = conf.maxRange;
        _minRange = conf.minRange;
        _focusMoveSpeed = conf.focusMoveSpeed;
        _positionMoveSpeed = conf.positionMoveSpeed;
        _posHeightMod = conf.posHeightMod;
        _sideBuffer = conf.sideBuffer;
        _offsetFocus = conf.focusOffset;
        _ryA = conf.ry0;
        _ryB = conf.ry1;
        _beginMaxDistanceModValue = conf.beginMaxDistanceModValue;
        _endMaxDistanceModValue = conf.endMaxDistanceModValue;
        _beginMinDistanceModValue = conf.beginMinDistanceModValue;
        _endMinDistanceModValue = conf.endMinDiastanceModValue;
        _followMinHeight = conf.followMinHeight;
        _followMaxHeight = conf.followMaxHeight;

        // reset currnt offset height, if not setting the value then camera use LeftFoot and rightFoot mid
        XCameraUpdater.offsetHeight = _restFocusHeight;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        if (!_paused)
        {
            transform.localRotation = Quaternion.identity;

            XCameraUpdater.ResetTransformList();

            // apply new camera config
            ApplyConfigure(
                XCameraHelper.confPvp
                );
            
            _t0 = XCameraUpdater.midPosition;
            _t1 = XCameraUpdater.midPosition;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        StopAllCoroutines();
    }

    /// <summary>
    /// 
    /// </summary>
    private void LateUpdate()
    {
        if (!_paused)
        {
            XCameraUpdater.followMaxHeight = _followMaxHeight;
            XCameraUpdater.followMinHeight = _followMinHeight;

            // update camera
            XCameraUpdater.offsetFocus = _offsetFocus;
            XCameraUpdater.Update(_shouldResetTwoPos,
                _resetTwoPosX);

            LazyOrbitUpdate(_force);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void Update()
    {
        if (!_paused)
        {
            if (!IsPvp())
            {
                XCameraUpdater.Update(false, _resetTwoPosX);

                float theDistance = (XCameraUpdater.sourcePosition - XCameraUpdater.targetPosition).magnitude;

                if (theDistance > _beginResetTwoPosX)
                {
                    _shouldResetTwoPos = true;
                    theDistance = _resetTwoPosX;
                }
                else
                {
                    _shouldResetTwoPos = false;
                }

                if (_inMaxCameraDistanceMod == false)
                {
                    if (theDistance > _beginMaxDistanceModValue)
                    {
                        AddModifierForCharge(2, "modMaxDistance", 1, 0);
                        _inMaxCameraDistanceMod = true;
                    }
                }
                else
                {
                    if (theDistance < _endMaxDistanceModValue)
                    {
                        RemoveModifier("modMaxDistance");
                        _inMaxCameraDistanceMod = false;
                    }
                }

                if (_inMinCameraDistanceMod == false)
                {
                    if (theDistance < _beginMinDistanceModValue)
                    {
                        AddModifierForCharge(3, "modMinDistance", 1, 0);
                        _inMinCameraDistanceMod = true;
                    }
                }
                else
                {
                    if (theDistance > _endMinDistanceModValue)
                    {
                        RemoveModifier("modMinDistance");
                        _inMinCameraDistanceMod = false;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="force"></param>
    private void LazyOrbitUpdate(bool force)
    {
        Vector3 vector;
        if (Time.timeScale > .0f)
        {
            if (_activeModifier != null)
            {
                if (_camera.fieldOfView != _activeModifier.fieldOfView)
                    _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, _activeModifier.fieldOfView, _fieldOfViewMoveSpeed * Time.timeScale);
            }
            else
            {
                if (_camera.fieldOfView != _fov && Time.timeScale > .0f)
                    _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, _fov, _fieldOfViewMoveSpeed * Time.timeScale);
            }
        }

        float num = ((_camera.fieldOfView * Screen.width) / ((float)Screen.height)) * 0.5f;
        float num2 = (XCameraUpdater.diffPos.magnitude * 0.5f) + (2f * _sideBuffer);
        _range = ((num2 / Mathf.Tan(num * (Mathf.PI / 180f))) + (Mathf.Abs(XCameraUpdater.diffPos.y) * _yDiffBoostMult)) + GetRangeModifier();
        _range = Mathf.Clamp(_range, _minRange, _maxRange);
        _t1 = XCameraUpdater.midPosition + GetFocusModifier() + _foucsOffset;
        _t0 = Vector3.Lerp(_t0, _t1, !force ? _focusMoveSpeed : 1f);
        _ryA = Mathf.Lerp(_ryA, _ryB + GetYawModifier(), !force ? _ryLerpSpeed : 1f);

        vector.x = -Mathf.Sin((Mathf.PI / 180f) * _ryA);
        vector.z = -Mathf.Cos((Mathf.PI / 180f) * _ryA);
        vector.y = _offsetHeight;
        Vector3 to = _t0 + ((Vector3)(vector * _range));
        if (_ignoreBoundary == false)
        {
            if (to.x <= _leftBoundary + _wallBuffer - GetWallBufferModifier())
            {
                _isLLock = true;
            }
            else
            {
                _isLLock = false;
            }

            if (to.x >= _rightBoundary - _wallBuffer + GetWallBufferModifier())
            {
                _isRLock = true;
            }
            else
            {
                _isRLock = false;
            }

            if (_cameraType == XCameraType.PVP)
            {
                _t0.x = Mathf.Clamp(_t0.x, _leftBoundary + _wallBuffer - GetWallBufferModifier(), _rightBoundary - _wallBuffer + GetWallBufferModifier());
                to.x = Mathf.Clamp(to.x, _leftBoundary + _wallBuffer - GetWallBufferModifier(),
                                   _rightBoundary - _wallBuffer + GetWallBufferModifier());
            }
            else
            {
                _t0.x = Mathf.Clamp(_t0.x, _leftBoundary, _rightBoundary);
                to.x = Mathf.Clamp(to.x, _leftBoundary, _rightBoundary);
            }
        }

        to.y = _t1.y + GetHeightModifier();

        if (Time.timeScale > 0f)
        {
            float focus_distance = (_t1 - _t0).magnitude;
            float target_distance = (to - transform.position).magnitude;

            if ((target_distance > _beginChangeSpeedValue) || (focus_distance > _beginChangeSpeedValue))
            {
                _t0 = Vector3.Lerp(_t0, _t1, !force ? (_focusFastMoveSpeed * Time.timeScale) : 1f);
                to = Vector3.Lerp(transform.position, to, !force ? (_positionFastMoveSpeed * Time.timeScale) : 1f);
            }
            else
            {
                _t0 = Vector3.Lerp(_t0, _t1, !force ? _focusMoveSpeed : 1f);
                to = Vector3.Lerp(transform.position, to, !force ? (_positionMoveSpeed * Time.timeScale) : 1f);

                Vector3 dir = _t0 - _t1;
                if (dir != Vector3.zero)
                {
                    if (dir.x > 0.01f)
                    {
                        _t0 = Vector3.Lerp(_t0, _t1, !force ? _focusFastMoveSpeed : 1f);
                    }
                }
            }
        }

        transform.position = to;
        transform.LookAt(to);
        if (_cameraType != XCameraType.PVP)
            transform.rotation = Quaternion.Euler(transform.rotation.x, 0, transform.rotation.z);

        DoActiveModByLoopType();
    }
        
    /// <summary>
    /// 
    /// </summary>
    private void DoActiveModByLoopType()
    {
        if (_activeModifier != null)
        {
            if (_activeModifier.loopType == XCmaeraModLoopType.Once)
            {
                _loopTimeCounter += Time.fixedDeltaTime;
                if (_loopTimeCounter > _activeModifier.loopDuration)
                {
                    RemoveModifier(_activeModifier.name);
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mod"></param>
    public void AddModifier(XCameraModifier mod)
    {
        if ((_activeModifier == null) || (mod.priority >= _activeModifier.priority))
        {
            _activeModifierList.Add(mod);

            UpdateModifier();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    public void RemoveModifier(string name)
    {
        int index = _activeModifierList.FindIndex(delegate(XCameraModifier mod) {
            return mod.name.Equals(name);
        });
        if (index >= 0)
        {
            _activeModifierList.RemoveAt(index);

            UpdateModifier();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void ClearModifier()
    {
        _inMinCameraDistanceMod = false;
        _inMaxCameraDistanceMod = false;

        if (_activeModifierList.Count > 0)
            _activeModifierList.Clear();

        UpdateModifier();
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateModifier()
    {
        _activeModifier = null;
        _loopTimeCounter = .0f;

        if (_activeModifierList.Count > 0)
        {
            _activeModifier = _activeModifierList[_activeModifierList.Count - 1];
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private Vector3 GetFocusModifier()
    {
        return ((_activeModifier == null) ? Vector3.zero : new Vector3(_activeModifier.focusMod.x, _activeModifier.focusMod.y, 0f));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private float GetHeightModifier()
    {
        return ((_activeModifier == null) ? 0f : _activeModifier.heightMod);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private float GetRangeModifier()
    {
        return ((_activeModifier == null) ? 0f : _activeModifier.rangeMod);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private float GetWallBufferModifier()
    {
        return ((_activeModifier == null) ? 0f : _activeModifier.wallBufferMod);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private float GetYawModifier()
    {
        return ((_activeModifier == null) ? 0f : _activeModifier.yawMod);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public float GetCameraLeftBarride()
    {
        return _wallBuffer - GetWallBufferModifier();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public float GetCameraRightBarride()
    {
        return _wallBuffer - GetWallBufferModifier();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public XCameraModifier GetActiveModifier()
    {
        return _activeModifier;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="paused"></param>
    public void SetPaused(bool paused)
    {
        _paused = paused;
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResetCameraForce()
    {
        _force = true;

        StartCoroutine(DoInternalResetCameraForce());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator DoInternalResetCameraForce()
    {
        int delay = 2;
        while (delay > 0)
        {
            yield return null;
            delay --;
        }

        _force = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="trans"></param>
    public void AddTransform(Transform trans)
    {
        XCameraUpdater.AddTransform(trans);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="trans"></param>
    public void RemoveTransform(Transform trans)
    {
        XCameraUpdater.RemoveTransform(trans);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResetTransformList()
    {
        XCameraUpdater.ResetTransformList();
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="go"></param>
    public void SetSource(GameObject go)
    {
        XCameraUpdater.SetPlayer(go);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="go"></param>
    public void SetTarget(GameObject go)
    {
        XCameraUpdater.SetEnemy(go);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool IsPvp()
    {
        return _cameraType == XCameraType.PVP;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="left"></param>
    /// <param name="right"></param>
    public void Configure(XCameraType type, float left, float right, GameObject source, GameObject target)
    {
        XCameraUpdater.type = _cameraType;

        _cameraType = type;
        _leftBoundary = left;
        _rightBoundary = right;

        if (source)
            SetSource(source);

        if (target)
            SetTarget(target);

        XCameraConfigure conf = XCameraHelper.confPvp;
        if (IsPvp())
        {
            _shouldResetTwoPos = false;
        }
        else
        {
            _inMinCameraDistanceMod = false;
            _inMaxCameraDistanceMod = false;

            conf = XCameraHelper.confPve;
        }

        ApplyConfigure(conf);
        ResetCameraForce();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modNum"></param>
    /// <param name="name"></param>
    /// <param name="priority"></param>
    /// <param name="loopDuration"></param>
    public void AddModifierForCharge(int modNum, string name, int priority, float loopDuration)
    {
        XCameraConfigure conf = XCameraHelper.confPvp;
        if (!IsPvp())
            conf = XCameraHelper.confPve;

        XCameraConfigure.ModClass mod = conf.GetMod(name);

        if (mod != null)
        {
            XCameraModifier modForCharge =new XCameraModifier(mod.modName, mod.modPriority, mod.focusMod, 
                mod.heightMod, mod.rangeMod, mod.yawMod * priority, mod.wallBufferMod, mod.fieldOfView, mod.loopType, loopDuration);

            AddModifier(modForCharge);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool PlayShake(int type)
    {
        XCameraConfigure conf = XCameraHelper.confPvp;
        if (!IsPvp())
        {
            conf = XCameraHelper.confPve;
        }

        if (!conf || conf.myShakes.Count < type)
        {
            return false;
        }

        XCameraShake shake = XCameraShake.GetSingleton();
        if (shake && shake.isActiveAndEnabled)
        {
            shake.DoShake(conf.myShakes[type - 1].numberOfShakes, 
                        conf.myShakes[type - 1].shakeAmount,
                        conf.myShakes[type - 1].rotationAmount, 
                        conf.myShakes[type - 1].distance,
                        conf.myShakes[type - 1].speed, 
                        conf.myShakes[type - 1].decay,
                        conf.myShakes[type - 1].guiShakeMod,
                        conf.myShakes[type - 1].multiplyByTimeScale);
        }

        return true;
    }
}
