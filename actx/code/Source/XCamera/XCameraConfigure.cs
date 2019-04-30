using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum XCmaeraModLoopType
{
    Always,
    Once,
    Looping,
}

/// <summary>
/// 
/// </summary>
[CreateAssetMenu]
public class XCameraConfigure : ScriptableObject 
{
    public float fov = 40f;
    public float restFocusHeight = 0.9f;
    public float maxHeight = 8.2f;
    public float minHeight = 1.1f;
    public float maxRange = 12f;
    public float minRange = 4f;
    public float focusMoveSpeed = 0.4f;
    public float positionMoveSpeed = 0.05f;
    public float posHeightMod = 2.5f;
    public float sideBuffer = 1f;
    public float ry0 = 0;
    public float ry1 = 0;
    public float ryLerpSpeed = 0.1f;
    public float beginMaxDistanceModValue = 7f;
    public float endMaxDistanceModValue = 5f;
    public float beginMinDistanceModValue = 3f;
    public float endMinDiastanceModValue = 3.5f;
    public float airBorneHeight = 2.0f;
    public float airBornMaxHeight = 5.0f;
    public float wallBuffer = 2.0f;
    public float yDiffBoostMult = 1.0f;
    public float beginChangeSpeedValue = 50.0f;
    public float focusFastMoveSpeed = 0.8f;
    public float positionFastMoveSpeed = 0.8f;
    public float fieldOfViewMoveSpeed = 0.5f;
    public float focusOffset = 0f;
    public float followMinHeight = 100;
    public float followMaxHeight = 120;

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class ModClass
    {
        public string modName = "modCharge";
        public int modPriority = 1;
        public Vector2 focusMod = new Vector2(0f, 0f);
        public float heightMod = 0f;
        public float rangeMod = 2f;
        public float yawMod = 20f;
        public float wallBufferMod = 0.85f;
        public float fieldOfView = 40f;
        public XCmaeraModLoopType loopType = XCmaeraModLoopType.Always;
    }

    /// <summary>
    /// 
    /// </summary>
    public List<ModClass> myMods;

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class ShakeClass
    {
        public string shakeName = "";
        public int numberOfShakes = 3;
        public Vector3 shakeAmount = new Vector3(1f, 0.5f, 1f);
        public Vector3 rotationAmount = new Vector3(1f, 1f, 1f);
        public float distance = 0.1f;
        public float speed = 60f;
        public float decay = 0.2f;
        public float guiShakeMod = 1f;
        public bool multiplyByTimeScale = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public List<ShakeClass> myShakes;

    /// <summary>
    /// 
    /// </summary>
    public void init()
    {
        myMods = new List<ModClass>();
        ModClass mod1 = new ModClass();
        myMods.Add(mod1);

        myShakes = new List<ShakeClass>();
        ShakeClass shake1 = new ShakeClass();
        myShakes.Add(shake1);
    }

    Dictionary<string, ModClass> modsMap;
    Dictionary<string, ShakeClass> shakesMap;

    /// <summary>
    /// 
    /// </summary>
    public void Initialize()
    {
        modsMap = new Dictionary<string, ModClass>();
        for (int i = 0; i < myMods.Count; i++)
        {
            modsMap.Add(myMods[i].modName, myMods[i]);
        }

        shakesMap = new Dictionary<string, ShakeClass>();
        for (int i = 0; i < myShakes.Count; i++)
        {
            shakesMap.Add(myShakes[i].shakeName, myShakes[i]);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public ModClass GetMod(string name)
    {
        ModClass mod = null;
        modsMap.TryGetValue(name, out mod);
        return mod;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public ShakeClass GetShake(string name)
    {
        ShakeClass shake = null;
        shakesMap.TryGetValue(name, out shake);
        return shake;
    }
}
