using UnityEngine;
using System.Collections.Generic;
using SLua;

[CustomLuaClass]
public class XEffectComponent : MonoBehaviour
{

    [System.Serializable]
    public class EffectAudioClip
    {
        public GameObject audioClip;
    }

    [System.Serializable]
    public class EffectAdjustor
    {
        public float scale = 1.0f;
        public Vector3 offsetPos = Vector3.zero;
    }

    public enum EffectEventType
    {
        FINISHED,
        STOPED,
        TRIGGER_NEXT,
    };

    public enum EffectFollowType
    {
        None, Both, Position, PositionButY,
    };

    public delegate void EffectEventHandler(XEffectComponent effect, EffectEventType eventType);
    public EffectEventHandler eventCallBack;

    public int gid = 0;

    public float playTime = 5.0f;
    public float triggerNextTime = 3.0f;
    public bool isOneShot = false;
    public bool keepLooping = false;

    public bool forceUnfollow = false;
    [HideInInspector, System.NonSerialized]
    public bool onlyFollowPosition = false;
    [HideInInspector, System.NonSerialized]
    public bool ignoreFollowHeight = false;
    [HideInInspector, System.NonSerialized]
    public float followStayHeight = .0f;

    float _startTime;
    bool _reachTrigger = false;

    ParticleSystem[] _shurikens;
    Animation[] _anims;

    public List<EffectAudioClip> audioClips;

    List<AudioSource> _audioSources = new List<AudioSource>();

    [DoNotToLua]
    public const float LOOP_TIME = -1.0f;

    [HideInInspector]
    public GameObject owner;
    [HideInInspector]
    public string resPath;

    public enum InterruptType { None, Stop, Destory }
    [HideInInspector, System.NonSerialized]
    public InterruptType interruptType = InterruptType.None;
    [HideInInspector, System.NonSerialized]
    public int activedAnimatorState = 0;
    [HideInInspector, System.NonSerialized]
    public bool freezeByAnimation = true;

    void Awake()
    {
        _shurikens = gameObject.GetComponentsInChildren<ParticleSystem>();
        _anims = gameObject.GetComponentsInChildren<Animation>();

        initEffectAudio();
    }

    void OnEnable()
    {
        _startTime = Time.time;
        interruptType = InterruptType.None;
        activedAnimatorState = 0;
        freezeByAnimation = true;
    }

    void OnDestroy()
    {
        if (gid > 0)
            XEffectManager.Instance.DestroyEffect(this, true);
    }

    public void Initialize(GameObject obj, Vector3 position, Quaternion rotation, GameObject effectPrefab, float time)
    {
        transform.position = position;
        transform.rotation = rotation;
        transform.SetParent(null);
        transform.Rotate(effectPrefab.transform.rotation.eulerAngles);
        transform.Translate(effectPrefab.transform.position, Space.World);
        transform.localScale = effectPrefab.transform.localScale;

        playTime = time;

        if (time < 0)
            keepLooping = true;
        else
            keepLooping = false;

        if (time == 0)
            isOneShot = true;
        else
            isOneShot = false;

        owner = obj;
    }

    public void Initialize(GameObject parent, GameObject effectPrefab, float time)
    {
        if (parent != null)
        {
            transform.position = parent.transform.position;
            if (onlyFollowPosition)
                transform.rotation = effectPrefab.transform.rotation;
            else
                transform.rotation = parent.transform.rotation;
        }

        transform.Rotate(effectPrefab.transform.rotation.eulerAngles);
        if (parent != null)
            transform.SetParent((forceUnfollow || onlyFollowPosition) ? null : parent.transform);
        transform.localScale = effectPrefab.transform.localScale;

        playTime = time;

        if (time < 0)
            keepLooping = true;
        else
            keepLooping = false;

        if (time == 0)
            isOneShot = true;
        else
            isOneShot = false;

        owner = parent;
    }

    public void ApplyAdjustor(EffectAdjustor adjustor)
    {
        ApplyScale(adjustor.scale);
        ApplyPosOffset(adjustor.offsetPos);
    }

    public void Pause(bool pause)
    {
        for (int i = 0; i < _shurikens.Length; i++)
        {
            ParticleSystem ps = _shurikens[i];
            if (ps != null)
            {
                if (pause && ps.isPlaying)
                    ps.Pause();
                if (!pause && ps.isPaused)
                    ps.Play();
            }
        }

        if (_anims != null)
        {
            for (int i = 0; i < _anims.Length; i++)
            {
                Animation anim = _anims[i];
                string name = anim.clip.name;
                anim[name].speed = pause ? 0 : 1;
            }
        }
    }

    void initEffectAudio()
    {
        for (int i = 0; i < audioClips.Count; ++i)
        {
            if (audioClips[i].audioClip != null)
            {
                GameObject audioCom = Instantiate<GameObject>(audioClips[i].audioClip);
                audioCom.transform.SetParent(gameObject.transform);
                audioCom.GetComponent<XAudioComponent>().autoDeactive = false;
            }
            else
                GLog.Log(string.Format("<color=orange>Effect lose audio prefab</color>"));
        }
    }

    public void PlayAudio(string name)
    {
        foreach (AudioSource audioSource in _audioSources)
        {
            if (audioSource.clip.name.Equals(name))
                audioSource.Play();
        }
    }

    public void ApplyPosOffset(Vector3 offset)
    {
        transform.Translate(offset, Space.World);
    }

    public void ApplyScale(float _scale)
    {
        applyTransformScale(_scale);
        applyParticleSizeScale(_scale);
    }

    void applyTransformScale(float _scale)
    {
        transform.localScale *= _scale;
    }

    void applyParticleSizeScale(float _scale)
    {
        for (int i = 0; i < _shurikens.Length; i++)
        {
            ParticleSystem ps = _shurikens[i];
            if (ps != null)
            {
                ps.startSize *= _scale;
                ps.startSpeed *= _scale;
            }
        }
    }

    void Update()
    {
        if (isOneShot)
        {

            if (oneShotParticleSystemFinished())
            {
                if (eventCallBack != null)
                    eventCallBack(this, EffectEventType.FINISHED);
                Destroy();
            }
        }
        else
        {
            float elapseTime = Time.time - _startTime;

            if (!_reachTrigger && elapseTime > triggerNextTime)
            {
                _reachTrigger = true;

                if (eventCallBack != null)
                    eventCallBack(this, EffectEventType.TRIGGER_NEXT);
            }

            if (elapseTime > playTime && !keepLooping)
            {
                if (eventCallBack != null)
                    eventCallBack(this, EffectEventType.FINISHED);
                Destroy();
            }
        }
    }

    void LateUpdate()
    {
        if (owner != null)
        {
            if (onlyFollowPosition)
            {
                if (ignoreFollowHeight)
                    transform.position = new Vector3(owner.transform.position.x, followStayHeight, owner.transform.position.z);
                else
                    transform.position = owner.transform.position;
            }

            if (!owner.activeInHierarchy)
                Destroy();
        }

    }

    public void Stop()
    {
        if (eventCallBack != null)
            eventCallBack(this, EffectEventType.STOPED);

        Destroy();
    }

    void Destroy()
    {
        if (!string.IsNullOrEmpty(resPath))
            XEffectManager.Instance.DestroyEffect(this);
        else
            gameObject.SetActive(false);
    }

    bool oneShotParticleSystemFinished()
    {
        bool isFinished = true;
        for (int i = 0; i < _shurikens.Length; i++)
        {
            if (_shurikens[i] != null)
            {
                if (_shurikens[i].isPlaying)
                    return false;

                isFinished &= !_shurikens[i].isPlaying;
            }
        }

        if (isFinished)
        {
            for (int i = 0; i < _shurikens.Length; i++)
            {
                if (_shurikens[i] != null && _shurikens[i].IsAlive())
                    return false;
            }
        }
        return isFinished;
    }

    public void Interrupt(int animatorState)
    {
        if (interruptType == InterruptType.None)
            return;

        switch (interruptType)
        {
            case InterruptType.Stop:

            case InterruptType.Destory:
                Stop();
                break;
            default:
                break;
        }
    }

    public void Mirror(bool apply)
    {
        XEffectAxisMirror[] mirros = GetComponentsInChildren<XEffectAxisMirror>();
        for (int i = 0; i < mirros.Length; i++)
        {
            mirros[i].Apply(apply);
        }
    }

    public void SetPlaybackSpeed(float speed)
    {
        for (int i = 0; i < _shurikens.Length; i++)
        {
            ParticleSystem ps = _shurikens[i];
            if (ps != null)
                ps.playbackSpeed = speed;
        }
    }
}
