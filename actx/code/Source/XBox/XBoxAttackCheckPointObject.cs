using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SLua;


[CustomLuaClass]
[System.Serializable]
public class XBoxHitReact
{
    public enum HitReactAniType
    {
        None, SoftHit, HeavyHit, DownHit, AttackFly,
        TC2Hit, SkillSwoon, FastHit,
        LeftChestHit, RightChestHit, FrontChestHit,
        CrouchHit, CrouchAttackFly, CircleHit, HitFlow, LongHit,
        fukong1, fukong2, fukong3, zhanshaNormal, zhanshaBoss,
        TC2Hit2, BehindHit, RicochetHit, ClutchHit, BaTC2Hit, BaTC2Hit2,
    };

    public enum AirHitReactAniType
    {
        None, SoftHit, DownHit, DropHit, AttackFly, WelkinHit, WelkinHitAgain,
    };

    public enum HitReactCamShakeType
    {
        None, Soft, Medium, Heavy, HitFocus, PVE_large01, PVE_large02, PVE_soft, PVE_medium, PVE_large,
    };

    [HideInInspector, System.NonSerialized]
    public Quaternion hitPointRot;
    [HideInInspector, System.NonSerialized]
    public Vector3 hitPoint;

    public HitReactCamShakeType cameraShake = HitReactCamShakeType.None;
    public HitReactCamShakeType cameraMonsterShake = HitReactCamShakeType.None;
    [Header("Audio")]
    public GameObject hitAudio;

    [Header("Block")]
    public bool canBlock = true;
    public bool canBlockDown = true;
    [Space(5)]
    public int blockFreeze = 4;
    public int blockFreezeSelf = 2;

    [Space(20), Header("Hero Reaction")]
    public HitReactAniType hitReactAni = HitReactAniType.SoftHit;
    public AirHitReactAniType airHitReactAni = AirHitReactAniType.SoftHit;
    public XEffectConfigObject[] hitEffects;
    [Space(5)]
    public XDynamicPointLightInstance dynamicLightInstance;
    [Space(5)]
    public int freeze = 4;
    public int freezeSelf = 4;
    public int freezehero = 0;
    public int freemonsterSelf = 0;
    public int airFreeze = 4;
    public int airFreezeSelf = 4;
    public int airFreezeHero = 0;
    public int airFreezeMonsterSelf = 0;
    public float shakeDelta = 0.03f;

    public string cameraMod;
    public float cameraModDuration = 0f;

    public HitReactAniType flowHitReactAni = HitReactAniType.fukong1;

    [Space(20), Header("Monster Reaction")]

    public HitReactCamShakeType monsterCameraShake = HitReactCamShakeType.None;
    public HitReactCamShakeType bulletCameraShake = HitReactCamShakeType.None;

    public bool canBlockMonster = true;
    public bool ignInvulnerable = false;
    public bool breakSuperArmor = false;
    public bool canSendDamage = true;
    public HitReactAniType hitReactAniMonster = HitReactAniType.SoftHit;

    public HitReactAniType hitReactAniSuperMonter = HitReactAniType.SoftHit;
    public HitReactAniType hitMutiAniMonster = HitReactAniType.fukong1;
    public HitReactAniType hitBossAniMonster = HitReactAniType.FastHit;
    public AirHitReactAniType hitReactAirMonster = AirHitReactAniType.None;

    public XEffectConfigObject[] hitEffectsMonster;
    [Space(5)]
    public int freezeMonster = 2;
    public int freezeMonsterSelf = 2;

}

[CustomLuaClass]
[System.Serializable]
public class XBoxAttackSfxProperty : XBulletProperty
{
    public int damageTotal = 1;
    public float damageInterval = 0.1f;
    public bool through = false;

    [DoNotToLua]
    public void Copy(XBoxAttackSfxProperty prop)
    {
        damageTotal = prop.damageTotal;
        damageInterval = prop.damageInterval;
        through = prop.through;

        attackInterval = prop.attackInterval;
        bulletLevel = prop.bulletLevel;
        bulletType = prop.bulletType;
        fireVector3 = prop.fireVector3;
        follow = prop.follow;
        gravity = prop.gravity;
        minHeight = prop.minHeight;
        pveSpeedScale = prop.pveSpeedScale;
        speed = prop.speed;
        time = prop.time;
        traceSpeed = prop.traceSpeed;
    }
}

[CustomLuaClass]
public class XAnimationEventData
{
    public string stringParameter;
    public int intParameter;
    public float floatParameter;
}

[CustomLuaClass]
public enum XAttackCheckType
{
    Normal, Bullet, Tornado, FlyProp, FlyTackle,
};

[CustomLuaClass]
public enum XAttackRangeType
{
    None, Middle, Lower, All
};

[CustomLuaClass]
[CreateAssetMenuAttribute]
public class XBoxAttackCheckPointObject : ScriptableObject
{
    [DoNotToLua]
    public void SetData(HumanBodyBones b, Vector3 p, Quaternion r, float _range)
    {
        bone = b;
        position = p;
        rotation = r;
        range = _range;
    }

    [DoNotToLua]
    public void CopySetting(XBoxAttackCheckPointObject source)
    {
        hitReact = source.hitReact;
        blur = source.blur;
        blurDist = source.blurDist;
        blurFrame = source.blurFrame;
        blurStrength = source.blurStrength;

        rangeMonster = source.rangeMonster;
        rangeMonsterVector4 = source.rangeMonsterVector4;
        rangeMonsterOffset = source.rangeMonsterOffset;


        checkType = source.checkType;
        attackRange = source.attackRange;
        attackSfx = source.attackSfx;
        attackSfxProp = source.attackSfxProp;
        //attackSfxPropPVE = source.attackSfxPropPVE;
    }

    [SerializeField]
    public string AssetGUID = "";

    [HideInInspector, System.NonSerialized]
    public string attackName = string.Empty;

    [Header("Check Data [Create by ActionEditor]")]
    public HumanBodyBones bone;
    public Vector3 position;
    public Quaternion rotation;
    public float range;
    public float rangeMonster = 2.5f;
    public Vector4 rangeMonsterVector4 = new Vector4(2, 2, 2, 2);
    public float attackRangeY = 0f;
    public Vector4 rangeMonsterOffset = Vector4.zero;
    public int AttackBoxOffsetX = 0;
    public int AttackBoxOffsetY = 0;
    public int AttackBoxWidth = 0;
    public int AttackBoxHeight = 0;
    public bool IsAttackBoxFollowBone = false;

    [Header("Hit Radial Blur"), Space(30)]
    public bool blur = false;
    public float blurDist = 0.1f;
    public float blurStrength = 3.5f;
    public int blurFrame = 3;

    [HideInInspector, System.NonSerialized]
    public int dmgCurrent = 1;
    [HideInInspector, System.NonSerialized]
    public int dmgTotal = 1;
    [HideInInspector, System.NonSerialized]
    public int dmgRate = 100;

    [Header("Hit Reaction"), Space(30)]
    public XBoxHitReact hitReact;

    [Header("Attack Check"), Space(30)]
    public XAttackCheckType checkType = XAttackCheckType.Normal;
    public XAttackRangeType attackRange = XAttackRangeType.None;
    public XEffectConfigObject attackSfx;
    public XBoxAttackSfxProperty attackSfxProp;
    //public XBoxAttackSfxProperty attackSfxPropPVE;
    [HideInInspector, System.NonSerialized]
    public bool simulation;
    [HideInInspector, System.NonSerialized]
    public float delay;
    [HideInInspector, System.NonSerialized]
    public AnimationClip aniClip;
    [HideInInspector, System.NonSerialized]
    public string stringParam;

    public float tackleTime = 0;
    public bool MissHitDisAttackWindow = false;

    public int BuffIdSelf = -1;
    public float BuffTimeSelf = -1;

    public int BuffIdTarget = -1;
    public float BuffTimeTarget = -1;
}

