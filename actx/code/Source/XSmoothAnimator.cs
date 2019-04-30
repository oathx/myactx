using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SLua;

[CustomLuaClass]
public class XSmoothAnimator : MonoBehaviour {
    private Animator _eventAnimator;
    private Animator _avatarAnimator;
    private bool _autoUpdate = false;

    /// <summary>
    /// 
    /// </summary>
    bool InitAnimator()
    {
        _avatarAnimator = GetComponent<Animator>();
        if (!_avatarAnimator)
            return false;

        if (!_autoUpdate)
        {
            Transform shape = transform.Find(XActorElementName.shape.ToString());
            if (shape)
            {
                shape.gameObject.AddComponent<XModelEventCB>();

                _eventAnimator = shape.gameObject.GetComponent<Animator>();
                if (!_eventAnimator)
                    _eventAnimator = shape.gameObject.AddComponent<Animator>();

                AnimatorOverrideController overrideClone = Object.Instantiate(_avatarAnimator.runtimeAnimatorController) as AnimatorOverrideController;
                _eventAnimator.runtimeAnimatorController = overrideClone;
                _eventAnimator.Rebind();

                _eventAnimator.avatar = null;
            }
        }

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        InitAnimator();
    }

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        _eventAnimator.enabled = false;
        _avatarAnimator.enabled = false;
    }

    private void Update()
    {
        if (_autoUpdate)
        {
            if (_eventAnimator)
            {
                _eventAnimator.Update(Time.deltaTime);
            }

            Render(Time.deltaTime);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="autoUpdate"></param>
    public void SetAutoUpdate(bool autoUpdate)
    {
        if (_autoUpdate != autoUpdate)
        {
            _autoUpdate = autoUpdate;

            InitAnimator();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool IsAutoUpdate()
    {
        return _autoUpdate;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Animator GetEventAnimator()
    {
        return _eventAnimator;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Animator GetAvatarAnimator()
    {
        return _avatarAnimator;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="deltaTime"></param>
    public void Render(float deltaTime)
    {
        if (_avatarAnimator)
        {
            _avatarAnimator.Update(deltaTime);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fDeltaTime"></param>
    public void Update(float fDeltaTime)
    {
        if (_eventAnimator)
        {
            _eventAnimator.Update(fDeltaTime);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public void SetBool(string name, bool value)
    {
        if (_avatarAnimator)
        {
            _avatarAnimator.SetBool(name, value);
        }
        if (_eventAnimator)
        {
            _eventAnimator.SetBool(name, value);
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="value"></param>
    public void SetBool(int id, bool value)
    {
        if (_avatarAnimator)
        {
            _avatarAnimator.SetBool(id, value);
        }
        if (_eventAnimator)
        {
            _eventAnimator.SetBool(id, value);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool GetBool(string name)
    {
        return _eventAnimator.GetBool(name);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool GetBool(int id)
    {
        return _eventAnimator.GetBool(id);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public void SetFloat(string name, float value)
    {
        if (_avatarAnimator)
        {
            _avatarAnimator.SetFloat(name, value);
        }
        if (_eventAnimator)
        {
            _eventAnimator.SetFloat(name, value);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="dampTime"></param>
    /// <param name="deltaTime"></param>
    public void SetFloat(string name, float value, float dampTime, float deltaTime)
    {
        if (_avatarAnimator)
        {
            _avatarAnimator.SetFloat(name, value, dampTime, deltaTime);
        }
        if (_eventAnimator)
        {
            _eventAnimator.SetFloat(name, value, dampTime, deltaTime);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="value"></param>
    public void SetFloat(int id, float value)
    {
        if (_avatarAnimator)
        {
            _avatarAnimator.SetFloat(id, value);
        }
        if (_eventAnimator)
        {
            _eventAnimator.SetFloat(id, value);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="value"></param>
    /// <param name="dampTime"></param>
    /// <param name="deltaTime"></param>
    public void SetFloat(int id, float value, float dampTime, float deltaTime)
    {
        if (_avatarAnimator)
        {
            _avatarAnimator.SetFloat(id, value, dampTime, deltaTime);
        }
        if (_eventAnimator)
        {
            _eventAnimator.SetFloat(id, value, dampTime, deltaTime);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public float GetFloat(string name)
    {
        return _eventAnimator.GetFloat(name);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public float GetFloat(int id)
    {
        return _eventAnimator.GetFloat(id);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public void SetInteger(string name, int value)
    {
        if (_avatarAnimator)
        {
            _avatarAnimator.SetInteger(name, value);
        }
        if (_eventAnimator)
        {
            _eventAnimator.SetInteger(name, value);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="value"></param>
    public void SetInteger(int id, int value)
    {
        if (_avatarAnimator)
        {
            _avatarAnimator.SetInteger(id, value);
        }
        if (_eventAnimator)
        {
            _eventAnimator.SetInteger(id, value);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public int GetInteger(string name)
    {
        return _eventAnimator.GetInteger(name);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public int GetInteger(int id)
    {
        return _eventAnimator.GetInteger(id);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="layerName"></param>
    /// <returns></returns>
    public int GetLayerIndex(string layerName)
    {
        return _eventAnimator.GetLayerIndex(layerName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="layerIndex"></param>
    /// <returns></returns>
    public string GetLayerName(int layerIndex)
    {
        return _eventAnimator.GetLayerName(layerIndex);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="layerIndex"></param>
    /// <returns></returns>
    public AnimatorClipInfo[] GetNextAnimatorClipInfo(int layerIndex)
    {
        return _eventAnimator.GetNextAnimatorClipInfo(layerIndex);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="layerIndex"></param>
    /// <returns></returns>
    public AnimatorStateInfo GetNextAnimatorStateInfo(int layerIndex)
    {
        return _eventAnimator.GetNextAnimatorStateInfo(layerIndex);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="layerIndex"></param>
    /// <returns></returns>
    public bool IsInTransition(int layerIndex)
    {
        return _eventAnimator.IsInTransition(layerIndex);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="layerIndex"></param>
    /// <returns></returns>
    public AnimatorClipInfo[] GetCurrentAnimatorClipInfo(int layerIndex)
    {
        return _eventAnimator.GetCurrentAnimatorClipInfo(layerIndex);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="layerIndex"></param>
    /// <returns></returns>
    public AnimatorStateInfo GetCurrentAnimatorStateInfo(int layerIndex)
    {
        return _eventAnimator.GetCurrentAnimatorStateInfo(layerIndex);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="humanBoneId"></param>
    /// <returns></returns>
    public Transform GetBoneTransform(HumanBodyBones humanBoneId)
    {
        return _avatarAnimator.GetBoneTransform(humanBoneId);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public AnimatorControllerParameter GetParameter(int index)
    {
        return _eventAnimator.GetParameter(index);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="layerIndex"></param>
    /// <param name="stateID"></param>
    /// <returns></returns>
    public bool HasState(int layerIndex, int stateID)
    {
        return _eventAnimator.HasState(layerIndex, stateID);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stateName"></param>
    public void Play(string stateName)
    {
        if (_avatarAnimator)
        {
            _avatarAnimator.Play(stateName);
        }
        if (_eventAnimator)
        {
            _eventAnimator.Play(stateName);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    public void Play(int state)
    {
        if (_avatarAnimator)
        {
            _avatarAnimator.Play(state);
        }
        if (_eventAnimator)
        {
            _eventAnimator.Play(state);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    /// <param name="layer"></param>
    public void Play(int state, int layer)
    {

        if (_avatarAnimator)
        {
            _avatarAnimator.Play(state, layer);
        }
        if (_eventAnimator)
        {
            _eventAnimator.Play(state, layer);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="layer"></param>
    public void Play(string stateName, int layer)
    {

        if (_avatarAnimator)
        {
            _avatarAnimator.Play(stateName, layer);
        }
        if (_eventAnimator)
        {
            _eventAnimator.Play(stateName, layer);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="layer"></param>
    /// <param name="normalizedTime"></param>
    public void Play(string stateName, int layer, float normalizedTime)
    {
        if (_avatarAnimator)
        {
            _avatarAnimator.Play(stateName, layer, normalizedTime);
        }

        if (_eventAnimator)
        {
            _eventAnimator.Play(stateName, layer, normalizedTime);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stateNameHash"></param>
    /// <param name="layer"></param>
    /// <param name="normalizedTime"></param>
    public void Play(int stateNameHash, int layer, float normalizedTime)
    {
        if (_avatarAnimator)
        {
            _avatarAnimator.Play(stateNameHash, layer, normalizedTime);
        }

        if (_eventAnimator)
        {
            _eventAnimator.Play(stateNameHash, layer, normalizedTime);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stateNameHash"></param>
    /// <param name="transitionDuration"></param>
    public void CrossFade(int stateNameHash, float transitionDuration)
    {
        if (_eventAnimator)
        {
            _eventAnimator.CrossFade(stateNameHash, transitionDuration);
        }

        if (_avatarAnimator)
        {
            _avatarAnimator.CrossFade(stateNameHash, transitionDuration);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="transitionDuration"></param>
    public void CrossFade(string stateName, float transitionDuration)
    {
        if (_eventAnimator)
        {
            _eventAnimator.CrossFade(stateName, transitionDuration);
        }

        if (_avatarAnimator)
        {
            _avatarAnimator.CrossFade(stateName, transitionDuration);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="transitionDuration"></param>
    /// <param name="layer"></param>
    public void CrossFade(string stateName, float transitionDuration, int layer)
    {
        if (_eventAnimator)
        {
            _eventAnimator.CrossFade(stateName, transitionDuration, layer);
        }

        if (_avatarAnimator)
        {
            _avatarAnimator.CrossFade(stateName, transitionDuration, layer);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stateNameHash"></param>
    /// <param name="transitionDuration"></param>
    /// <param name="layer"></param>
    public void CrossFade(int stateNameHash, float transitionDuration, int layer)
    {
        if (_eventAnimator)
        {
            _eventAnimator.CrossFade(stateNameHash, transitionDuration, layer);
        }

        if (_avatarAnimator)
        {
            _avatarAnimator.CrossFade(stateNameHash, transitionDuration, layer);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="transitionDuration"></param>
    /// <param name="layer"></param>
    /// <param name="normalizedTime"></param>
    public void CrossFade(string stateName, float transitionDuration, int layer, float normalizedTime)
    {
        if (_eventAnimator)
        {
            _eventAnimator.CrossFade(stateName, transitionDuration, layer);
        }

        if (_avatarAnimator)
        {
            _avatarAnimator.CrossFade(stateName, transitionDuration, layer);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stateNameHash"></param>
    /// <param name="transitionDuration"></param>
    /// <param name="layer"></param>
    /// <param name="normalizedTime"></param>
    public void CrossFade(int stateNameHash, float transitionDuration, int layer, float normalizedTime)
    {
        if (_eventAnimator)
        {
            _eventAnimator.CrossFade(stateNameHash, transitionDuration, layer, normalizedTime);
        }

        if (_avatarAnimator)
        {
            _avatarAnimator.CrossFade(stateNameHash, transitionDuration, layer, normalizedTime);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    public void ResetTrigger(string name)
    {
        if (_avatarAnimator)
        {
            _avatarAnimator.ResetTrigger(name);
        }
        if (_eventAnimator)
        {
            _eventAnimator.ResetTrigger(name);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public void ResetTrigger(int id)
    {

        if (_avatarAnimator)
        {
            _avatarAnimator.ResetTrigger(id);
        }
        if (_eventAnimator)
        {
            _eventAnimator.ResetTrigger(id);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    public void SetTrigger(string name)
    {
        if (_avatarAnimator)
        {
            _avatarAnimator.SetTrigger(name);
        }
        if (_eventAnimator)
        {
            _eventAnimator.SetTrigger(name);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public void SetTrigger(int id)
    {
        if (_avatarAnimator)
        {
            _avatarAnimator.SetTrigger(id);
        }
        if (_eventAnimator)
        {
            _eventAnimator.SetTrigger(id);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static int StringToHash(string name)
    {
        return Animator.StringToHash(name);
    }

    /// <summary>
    /// 
    /// </summary>
    public new bool enabled
    {
        get
        {
            return _avatarAnimator.enabled;
        }

        set
        {
            _avatarAnimator.enabled = value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public float speed
    {
        get
        {
            return _eventAnimator.speed;
        }

        set
        {
            _eventAnimator.speed = value;
            _avatarAnimator.speed = value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="layerIndex"></param>
    /// <returns></returns>
    public int GetEventStateHash(int layerIndex)
    {
        AnimatorStateInfo state;
        if (_eventAnimator.IsInTransition(layerIndex))
        {
            state = _eventAnimator.GetNextAnimatorStateInfo(layerIndex);
        }
        else
        {
            state = _eventAnimator.GetCurrentAnimatorStateInfo(layerIndex);
        }

        return state.shortNameHash;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="layerIndex"></param>
    /// <returns></returns>
    public int GetAvatarStateHash(int layerIndex)
    {
        AnimatorStateInfo state;
        if (_avatarAnimator.IsInTransition(layerIndex))
        {
            state = _avatarAnimator.GetNextAnimatorStateInfo(layerIndex);
        }
        else
        {
            state = _avatarAnimator.GetCurrentAnimatorStateInfo(layerIndex);
        }

        return state.shortNameHash;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="layerIndex"></param>
    public void PlaySync(int layerIndex)
    {
        AnimatorStateInfo state;
        if (_eventAnimator.IsInTransition(layerIndex))
        {
            state = _eventAnimator.GetNextAnimatorStateInfo(layerIndex);
        }
        else
        {
            state = _eventAnimator.GetCurrentAnimatorStateInfo(layerIndex);
        }

        _avatarAnimator.Play(state.shortNameHash, layerIndex, state.normalizedTime);
    }
}
