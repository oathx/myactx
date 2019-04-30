using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// control particle performance,off,low,high
// control param : emit rate, start size, rotation, 
[ExecuteInEditMode]
public class XParticleLevel : MonoBehaviour
{
    public enum eQUALITY
    {
        Off = -1,
        Low = 0,
        Med = 1,
        High = 2,
    }

    public enum ePARAMETER
    {
        None,
        EmissionRate,
        MaxParticles,
        GravityMultiplier,
        StartingColor,
        StartingLifeSpan,
        StartingRotation,
        StartingSize,
        StartingSpeed
    }

    public enum eTRIGGER
    {
        Constant,
        Height,
    }

    public enum eTUNING
    {
        Constant,
        Curve,
    }

    public static int PARAMETER_COUNT = Enum.GetValues(typeof(ePARAMETER)).Length;
    public static int TRIGGER_COUNT = Enum.GetValues(typeof(eTRIGGER)).Length;
    public static int TUNING_COUNT = Enum.GetValues(typeof(eTUNING)).Length;
    public static int QUALITY_COUNT = Enum.GetValues(typeof(eQUALITY)).Length - 1; // CAN NOT debug for "Off"

    public class Config
    {
        public delegate eQUALITY GetParticleQuality();
        public GetParticleQuality GetQualityHandler = null;
    }

    private static Config _config;

    public static void SetConfig(Config config)
    {
        _config = config;
    }

    public eQUALITY GetQuality()
    {
        if (_config != null)
        {
            return _config.GetQualityHandler();
        }
        return eQUALITY.High;
    }

    #region Serializable Classes used for Serialized data
    [System.Serializable]
    public class Condition
    {
        public bool expanded; // for inspector
        public ePARAMETER parameter;
        public eTRIGGER trigger;

        [System.Serializable]
        public class Tuning
        {
            public eTUNING tuningType;
            public float constant;
            public Color constantColor;
            public AnimationCurve curve;
            public float minX;
            public float minY;
            public float maxX;
            public float maxY;
            public Color minColor;
            public Color maxColor;

            public Tuning()
            {
                tuningType = eTUNING.Constant;
                constant = 0.0f;
                constantColor = Color.white;
                curve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
                minX = 0.0f;
                minY = 0.0f;
                maxX = 10.0f;
                maxY = 10.0f;
                minColor = Color.white;
                maxColor = Color.white;
            }
        }

        public Tuning[] tunings;

        public Condition()
        {
            expanded = true;
            parameter = ePARAMETER.None;
            trigger = eTRIGGER.Constant;
            tunings = new Condition.Tuning[XParticleLevel.QUALITY_COUNT];
            for (var i = 0; i < XParticleLevel.QUALITY_COUNT; ++i)
            {
                tunings[i] = new Condition.Tuning();
            }
        }
    }
    #endregion   //Serializable Classes used for Serialized data

    [SerializeField]
    private ParticleSystem _particleSystem;
    public List<Condition> Conditions = new List<Condition>();
    public List<bool> IsQualitysEnabled = new List<bool>();

    void Awake()
    {
        if (IsQualitysEnabled == null || IsQualitysEnabled.Count != XParticleLevel.QUALITY_COUNT)
        {
            IsQualitysEnabled = new List<bool>() { true, true, true };
        }
    }

    void Start()
    {
        if (Application.isPlaying)
        {
            _quality = GetQuality();
            if (DestroyIfPossible(gameObject))
            {
                return;
            }
        }

        _particleSystem = this.gameObject.GetComponent<ParticleSystem>();

        if (Conditions == null)
        {
            Conditions = new List<Condition>();
        }
    }

    [System.NonSerialized]
    private eQUALITY _quality = eQUALITY.High;

    void Update()
    {
        if (_particleSystem == null) return;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            _quality = XParticlePreview.Quality;
            this.GetComponent<Renderer>().enabled = (_quality != eQUALITY.Off) && IsQualitysEnabled[(int)_quality];
        }
        else
        {
            _quality = GetQuality();
            this.GetComponent<Renderer>().enabled = (_quality != eQUALITY.Off) && IsQualitysEnabled[(int)_quality];
        }
#endif


        bool allConstant = true;

        foreach (Condition condition in Conditions)
        {
            Condition.Tuning tuning = condition.tunings[(int)_quality];

            if (condition.trigger == eTRIGGER.Constant || tuning.tuningType == eTUNING.Constant)
            {
                if (condition.parameter == ePARAMETER.StartingColor)
                {
                    SetParameter(condition.parameter, tuning.constantColor);
                }
                else
                {
                    SetParameter(condition.parameter, tuning.constant);
                }
                continue;
            }

            allConstant = false;

            // if height
            var triggerValue = 0.0f;
            switch (condition.trigger)
            {
                case (eTRIGGER.Height):
                    triggerValue = this.transform.position.y;
                    break;
                default:
                    break;
            }

            float interpolateKey = 0.0f;

            //if curve
            switch (tuning.tuningType)
            {
                case (eTUNING.Curve):
                    AnimationCurve curve = tuning.curve;
                    interpolateKey = curve.Evaluate((triggerValue - tuning.minX) / (tuning.maxX - tuning.minX));
                    break;
                default:
                    break;
            }

            switch (condition.parameter)
            {
                case (ePARAMETER.StartingColor):
                    Color interpolatedColor = Color.Lerp(tuning.minColor, tuning.maxColor, interpolateKey);
                    SetParameter(condition.parameter, interpolatedColor);
                    break;

                default:
                    float interpolatedFloat = interpolateKey * (tuning.maxY - tuning.minY) + tuning.minY;
                    SetParameter(condition.parameter, interpolatedFloat);
                    break;
            }
        }

        if (allConstant && Application.isPlaying)
        {
            this.enabled = false;
        }
    }

    private void SetParameter(ePARAMETER parameter, float val)
    {
        if (_particleSystem == null)
        {
            Debug.LogError("there is NO particle system!");
            return;
        }

        ParticleSystem.MainModule mainModule = _particleSystem.main;
        ParticleSystem.EmissionModule emissModule = _particleSystem.emission;
        switch (parameter)
        {
            case (ePARAMETER.EmissionRate):
                emissModule.rate = new ParticleSystem.MinMaxCurve(val);
                break;
            case (ePARAMETER.MaxParticles):
                mainModule.maxParticles = (int)val;
                break;
            case (ePARAMETER.StartingLifeSpan):
                mainModule.startLifetime = val;
                break;
            case (ePARAMETER.StartingSize):
                mainModule.startSize = val;
                break;
            case (ePARAMETER.StartingSpeed):
                mainModule.startSpeed = val;
                break;
            case (ePARAMETER.StartingRotation):
                mainModule.startRotation = val;
                break;
            case (ePARAMETER.GravityMultiplier):
                mainModule.gravityModifier = val;
                break;
            case (ePARAMETER.None):
                break;
            default:
                break;
        }
    }

    private void SetParameter(ePARAMETER parameter, Color val)
    {
        if (_particleSystem == null)
        {
            Debug.LogError("there is NO particle system!");
            return;
        }

        ParticleSystem.MainModule mainModule = _particleSystem.main;
        switch (parameter)
        {
            case (ePARAMETER.StartingColor):
                mainModule.startColor = val;
                break;
            default:
                break;
        }
    }

    public static bool DisabledByParticleLevel(GameObject gameObject)
    {
        XParticleLevel particleLevel = gameObject.GetComponent<XParticleLevel>();

        if (particleLevel == null)
        {
            return false;
        }

        if (particleLevel.IsQualitysEnabled == null || particleLevel.IsQualitysEnabled.Count != XParticleLevel.QUALITY_COUNT)
        {
            particleLevel.IsQualitysEnabled = new List<bool>() { true, true, true };
        }

        var quality = particleLevel.GetQuality();
        return (quality == eQUALITY.Off) || !particleLevel.IsQualitysEnabled[(int)quality];
    }

    public static bool CanDelete(GameObject gameObject)
    {
        if (!DisabledByParticleLevel(gameObject))
        {
            return false;
        }

        bool canDelete = true;

        for (int i = 0; i < gameObject.transform.childCount; ++i)
        {
            canDelete &= CanDelete(gameObject.transform.GetChild(i).gameObject);
        }

        return canDelete;
    }

    // delete particle when quality in particle level on it NOT meets global control quality
    private static bool DestroyIfPossible(GameObject gameObject)
    {
        XParticleLevel particleLevel = gameObject.GetComponent<XParticleLevel>();

        if (particleLevel == null)
        {
            return false;
        }
        bool isDestroyOrDisable = false;

        if (CanDelete(gameObject))
        {
            Destroy(gameObject);
            isDestroyOrDisable = true;
        }
        else
        {
            var quality = particleLevel.GetQuality();
            if ((quality == eQUALITY.Off) || !particleLevel.IsQualitysEnabled[(int)quality])
            {
                ParticleSystem particle = gameObject.GetComponent<ParticleSystem>();
                if (particle != null)
                {
                    particle.Pause();
                    ParticleSystem.EmissionModule emi = particle.emission;
                    emi.enabled = false;
                    particleLevel.enabled = false;
                }
                isDestroyOrDisable = true;
            }
        }
        return isDestroyOrDisable;
    }

    //use for editor
#if UNITY_EDITOR
    public void AddCondition()
    {
        Conditions.Add(new Condition());
    }

    public void RemoveCondition(Condition condition)
    {
        Conditions.Remove(condition);
    }
#endif

}
