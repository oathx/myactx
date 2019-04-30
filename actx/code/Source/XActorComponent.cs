using System.Collections.Generic;
using UnityEngine;
using SLua;

public enum XActorElementName
{
    model, shadow, shape,
};

[CustomLuaClass]
public class XActorComponent : MonoBehaviour {
    [CustomLuaClass]
    public enum BoneNogType
    {
        None,
        RHand,
        LHand,
        Hit,
        RFoot,
        LFoot,
        Center,
        Head,
        Pelvis,
        RElbow,
        LElbow,
        RKnee,
        LKnee,
        RWeapon,
        LWeapon,
        REye,
        LEye,
        Reserve1,
        Reserve2,
        Reserve3,
        LshoulderArmor,
        RshoulderArmor,
        LUpperArm,
        RUpperArm,
        LForearm,
        RForearm,
        Spine,
        Spine1,
        Spine2,
        LThigh,
        RThigh,
        LCalf,
        RCalf
    };

    [System.Serializable, CustomLuaClass]
    public class BoneNogPoint
    {
        public Transform head = null;
        public Transform hit = null;
        public Transform pelvis = null;
        public Transform center = null;
        public Transform leftElbow = null;
        public Transform rightElbow = null;
        public Transform leftHand = null;
        public Transform rightHand = null;
        public Transform leftKnee = null;
        public Transform rightKnee = null;
        public Transform leftFoot = null;
        public Transform rightFoot = null;
        public Transform leftWeapon = null;
        public Transform rightWeapon = null;
        public Transform leftEye = null;
        public Transform rightEye = null;
        public Transform reserve1 = null;
        public Transform reserve2 = null;
        public Transform reserve3 = null;

        public Transform leftshoulderArmor = null;
        public Transform rightshoulderArmor = null;
        public Transform leftUpperArm = null;
        public Transform rightUpperArm = null;
        public Transform leftForearm = null;
        public Transform rightForearm = null;
        public Transform spine = null;
        public Transform spine1 = null;
        public Transform spine2 = null;
        public Transform leftThigh = null;
        public Transform rightThigh = null;
        public Transform leftCalf = null;
        public Transform rightCalf = null;
    }

    protected BoneNogPoint _nogPoint;
    protected GameObject _checkPoint;
    protected XBoxAnimationLinkConfigObject _linkConfig;
    protected int _effectResIndex = 0;
    protected int _heroStarsIndex = 0;
    protected int _heroEquipStarsIndex = 0;
    protected int _warnXEffectId = 0;

    /// <summary>
    /// 
    /// </summary>
    protected Dictionary<string, AnimationClip>
        _overridedAnimation = new Dictionary<string, AnimationClip>();
    protected GameObject _footstepFallAudio;
    protected float _squatHeight = 1.2f;
    /// <summary>
    /// 
    /// </summary>
    protected Animator _animator;
    protected int _cid = 0;

    protected Dictionary<int, List<XEffectComponent>> _animatorFrameEffects = new Dictionary<int, List<XEffectComponent>>();

    /// <summary>
    /// 
    /// </summary>
    [ContextMenu("Generate Bone NogPoint")]
    public void GenerateBoneNogPoint()
    {
        _nogPoint = new BoneNogPoint();
        BoneTransformToNog();
        FindAllNogPoint(transform);
    }

    public BoneNogPoint NogPoint
    {
        get { return _nogPoint; }
    }

    public XBoxAnimationLinkConfigObject LinkConfig
    {
        get { return _linkConfig; }
    }

    public GameObject FootstepFallAudio
    {
        get { return _footstepFallAudio; }
    }

    public float SquatHeight
    {
        get { return _squatHeight; }
    }

    /// <summary>
    /// 
    /// </summary>
    public int cid
    { get; set;}

    /// <summary>
    /// 
    /// </summary>
    private void BoneTransformToNog()
    {
        if (!_animator)
            _animator = GetComponent<Animator>();

        _nogPoint.head = _animator.GetBoneTransform(HumanBodyBones.Head);
        _nogPoint.hit = _animator.GetBoneTransform(HumanBodyBones.Chest);
        _nogPoint.pelvis = _animator.GetBoneTransform(HumanBodyBones.Hips);
        _nogPoint.leftElbow = _animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        _nogPoint.leftHand = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
        _nogPoint.rightElbow = _animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        _nogPoint.rightHand = _animator.GetBoneTransform(HumanBodyBones.RightHand);
        _nogPoint.leftKnee = _animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        _nogPoint.rightKnee = _animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        _nogPoint.leftFoot = _animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        _nogPoint.rightFoot = _animator.GetBoneTransform(HumanBodyBones.RightFoot);
        _nogPoint.leftEye = _animator.GetBoneTransform(HumanBodyBones.LeftEye);
        _nogPoint.rightEye = _animator.GetBoneTransform(HumanBodyBones.RightEye);

        _nogPoint.center = transform.Find(XActorElementName.shape.ToString());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="root"></param>
    private void FindAllNogPoint(Transform root)
    {
        foreach (Transform child in root)
        {
            string name = child.name.ToLower();
            if (name == "head_empty")
                _nogPoint.head = child;
            else if (name == "hit_empty")
                _nogPoint.hit = child;
            else if (name == "pelvis_empty")
                _nogPoint.pelvis = child;
            else if (name == "center_empty")
                _nogPoint.center = child;
            else if (name == "l_anc_empty")
                _nogPoint.leftElbow = child;
            else if (name == "l_h_empty")
                _nogPoint.leftHand = child;
            else if (name == "r_anc_empty")
                _nogPoint.rightElbow = child;
            else if (name == "r_h_empty")
                _nogPoint.rightHand = child;
            else if (name == "l_knee_empty")
                _nogPoint.leftKnee = child;
            else if (name == "r_knee_empty")
                _nogPoint.rightKnee = child;
            else if (name == "l_f_empty")
                _nogPoint.leftFoot = child;
            else if (name == "r_f_empty")
                _nogPoint.rightFoot = child;
            else if (name == "l_weapon")
                _nogPoint.leftWeapon = child;
            else if (name == "r_weapon")
                _nogPoint.rightWeapon = child;
            else if (name == "bip01")
                _nogPoint.center = child;

            FindAllNogPoint(child);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bindType"></param>
    /// <returns></returns>
    public Transform GetBoneNogPoint(BoneNogType bindType)
    {
        Transform nogPointTrans = null;

        switch (bindType)
        {
            case BoneNogType.Head:
                nogPointTrans = _nogPoint.head;
                break;
            case BoneNogType.Hit:
                nogPointTrans = _nogPoint.hit;
                break;
            case BoneNogType.Pelvis:
                nogPointTrans = _nogPoint.pelvis;
                break;
            case BoneNogType.Center:
                nogPointTrans = _nogPoint.center;
                break;
            case BoneNogType.LElbow:
                nogPointTrans = _nogPoint.leftElbow;
                break;
            case BoneNogType.RElbow:
                nogPointTrans = _nogPoint.rightElbow;
                break;
            case BoneNogType.LHand:
                nogPointTrans = _nogPoint.leftHand;
                break;
            case BoneNogType.RHand:
                nogPointTrans = _nogPoint.rightHand;
                break;
            case BoneNogType.LKnee:
                nogPointTrans = _nogPoint.leftKnee;
                break;
            case BoneNogType.RKnee:
                nogPointTrans = _nogPoint.rightKnee;
                break;
            case BoneNogType.LFoot:
                nogPointTrans = _nogPoint.leftFoot;
                break;
            case BoneNogType.RFoot:
                nogPointTrans = _nogPoint.rightFoot;
                break;
            case BoneNogType.LWeapon:
                nogPointTrans = _nogPoint.leftWeapon;
                break;
            case BoneNogType.RWeapon:
                nogPointTrans = _nogPoint.rightWeapon;
                break;
            case BoneNogType.Reserve1:
                nogPointTrans = _nogPoint.reserve1;
                break;
            case BoneNogType.Reserve2:
                nogPointTrans = _nogPoint.reserve2;
                break;
            case BoneNogType.Reserve3:
                nogPointTrans = _nogPoint.reserve3;
                break;
            case BoneNogType.LshoulderArmor:
                nogPointTrans = _nogPoint.leftshoulderArmor;
                break;
            case BoneNogType.RshoulderArmor:
                nogPointTrans = _nogPoint.rightshoulderArmor;
                break;
            case BoneNogType.LUpperArm:
                nogPointTrans = _nogPoint.leftUpperArm;
                break;
            case BoneNogType.RUpperArm:
                nogPointTrans = _nogPoint.rightUpperArm;
                break;
            case BoneNogType.LForearm:
                nogPointTrans = _nogPoint.leftForearm;
                break;
            case BoneNogType.RForearm:
                nogPointTrans = _nogPoint.rightForearm;
                break;
            case BoneNogType.Spine:
                nogPointTrans = _nogPoint.spine;
                break;
            case BoneNogType.Spine1:
                nogPointTrans = _nogPoint.spine1;
                break;
            case BoneNogType.Spine2:
                nogPointTrans = _nogPoint.spine2;
                break;
            case BoneNogType.LCalf:
                nogPointTrans = _nogPoint.leftCalf;
                break;
            case BoneNogType.RCalf:
                nogPointTrans = _nogPoint.rightCalf;
                break;
        }

        if (nogPointTrans == null)
            nogPointTrans = transform;

        return nogPointTrans;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Animator GetAnimator()
    {
        return _animator;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="clip"></param>
    public virtual void SetAnimatorOverrideAnimation(string name, AnimationClip clip)
    {
        if (!_animator)
            return;

        AnimatorOverrideController controller = _animator.runtimeAnimatorController as AnimatorOverrideController;
        if (controller != null && clip != null)
        {
            AnimationClip old = controller[name];
            controller[name] = clip;

            if (!_overridedAnimation.ContainsKey(name))
                _overridedAnimation.Add(name, old);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="clip"></param>
    public virtual void SetAnimatorOverrideAnimation(string name, string clip)
    {
        if (!_animator)
            return;

        AnimatorOverrideController controller = _animator.runtimeAnimatorController as AnimatorOverrideController;
        if (controller != null && !string.IsNullOrEmpty(clip))
        {
            AnimationClip old = controller[name];
            controller[name] = controller[clip];

            if (!_overridedAnimation.ContainsKey(name))
                _overridedAnimation.Add(name, old);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    public void RevertOverrideAnimation(string name)
    {
        if (!_animator)
            return;

        AnimationClip clip = null;
        _overridedAnimation.TryGetValue(name, out clip);

        if (clip != null)
        {
            AnimatorOverrideController controller = _animator.runtimeAnimatorController as AnimatorOverrideController;
            if (controller != null && clip != null)
            {
                controller[name] = clip;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public virtual AnimatorStateInfo GetCurrentAnimatorState()
    {
        if (_animator)
        {
            if (_animator.IsInTransition(0))
                return _animator.GetNextAnimatorStateInfo(0);

            return _animator.GetCurrentAnimatorStateInfo(0);
        }

        return default(AnimatorStateInfo);
    }

    public bool IsPvp()
    {
        XCameraFight fight = XCameraFight.GetSingleton();
        if (fight)
            return fight.IsPvp();

        return true;
    }

    private int GetEffectIndexByHeroStar(XEffectConfigObject effectConfig)
    {
        if (effectConfig.isDecideByStar && effectConfig.isDecideByStar == effectConfig.isDecideByEquipStar)
        {
            return _effectResIndex;
        }

        int index = _effectResIndex;
        if (effectConfig.isDecideByStar)
        {
            index = _heroStarsIndex;
        }
        else if (effectConfig.isDecideByEquipStar)
        {
            index = _heroEquipStarsIndex;
        }

        if (effectConfig.effectFiles.Length <= index)
        {
            index = effectConfig.effectFiles.Length - 1;
        }

        return index >= 0 ? index : 0;
    }

    private void MakeOffsetScale(XEffectComponent xeffect, AnimationEvent e)
    {
        string srcString = e.stringParameter;
        if (srcString == "" || !srcString.Contains("#"))
            return;

        float offsetParam = float.Parse(srcString.Split('#')[0]);
        float scaleX = float.Parse(srcString.Split('#')[1]);
        float scaleZ = float.Parse(srcString.Split('#')[2]);

        xeffect.transform.localScale = new Vector3(scaleX, scaleZ, scaleZ);
        Vector3 tempPos = xeffect.transform.localPosition;
        tempPos.x += offsetParam * transform.Find("shape").localScale.x;
        xeffect.transform.localPosition = tempPos;

        _warnXEffectId = xeffect.gid;

        XBoxMotifyAttackWarning warning = xeffect.GetComponentInChildren<XBoxMotifyAttackWarning>();
        if (warning != null)
        {
            warning.durtion = e.floatParameter;
            warning.elapse = 0;
            warning.isstart = true;
        }
    }

    protected void HandleAnimationEffect(AnimationEvent e)
    {
        XEffectConfigObject config = e.objectReferenceParameter as XEffectConfigObject;
        if (config == null) return;

        float time = e.floatParameter;
        string res = config.getRes(GetEffectIndexByHeroStar(config));
        if (!string.IsNullOrEmpty(res))
        {
            int animatorState = e.animatorStateInfo.fullPathHash;
            if (config.follow == XEffectComponent.EffectFollowType.Both)
            {

                XEffectManager.GenerateAsync(GetBoneNogPoint(config.linkBone).gameObject, res, time,
                    delegate (XEffectComponent xeffect)
                    {
                        if (xeffect != null)
                        {
                            if (IsPvp() == false)
                                MakeOffsetScale(xeffect, e);

                            if (gameObject != null)
                                AddFrameEffect(animatorState, config, xeffect);
                            else
                                xeffect.Stop();
                        }
                    });
            }
            else
            {
                Transform bone = GetBoneNogPoint(config.linkBone);
                Quaternion rot = _nogPoint.center.rotation;
                if (bone.transform.lossyScale.x < 0 && config.isIgnoreMorror)
                {
                    rot.eulerAngles += new Vector3(0f, -180f, 0f);
                }

                XEffectManager.GenerateAsync(bone.gameObject, bone.position, rot, res, time,
                    delegate (XEffectComponent xeffect)
                    {
                        if (xeffect == null) return;
                        if (gameObject != null)
                        {
                            if (!IsPvp())
                                MakeOffsetScale(xeffect, e);

                            if (config.follow == XEffectComponent.EffectFollowType.Position || config.follow == XEffectComponent.EffectFollowType.PositionButY)
                            {
                                xeffect.onlyFollowPosition = true;
                                if (config.follow == XEffectComponent.EffectFollowType.PositionButY)
                                {
                                    xeffect.ignoreFollowHeight = true;
                                    xeffect.followStayHeight = bone.position.y;
                                }
                                AddFrameEffect(animatorState, config, xeffect);
                                xeffect.Mirror(bone.transform.lossyScale.x < 0);
                            }
                            else
                            {
                                xeffect.onlyFollowPosition = false;
                            }
                        }
                        else
                            xeffect.Stop();
                    });
            }
        }
    }

    protected void AddFrameEffect(int status, XEffectConfigObject config, XEffectComponent xeffect)
    {
        xeffect.interruptType = config.interrupt;
        xeffect.freezeByAnimation = config.freezeByAnimation;
        xeffect.activedAnimatorState = status;

        List<XEffectComponent> list = null;
        _animatorFrameEffects.TryGetValue(status, out list);
        if (list == null)
        {
            list = new List<XEffectComponent>();
            _animatorFrameEffects.Add(status, list);
        }

        list.Add(xeffect);
    }

    protected void InterruptAllPreEffects(int previous, int current)
    {
        foreach (KeyValuePair<int, List<XEffectComponent>> item in _animatorFrameEffects)
        {
            if (item.Key != current)
            {
                List<XEffectComponent> list = item.Value;
                for (int i = 0; i < list.Count; i++)
                {
                    XEffectComponent xeffect = list[i];
                    if (xeffect != null && xeffect.gameObject.activeSelf)
                        xeffect.Interrupt(previous);
                }
                list.Clear();
            }
        }
    }
}
