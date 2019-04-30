using UnityEngine;
using System.Collections;

public class XDynamicPointLightInstance : MonoBehaviour
{
    public AnimationCurve Intensity;
    public float IntensityMultiplier = 1.0f;
    public float IntensityFallOffDistance = 1.0f;
    public UnityEngine.Gradient Gradient;
    public float CycleTime = 1.0f;
    public float Lifetime { get; private set; }
    public bool AutoPlay = true;
    public bool OneShot = true;
    public bool SimMode = false;
    public bool IsPlaying = false;

    float _PrevTick = 0f;

    // SR This is for the Matinee Cutscene, i need to be able to control the time independently
    // If you'd like to refactor this, lemme know..
    public void EnableSimMode(bool enable)
    {
        // Kick off the dynamic point light for Matinee!
        if (enable)
        {
            IsPlaying = false;
            SimMode = true;
            Play();
        }
        else
        {
            Stop();
        }
    }

    void OnEnable()
    {
        if (AutoPlay)
        {
            Reset();
            Play();
        }
    }

    void OnDisable()
    {
        if (IsPlaying)
        {
            Stop();
        }
    }

    public void Play()
    {
        if (IsPlaying)
        {
            return;
        }
        Reset();
        XDynamicPointLightManager.Instance.Register(this);
        IsPlaying = true;
    }

    public void Stop()
    {
        if (!IsPlaying)
        {
            return;
        }
        Reset();
        XDynamicPointLightManager.Instance.DeRegister(this);
        IsPlaying = false;
        if (OneShot)
            Destroy(this.gameObject);
    }

    public void Reset()
    {
        Lifetime = 0;
        _PrevTick = 0;
    }

    void OnDestroy()
    {
        if (IsPlaying)
        {
            Stop();
        }
    }

    void Update()
    {
        if (IsPlaying)
        {
            if (SimMode == false)
            {
                Sim(UnityEngine.Time.time);
            }
        }
    }

    // SR I want to be able to run 
    public void Sim(float tick)
    {
        if (!IsPlaying)
        {
            return;
        }

        float deltaTick = tick - _PrevTick;
        if (_PrevTick == 0)
        {
            deltaTick = 0;
        }
        //Debug.Log("Life Time " +Lifetime+ " CYCLE "+CycleTime+" DELTA " + tick + " prev " + _PrevTick + " delta " +deltaTick);
        _PrevTick = tick;
        Lifetime += deltaTick;

        if (OneShot && Lifetime > CycleTime)
        {
            Stop();
        }
    }
}