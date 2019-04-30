using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class XDynamicPointLightManager : MonoBehaviour
{
    static XDynamicPointLightManager _this = null;
    static bool markDontReMake = false;

    private string _PointLightColorName = "_ACTPointLightColor";
    private string _PointLightPositionName = "_ACTPointLightPosition";
    private string _PointLightMultiplierName = "_ACTPointLightMultiplier";
    private string _PointLightIntensityName = "_ACTPointLightIntensity";

    private int _PointLightColorProperty = -1;
    private int _PointLightPositionProperty = -1;
    private int _PointLightMultiplierProperty = -1;
    private int _PointLightIntensityProperty = -1;

    public class Config
    {
        public delegate bool IsEnabled();
        public IsEnabled GetEnableHandler = null;
    }

    private static Config _config;

    public static void SetConfig(Config config)
    {
        _config = config;
    }

    public bool IsEnabled()
    {
        if (_config != null)
        {
            return _config.GetEnableHandler();
        }
        return true;
    }

    public static XDynamicPointLightManager Instance
    {
        get
        {
            if (!markDontReMake && _this == null)
            {
                InitializeInstance();
            }

            return _this;
        }
    }

    public static void InitializeInstance()
    {
        if (_this == null)
        {
            GameObject go = new GameObject("DynamicPointLightManager");
            _this = go.AddComponent<XDynamicPointLightManager>();
            DontDestroyOnLoad(go);
        }

        _this.Init();
    }

    public static bool IsInitialized()
    {
        return _this != null;
    }


    List<XDynamicPointLightInstance> lights = new List<XDynamicPointLightInstance>(4);

    public void Init()
    {
        _PointLightColorProperty = Shader.PropertyToID(_PointLightColorName);
        _PointLightPositionProperty = Shader.PropertyToID(_PointLightPositionName);
        _PointLightMultiplierProperty = Shader.PropertyToID(_PointLightMultiplierName);
        _PointLightIntensityProperty = Shader.PropertyToID(_PointLightIntensityName);
        lights = new List<XDynamicPointLightInstance>(4);
    }

    public void Sim()
    {
        if (!IsEnabled())
        {
            return;
        }

        Matrix4x4 lighting = new Matrix4x4();
        Matrix4x4 position = new Matrix4x4();
        Vector4 multiplier = new Vector4();
        Vector4 intensity = new Vector4();

        for (int i = 0; i < lights.Count; ++i)
        {
            XDynamicPointLightInstance light = lights[i];

            float t = (light.Lifetime % light.CycleTime) / light.CycleTime;


            Color col = light.Gradient.Evaluate(t);
            lighting.SetColumn(i, col);

            Vector3 pos = light.gameObject.transform.position;
            position.SetRow(i, new Vector4(pos.x, pos.y, pos.z, 0.0f));

            intensity[i] = light.Intensity.Evaluate(t) * light.IntensityMultiplier;

            float fallOff = Mathf.Max(0.01f, light.IntensityFallOffDistance);
            multiplier[i] = 25.0f / (fallOff * fallOff);
        }

        for (int i = lights.Count; i < 4; ++i)
        {
            position.SetRow(i, Vector4.zero);
            lighting.SetColumn(i, Vector4.zero);
            multiplier[i] = 0;
            intensity[i] = 1;
        }

        Shader.SetGlobalMatrix(_PointLightColorProperty, lighting);
        Shader.SetGlobalMatrix(_PointLightPositionProperty, position);
        Shader.SetGlobalVector(_PointLightMultiplierProperty, multiplier);
        Shader.SetGlobalVector(_PointLightIntensityProperty, intensity);
    }

    void Update()
    {
        Sim();
    }

    public void Register(XDynamicPointLightInstance light)
    {
        if (lights.Contains(light))
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            Debug.LogWarning("DynamicPointLight trying to register the same light!");
#endif
            return;
        }
        if (lights.Count < lights.Capacity)
        {
            lights.Add(light);
        }
#if UNITY_EDITOR || UNITY_STANDALONE
        else
        {
            Debug.LogWarning("Too many DynamicPointLights!");
        }
#endif
    }

    public void DeRegister(XDynamicPointLightInstance light)
    {
        if (lights.Contains(light))
        {
            //Debug.Log("light deregistered!");
        }
        lights.Remove(light);
    }

    public void DeRegisterAll()
    {
        lights.Clear();
    }

    private void OnApplicationQuit()
    {
        Destroy(gameObject);
        markDontReMake = true;
    }

#if UNITY_EDITOR
    public void Clear()
    {
        if (_this != null && _this.gameObject != null)
        {
            GameObject.DestroyImmediate(_this.gameObject);
        }
    }
#endif
}
