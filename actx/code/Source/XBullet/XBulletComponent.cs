using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SLua;

[CustomLuaClass]
[System.Serializable]
public class XBulletProperty
{
    public float gravity = .0f;
    public float time = 2.0f;
    public float speed = .0f;
    public float traceSpeed = .0f;
    public bool follow = false;
    public float minHeight = float.MaxValue;
    public int bulletType = 0;
    public float attackInterval = 0f;
    public int bulletLevel = 0;
    public float pveSpeedScale = 1;
    public Vector3 fireVector3 = Vector3.zero;
}

[CustomLuaClass]
public class XBulletComponent : MonoBehaviour
{
    public GameObject fireEffectPrefab;
    public GameObject flyEffectPrefab;
    public GameObject hitEffectPrefab;

    public XBulletProperty property;

    public bool checkEnable = true;
    public int gid = 0;
    [HideInInspector]
    public string resPath;

    protected XEffectComponent fireEffect_;
    protected XEffectComponent flyEffect_;
    protected XEffectComponent hitEffect_;

    Vector3 destPos_ = Vector3.zero;
    Vector3 velocity_ = Vector3.zero;

    float elapseTime_ = .0f;
    float elapseinterval = .0f;

    private const float throughTimeThreshold_ = 0.07f;

    Transform[] hitTargets;
    System.Func<XBulletComponent, Transform, bool> hitCallback_;
    private bool inTraceMode_ = false;

    float minHeight = float.MaxValue;

    Collider collider_;
    private XBoxBulletHit hitBox_;
    Collider fire_collider_;

    //List<Transform> beHits = new List<Transform>();

    private bool isGoingNormal = false;
    private Vector3 waitPosition = Vector3.zero;
    private Transform curTransform = null;
    private bool isReturn = false;
    private bool isAbsorb = false;
    //private bool isBeginFire = true;
    private bool isLift = true;
    private Vector3 fireVec = Vector3.zero;
    private float originHeight = 0f;
    private float coinSpeed = 9f;

    private bool _hasDelayFlying = false;
    private bool _isStop = false;
    private int _flip = 1;

    Vector3 fireVector3 = Vector3.zero;
    Vector3 fireVector3Temp = Vector3.zero;
    public bool isHit = false;
    private bool _isPvp = true;

    void Awake()
    {
        XCameraFight fight = Camera.main.gameObject.GetComponent<XCameraFight>();
        if (fight)
            _isPvp = fight.IsPvp();

        if (fireEffectPrefab != null)
        {
            GameObject obj = GameObject.Instantiate(fireEffectPrefab);
            if (obj != null)
            {
                fireEffect_ = obj.GetComponent<XEffectComponent>();
                fire_collider_ = obj.GetComponent<Collider>();
                if (fire_collider_ != null)
                    fire_collider_.isTrigger = true;
            }
        }

        if (flyEffectPrefab != null)
        {
            GameObject obj = GameObject.Instantiate(flyEffectPrefab);
            if (obj != null)
            {
                flyEffect_ = obj.GetComponent<XEffectComponent>();
                collider_ = obj.GetComponent<Collider>();
                if (collider_ != null)
                    collider_.isTrigger = true;
                hitBox_ = obj.GetComponent<XBoxBulletHit>();
            }
        }

        if (hitEffectPrefab != null)
        {
            GameObject obj = GameObject.Instantiate(hitEffectPrefab);
            if (obj != null)
            {
                hitEffect_ = obj.GetComponent<XEffectComponent>();
                obj.transform.SetParent(transform);
            }
        }

        if (hitEffect_ != null)
        {
            hitEffect_.eventCallBack = delegate (XEffectComponent effect, XEffectComponent.EffectEventType eventType) {
                if (eventType == XEffectComponent.EffectEventType.FINISHED)
                    XEffectManager.Instance.DestroyBullet(this);
            };
        }
    }

    void Init()
    {
        transform.SetParent(null);

        destPos_.z = 0;

        Vector3 deltaPos = destPos_ - transform.position;
        _flip = deltaPos.x < 0 ? -1 : 1;
        float time = property.time;

        velocity_.x = Mathf.RoundToInt(deltaPos.x / time * XBoxComponent.FLOAT_CORRECTION) / XBoxComponent.FLOAT_CORRECTION;

        if (!_isPvp)
        {
            velocity_.x *= property.pveSpeedScale;
        }

        velocity_.z = Mathf.RoundToInt(deltaPos.z / time * XBoxComponent.FLOAT_CORRECTION) / XBoxComponent.FLOAT_CORRECTION;
        property.gravity *= 0.01f;

        velocity_.y = Mathf.RoundToInt((deltaPos.y / time + 0.5f * property.gravity * time) / time * XBoxComponent.FLOAT_CORRECTION) / XBoxComponent.FLOAT_CORRECTION;
        elapseTime_ = .0f;
        elapseinterval = .0f;
        inTraceMode_ = false;

        isGoingNormal = false;
        isReturn = false;
        isAbsorb = false;
        //isBeginFire = true;
        isLift = true;
        fireVec = Vector3.zero;
        originHeight = 0f;

        checkEnable = true;
        isHit = false;
        minHeight = property.minHeight;

        if (hitTargets != null && hitTargets.Length > 0)
        {
            //Transform trans = hitTargets[0];
        }

        if (fireEffect_ != null)
        {
            fireEffect_.Initialize(gameObject, transform.position, transform.rotation, fireEffectPrefab, 0);
            fireEffect_.gameObject.SetActive(true);
            fireEffect_.transform.SetParent(transform);
        }

        if (flyEffect_ != null)
        {
            flyEffect_.Initialize(gameObject, flyEffectPrefab, XEffectComponent.LOOP_TIME);
            flyEffect_.gameObject.SetActive(true);

            if (property.bulletType == 0)
                XBoxSystem.GetSingleton().RegisterBullet(this);
        }

        if (hitEffect_ != null)
            hitEffect_.gameObject.SetActive(false);

        _isStop = false;

        fireVector3 = property.fireVector3;
        fireVector3Temp = property.fireVector3;
    }

    void OnDestroy()
    {
        if (property.bulletType == 0)
            XBoxSystem.GetSingleton().UnRegisterBullet(this);

#if UNITY_EDITOR || UNITY_STANDALONE
        DestroyHitBoxShowStuff();
#endif
    }

    void OnNormal(float fFixedDeltaTime)
    {
        if (hitEffect_ != null && hitEffect_.gameObject.activeSelf || _isStop)
            return;

        if (fireVector3 != Vector3.zero)
        {
            TraceG(fFixedDeltaTime);
        }
        else
        {
            TraceTranslate(fFixedDeltaTime);
        }
    }

    private void TraceG(float fFixedDeltaTime)
    {
        float y_speed = fireVector3Temp.y + property.gravity * fFixedDeltaTime;
        float x_speed = fireVector3.x * fFixedDeltaTime;
        fireVector3Temp = new Vector3(x_speed * transform.right.x, y_speed, 0);
        transform.Translate(fireVector3Temp, Space.World);
        elapseTime_ += fFixedDeltaTime;
    }

    private void TraceTranslate(float fFixedDeltaTime)
    {
        if (!_hasDelayFlying)
        {
            _hasDelayFlying = true;
            return;
        }

        if ((property.follow == true) && (inTraceMode_ == false))
        {
            if (hitTargets.Length > 0 && hitTargets[0] != null)
            {
                inTraceMode_ = true;
            }
        }

        if (checkEnable)
        {

            elapseTime_ += fFixedDeltaTime;
            if (property.gravity == 0 && elapseTime_ > property.time)
            {
                return;
            }

            if (inTraceMode_ == false)
            {
                velocity_.y -= property.gravity * fFixedDeltaTime;
                if (_isPvp)
                {
                    transform.right = velocity_.normalized;
                }
                if (property.traceSpeed != 0)
                {
                    velocity_.y = property.traceSpeed;
                }
                transform.position += new Vector3(Mathf.RoundToInt(velocity_.x * fFixedDeltaTime * XBoxComponent.FLOAT_CORRECTION),
                    Mathf.RoundToInt(velocity_.y * fFixedDeltaTime * XBoxComponent.FLOAT_CORRECTION), Mathf.RoundToInt(velocity_.z * fFixedDeltaTime * XBoxComponent.FLOAT_CORRECTION)) / XBoxComponent.FLOAT_CORRECTION;
            }
            else
            {
                Vector3 targetPosition = new Vector3(hitTargets[0].position.x, hitTargets[0].position.y + 1f, 0f);
                Vector3 theVector = targetPosition - transform.position;
                transform.right = theVector.normalized;
                transform.Translate(transform.right * property.traceSpeed * fFixedDeltaTime, Space.World);
            }
        }
    }

    [DoNotToLua]
    public void HitRunOut()
    {
        if (checkEnable)
        {
            if (inTraceMode_ || property.gravity == 0)
            {
                if (elapseTime_ > property.time)
                    OnHit(null);
            }
            else
            {
                if (transform.position.y <= minHeight)
                {
                    OnHit(null);
                }
            }
        }
    }

    protected virtual bool HitDetection()
    {
        if (hitTargets != null)
        {
            for (int i = 0; i < hitTargets.Length; i++)
            {
                Transform trans = hitTargets[i];
                if (trans != null)
                {
                    bool hit = this.IsIntersect(trans);
                    if (hit)
                    {
                        if (OnHit(trans))
                        {
                            return true;
                        }
                    }

                    if (trans.position.y < minHeight)
                        minHeight = trans.position.y;
                }
            }
        }
        else
        {
            minHeight = .0f;
        }
        return false;
    }

    [DoNotToLua]
    public virtual bool CallHitDetection(List<Transform> transList)
    {
        if (hitTargets != null)
        {
            for (int i = 0; i < hitTargets.Length; i++)
            {
                Transform trans = hitTargets[i];
                if (trans != null)
                {
                    if (transList.Contains(trans))
                    {
                        if (OnHit(trans))
                        {
                            return true;
                        }
                    }

                    if (trans.position.y < minHeight)
                        minHeight = trans.position.y;
                }
            }
        }
        else
        {
            minHeight = .0f;
        }
        HitRunOut();
        return false;
    }

    [DoNotToLua]
    public XBoxRect GetBulletHitBox()
    {
        if (hitBox_ == null)
        {
            GLog.LogError("BULLET DO NOT HAVE A HIT BOX!");
            return new XBoxRect();
        }

        return hitBox_.GetFixedHitBox(_flip);
    }

    [DoNotToLua]
    public XBoxRect GetBulletWarningBox()
    {
        if (hitBox_ == null)
        {
            GLog.LogError("BULLET DO NOT HAVE A HIT BOX!");
            return null;
        }

        return hitBox_.GetFixedWarningBox(_flip);
    }

    void OnPirce(float fFixedDeltaTime)
    {
        velocity_.y -= property.gravity * fFixedDeltaTime;
        transform.right = velocity_.normalized;
        transform.position += velocity_ * fFixedDeltaTime;

        elapseTime_ += fFixedDeltaTime;
        elapseinterval += fFixedDeltaTime;

        bool hit = false;

        if (hitTargets != null)
        {
            for (int i = 0; i < hitTargets.Length; i++)
            {
                Transform trans = hitTargets[i];
                if (trans != null)
                {
                    hit = this.IsIntersect(trans);

                    if (hit)
                    {
                        if (elapseinterval >= property.attackInterval)
                        {
                            elapseinterval = 0f;
                            OnPirceHit(trans);
                        }
                    }

                    if (trans.position.y < minHeight)
                        minHeight = trans.position.y;
                }
            }
        }
        else
        {
            minHeight = .0f;
        }

        if (elapseTime_ >= property.time)
        {
            Stop();
        }
    }

    void OnFire(float fFixedDeltaTime)
    {
        if (fireVec == Vector3.zero)
        {
            fireVec = new Vector3(Random.Range(-3, 3), Random.Range(8, 10), 0);
            originHeight = transform.position.y;
            flyEffect_.gameObject.SetActive(false);
        }

        transform.Translate(fireVec * fFixedDeltaTime, Space.World);

        fireVec.y -= fFixedDeltaTime * 25.8f;

        if (transform.position.y <= originHeight && fireVec.y <= 0)
        {
            //isBeginFire = false;
        }
    }

    void OnCoin(float fFixedDeltaTime)
    {
        isGoingNormal = true;
        ReturnPlayer(hitTargets[0].gameObject);
    }

    void ReturnPlayer(GameObject obj)
    {
        waitPosition = new Vector3(transform.position.x,
        transform.position.y + 0.8f, transform.position.z);
        curTransform = obj.transform;
        isReturn = false;
        coinSpeed = 7f;
    }


    void OnChase(float fFixedDeltaTime)
    {
        Vector3 targetPosition = waitPosition;
        Vector3 theVector = targetPosition - transform.position;

        transform.Translate(theVector.normalized * fFixedDeltaTime * coinSpeed, Space.World);

        if (Vector3.Distance(waitPosition, transform.position) <= 0.25f)
        {
            if (isLift == true)
            {
                isLift = false;
                waitPosition = new Vector3(transform.position.x + Random.Range(-4f, 4f),
                                transform.position.y + Random.Range(0.5f, 1f), transform.position.z);
                coinSpeed = 9f;
                flyEffect_.gameObject.SetActive(true);
                if (fireEffect_ != null)
                    fireEffect_.gameObject.SetActive(false);
            }
            else
            {
                isReturn = true;
            }
        }
    }

    void OnRetrun(float fFixedDeltaTime)
    {
        if (isAbsorb == true)
            return;

        Vector3 targetPosition = new Vector3(curTransform.position.x, curTransform.position.y + 1, 0);
        Vector3 theVector = targetPosition - transform.position;
        transform.Translate(theVector.normalized * fFixedDeltaTime * 12, Space.World);

        if (Vector3.Distance(targetPosition, transform.position) <= 0.2f)
        {
            isAbsorb = true;
            OnHit(null);
        }
    }

    public void ForceFixedUpdate(float fFixedDeltaTime)
    {
        if (property.bulletType == 0)
        {
            OnNormal(fFixedDeltaTime);
#if UNITY_EDITOR || UNITY_STANDALONE
            DrawGL();
#endif
        }
        else if (property.bulletType == 1)
        {
            OnPirce(fFixedDeltaTime);
        }
        else if (property.bulletType == 2)
        {
            if (isGoingNormal == false)
                OnCoin(fFixedDeltaTime);
            else
            {
                if (isReturn == false)
                {
                    OnChase(fFixedDeltaTime);
                }
                else
                {
                    OnRetrun(fFixedDeltaTime);
                }
            }
        }
    }

    void OnPirceHit(Transform target)
    {
        hitCallback_(this, target);
    }

    public void SetIsHit(bool b)
    {
        isHit = b;
    }
    protected bool OnHit(Transform target)
    {
        bool hit = true;
        if (hitCallback_ != null)
        {
            hit = hitCallback_(this, target);
        }

        if (hit)
            Stop();

        return hit;
    }

    public void Fire(Vector3 dest, XBulletProperty prop, Transform[] targets, System.Func<XBulletComponent, Transform, bool> callback)
    {
        destPos_ = dest;
        property = prop;
        hitTargets = targets;
        hitCallback_ = callback;

        Init();
    }

    public void Stop()
    {
        if (flyEffect_ != null)
        {
            if (property.bulletType == 0)
                XBoxSystem.GetSingleton().UnRegisterBullet(this);

            flyEffect_.gameObject.SetActive(false);
        }

        if (hitEffect_ != null)
        {
            hitEffect_.gameObject.SetActive(true);
            hitEffect_.Initialize(gameObject, transform.position, transform.rotation, hitEffectPrefab, 0);
            hitEffect_.transform.SetParent(transform);
        }
        else
        {
            XEffectManager.Instance.AddDestroyBullet(this);
        }
        _hasDelayFlying = false;
        _isStop = true;

    }

    private XBoxRect GetBoundingRect(Transform trans, Collider col)
    {
        if (trans == null || col == null)
        {
            return null;
        }

        float rectX = col.bounds.min.x * XBoxComponent.FLOAT_CORRECTION;

        XBoxRect rect = new XBoxRect();
        rect.MinX = Mathf.RoundToInt(rectX);
        rect.MinY = Mathf.RoundToInt(Mathf.Max(0, col.bounds.min.y) * XBoxComponent.FLOAT_CORRECTION);
        rect.Width = Mathf.RoundToInt(col.bounds.size.x * XBoxComponent.FLOAT_CORRECTION);
        rect.Width = Mathf.RoundToInt(col.bounds.size.y * XBoxComponent.FLOAT_CORRECTION);

        return rect;
    }

    protected bool IsIntersect(Transform trans)
    {
        bool hit = false;
        XBoxComponent hc = trans.GetComponent<XBoxComponent>();
        if (hc != null && collider_ != null)
        {
            List<XBoxRect> hurtBoxes = hc.GetReceiveDamageBoxesRect();
            XBoxRect bulletRect = GetBoundingRect(transform, collider_);
            if (hurtBoxes != null && hurtBoxes.Count > 0 && bulletRect != null)
            {
                for (int i = 0; i < hurtBoxes.Count; i++)
                {
                    if (bulletRect.Overlap(hurtBoxes[i]) >= 0)
                    {
                        hit = true;
                        break;
                    }
                }
            }
        }
        else
        {
            Collider colid = trans.GetComponent<Collider>();

            if (colid != null)
            {
                if (collider_ != null)
                    hit = colid.bounds.Intersects(collider_.bounds);
                else
                    hit = colid.bounds.Contains(transform.position);
            }
        }
        return hit;
    }

    public Transform GetFlyEffectTrans()
    {
        if (flyEffect_ != null)
            return flyEffect_.transform;
        return null;
    }

#if UNITY_EDITOR || UNITY_STANDALONE
    private Material _hitBoxMat;
    private Material _warningBoxMat;
    private Mesh _hitMesh;
    private Mesh _warningMesh;
    private XBoxComponent _hc;

    private void InitHitBoxShowStuff()
    {
        _hitBoxMat = new Material(Shader.Find("PostEffect/Effect/QuadColor"));
        _hitBoxMat.SetColor("_Color", new Color(1f, 1f, 1f, 0.4f));

        _warningBoxMat = new Material(Shader.Find("PostEffect/Effect/QuadColor"));
        _warningBoxMat.SetColor("_Color", new Color(1f, 0.5f, 0f, 0.4f));

        _hc = FindObjectOfType<XBoxComponent>();
    }

    private void DestroyHitBoxShowStuff()
    {
        if (_hitBoxMat != null)
            Destroy(_hitBoxMat);
        if (_warningBoxMat != null)
            Destroy(_warningBoxMat);
        _hitBoxMat = null;
        _warningBoxMat = null;
    }


    private void DrawGL()
    {
        if (hitBox_ == null)
            return;

        if (_hitBoxMat == null)
            InitHitBoxShowStuff();

        if (_hc == null || (_hc != null && !_hc.IsDrawBox))
            return;

        float width;
        float height;

        float minx;
        float miny;

        XBoxRect r = GetBulletHitBox();

        if (r != null)
        {
            width = r.Width / (XBoxComponent.FLOAT_CORRECTION);
            height = r.Height / (XBoxComponent.FLOAT_CORRECTION);

            minx = r.MinX / (XBoxComponent.FLOAT_CORRECTION);
            miny = r.MinY / (XBoxComponent.FLOAT_CORRECTION);

            if (_hitMesh == null)
                _hitMesh = new Mesh();
            _hitMesh.Clear();
            _hitMesh.vertices = new Vector3[] { new Vector3(minx, miny, 0), new Vector3(minx + width, miny, 0), new Vector3(minx + width, miny + height, 0), new Vector3(minx, miny + height, 0), };
            _hitMesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
            _hitMesh.name = "HitBoxMesh";
            Graphics.DrawMesh(_hitMesh, Vector3.zero, Quaternion.identity, _hitBoxMat, 0);
        }


        r = GetBulletWarningBox();

        if (r != null)
        {
            width = r.Width / (XBoxComponent.FLOAT_CORRECTION);
            height = r.Height / (XBoxComponent.FLOAT_CORRECTION);

            minx = r.MinX / (XBoxComponent.FLOAT_CORRECTION);
            miny = r.MinY / (XBoxComponent.FLOAT_CORRECTION);

            if (_warningMesh == null)
                _warningMesh = new Mesh();
            _warningMesh.Clear();
            _warningMesh.vertices = new Vector3[] { new Vector3(minx, miny, 0), new Vector3(minx + width, miny, 0), new Vector3(minx + width, miny + height, 0), new Vector3(minx, miny + height, 0), };
            _warningMesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
            _warningMesh.name = "WarningBoxMesh";
            Graphics.DrawMesh(_warningMesh, Vector3.zero, Quaternion.identity, _warningBoxMat, 0);
        }
    }

#endif

}
