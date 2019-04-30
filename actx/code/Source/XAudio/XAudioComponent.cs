using UnityEngine;
using System.Collections;
using SLua;

[CustomLuaClass]
public class XAudioComponent : MonoBehaviour
{
    public enum AudioCategory
    {
        None,
        MaleNormal,
        FemaleNormal,
        MaleLight,
        FemaleLight,
        MaleHeavy,
        FemaleHeavy,
        MaleSuperHeavy,
        FemaleSuperHeavy
    };

    public enum VolumeLevel
    {
        Battle,
        Voice,
        UI,
        Music,
        VoiceSpecial
    };

    [System.Serializable]
    public class ClipProperty
    {
        public AudioClip clip;
        public AudioCategory category = AudioCategory.None;
        public int weight = 1;
        public int accWeight { get; set; }
    }

    public static bool globalMute = false;
    public static float globalVolume = 1;

    public AudioSource audioSource;
    [DoNotToLua]
    public ClipProperty[] clips;
    public bool autoPlay = true;
    public float delaySecond = .0f;

    public bool loop = false;
    public float playSecond = .0f;

    public bool autoDestroy = false;
    public bool autoDeactive = true;
    float elapseTime_ = .0f;
    bool played_ = false;

    int totalWeight_ = 0;

    double enableTime;

    AudioCategory category = AudioCategory.None;
    public bool useCategory = false;

    [Space(10)]
    [DoNotToLua]
    public VolumeLevel volumeLevel = VolumeLevel.Battle;
    private static float[] volumes = new float[] { 0.8f, 0.8f, 0.8f, 0.8f, 0.8f };

    [DoNotToLua, HideInInspector, System.NonSerialized]
    public float volumeAdjust = 1.0f;

    public int id = 0;
    public System.Action<int> finishCallback = null;

    void Awake()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = .0f;
        }
        else
        {
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = .0f;
        }

        CalculateWeight();
    }

    void CalculateWeight()
    {
        totalWeight_ = 0;
        if (clips != null)
        {
            for (int i = 0; i < clips.Length; i++)
            {
                ClipProperty property = clips[i];
                if (property.clip != null)
                {
                    bool calc = true;

                    if (property.category != AudioCategory.None)
                    {
                        useCategory = true;
                        if (property.category != category)
                            calc = false;
                    }


                    if (calc)
                    {
                        totalWeight_ += property.weight;
                        property.accWeight = totalWeight_;
                    }
                    else
                        property.accWeight = 0;
                }
                else
                {
                    property.weight = 0;
                    property.accWeight = 0;
                }
            }
        }
    }

    void OnEnable()
    {
        if (audioSource != null)
            audioSource.enabled = true;

        elapseTime_ = .0f;
        played_ = false;
        volumeAdjust = 1.0f;

        if (autoPlay && delaySecond == .0f)
            Play();
    }

    public void Play()
    {
        if (clips == null || audioSource == null) return;

        int random = Random.Range(0, totalWeight_);
        for (int i = 0; i < clips.Length; i++)
        {
            ClipProperty property = clips[i];
            if (random < property.accWeight && property.clip != null)
            {
                audioSource.clip = property.clip;
                audioSource.loop = loop;
                audioSource.volume = volumes[(int)volumeLevel] * globalVolume * volumeAdjust;

                if (!globalMute)
                    audioSource.Play();
                break;
            }
        }

        played_ = true;
    }

    public void SetCategory(AudioCategory cat)
    {
        if (category == cat) return;
        category = cat;
        CalculateWeight();
    }

    public void Stop()
    {
        if (audioSource.isPlaying)
            audioSource.Stop();

        if (autoDestroy)
            Destroy(gameObject);
        else
            if (autoDeactive)
            gameObject.SetActive(false);

        if (finishCallback != null)
            finishCallback(id);
    }

    // Update is called once per frame
    void Update()
    {
        if (played_)
        {
            if (loop && playSecond != .0f && elapseTime_ >= playSecond)
            {
                Stop();
            }
            else
            {
                if (!audioSource.isPlaying)
                    Stop();
            }
        }

        if (autoPlay && !played_)
        {
            // if( delaySecond == .0f )
            //     Play();
            // else
            if (elapseTime_ >= delaySecond)
            {
                Play();
                elapseTime_ = .0f;
            }
        }


        elapseTime_ += Time.deltaTime;
    }

    public static void SetVolumeLevelValue(VolumeLevel idx, float value)
    {
        volumes[(int)idx] = value;
    }

    public static float GetVolumeLevelValue(VolumeLevel idx)
    {
        return volumes[(int)idx];
    }
}
