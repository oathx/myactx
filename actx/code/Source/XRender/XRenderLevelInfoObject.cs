using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SLua;

[System.Serializable]
[CustomLuaClass]
public class XRenderLevelInfoObject : ScriptableObject
{
    public enum XPLATFORM
    {
        iOS = 0,
        Android = 1,
    }

    public const int PLATFORM_COUNT = 2;

    public const int ENVIRONMENT_COUNT = 2;

    public enum XMSAA
    {
        x1 = 1,
        x2 = 2,
        x4 = 4,
        x8 = 8,
    }

    public const int MSAA_COUNT = 4;

    public enum XANISOTROPIC
    {
        Disable = 0,
        Enable = 1,
        ForceEnable = 2,
    }

    public const int ANISOTROPIC_COUNT = 3;

    public enum XPARTICLE_QUALITY
    {
        Off = -1,
        Low = 0,
        Med = 1,
        High = 2,
    }

    public const int PARTICLE_QUALITY_COUNT = 4;

    public enum XPOSTFX_QUALITY
    {
        Off = -1,
        Low = 0,
        High = 1,
    }

    public const int POSTFX_QUALITY_COUNT = 3;

    public enum XPOSTFX
    {
        RadialBlur = 1,
        Bloom = 2,
        Warp = 4,
        Depth = 8,
        MotionBlur = 16,
    }

    public const int POSTFX_COUNT = 5;

    public enum XREFLECTION_QUALITY
    {
        Off = -1,
        Low = 0,
        High = 1,
    }

    public const int REFLECTION_QUALITY_COUNT = 3;

    public enum XBLEND_WEIGHTS
    {
        OneBone = 0,
        TwoBones = 1,
        FourBones = 4,
    }

    public const int BLEND_WEIGHTS_COUNT = 3;

    public enum XGLOBAL_QUALITY
    {
        Low = 0,
        Med = 1,
        High = 2,
    }
    public const int GLOBAL_QUALITY_COUNT = 3;

    public string ProfileName = "unloaded";

    public string ProfileId = "unloaded";

    public EnvironmentInfo GlobalEnvirInfo;

    [System.Serializable]
    [CustomLuaClass]
    public class EnvironmentInfo
    {
        public XMSAA msaa;
        //public eANISOTROPIC aniso; 
        public int lod;
        public int hiddenLayers;
        public XPARTICLE_QUALITY particleQuality;
        public XREFLECTION_QUALITY reflectionQuality;
        public int reflectedIgnoreLayers;
        public XPOSTFX_QUALITY postFXQuality;
        public XPOSTFX[] postFX;
        public XBLEND_WEIGHTS blendWeights;
        public XGLOBAL_QUALITY globalQualityLevel;

        public void SetDefaults()
        {
            msaa = XMSAA.x4;
            //aniso = eANISOTROPIC.Enable; 
            lod = 1300;
            hiddenLayers = 0;
            particleQuality = XPARTICLE_QUALITY.High;
            reflectionQuality = XREFLECTION_QUALITY.High;
            reflectedIgnoreLayers = 0;
            postFXQuality = XPOSTFX_QUALITY.High;
            postFX = new XPOSTFX[] { XPOSTFX.Bloom, XPOSTFX.RadialBlur, XPOSTFX.Warp };
            blendWeights = XBLEND_WEIGHTS.FourBones;
        }

        public void SetPostFX(int[] fxs)
        {
            postFX = new XPOSTFX[fxs.Length];
            for (int i = 0; i < fxs.Length; i++)
                postFX[i] = (XPOSTFX)fxs[i];
        }

        public void SetEnumPostFX(XPOSTFX[] fxs)
        {
            postFX = new XPOSTFX[fxs.Length];
            for (int i = 0; i < fxs.Length; i++)
                postFX[i] = (XPOSTFX)fxs[i];
        }

        public EnvironmentInfo Clone()
        {
            EnvironmentInfo info = new EnvironmentInfo();
            info.blendWeights = this.blendWeights;
            info.msaa = this.msaa;
            info.lod = this.lod;
            info.hiddenLayers = this.hiddenLayers;
            info.particleQuality = this.particleQuality;
            info.reflectionQuality = this.reflectionQuality;
            info.reflectedIgnoreLayers = this.reflectedIgnoreLayers;
            info.postFXQuality = this.postFXQuality;
            info.SetEnumPostFX(this.postFX);
            info.blendWeights = this.blendWeights;
            return info;
        }
    }


    private Dictionary<string, int> scenesToIndex = new Dictionary<string, int>();

    public int EnvironmentForScene(string sceneName)
    {
        if (scenesToIndex.ContainsKey(sceneName))
        {
            return scenesToIndex[sceneName];
        }
        return 0;
    }

    public XRenderLevelInfoObject()
    {
        GlobalEnvirInfo = new EnvironmentInfo();
    }
}
