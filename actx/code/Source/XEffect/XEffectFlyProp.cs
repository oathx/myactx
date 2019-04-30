using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SLua;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif
[CustomLuaClass]
public class XEffectFlyProp : XBulletComponent
{
    public GameObject OffsetEffectPrefab;
    public GameObject PacManEffectPrefab;
    private Transform[] _fliesTransInScene;
    public void RefreshFlyPropsInScene(XEffectFlyProp[] flies)
    {
        if (flies != null)
        {
            int len = flies.Length;
            _fliesTransInScene = new Transform[len];
            for (int i = 0; i < flies.Length; i++)
            {
                Transform trans = null;
                if (flies[i] != null)
                {
                    trans = flies[i].flyEffect_.transform;
                }
                else
                    GLog.Log("fly is null in index : " + i.ToString());
                _fliesTransInScene[i] = trans;
            }
        }
        else
            GLog.Log("RefreshFlyPropsInScene  no flies!");
    }

    protected override bool HitDetection()
    {
        if (base.HitDetection())
            return true;

        if (_fliesTransInScene != null)
        {
            for (int i = 0; i < _fliesTransInScene.Length; i++)
            {
                if (_fliesTransInScene[i] != null)
                {
                    if (_fliesTransInScene[i] != this.GetFlyEffectTrans())
                    {
                        Transform trans = _fliesTransInScene[i].transform;
                        bool hit = this.IsIntersect(trans);

                        if (hit)
                        {
                            if (OnHit(trans))
                            {
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    GLog.Log("Fly Prop missing??");
                }
            }
        }
        return false;
    }

    [DoNotToLua]
    public override bool CallHitDetection(List<Transform> transList)
    {
        if (base.CallHitDetection(transList))
            return true;

        if (_fliesTransInScene != null)
        {
            for (int i = 0; i < _fliesTransInScene.Length; i++)
            {
                if (_fliesTransInScene[i] != null)
                {
                    if (_fliesTransInScene[i] != this.GetFlyEffectTrans())
                    {
                        Transform trans = _fliesTransInScene[i].transform;
                        if (transList.Contains(trans))
                        {
                            if (OnHit(trans))
                            {
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    GLog.Log("Fly Prop missing??");
                }
            }
        }
        return false;
    }

    public void PlayOffsetEffect()
    {
        if (OffsetEffectPrefab != null)
        {
            XEffectManager.GenerateAsync(OffsetEffectPrefab, 0, delegate (XEffectComponent obj)
            {
                if (obj != null && flyEffect_ != null)
                {
                    obj.transform.position = flyEffect_.transform.position;
                }
            });
        }
    }

    public void PlayPacManEffect()
    {
        if (PacManEffectPrefab != null)
        {
            XEffectManager.GenerateAsync(PacManEffectPrefab, 0, delegate (XEffectComponent obj)
            {
                if (obj != null && flyEffect_ != null)
                {
                    obj.transform.position = flyEffect_.transform.position;
                }
            });
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Generate Effect Config")]
    protected void generateEffectConfig()
    {
        string prefabPath = AssetDatabase.GetAssetPath(gameObject).ToLower();
        string assetPath = prefabPath.Replace("/effect/", "/data/effect/");
        assetPath = assetPath.Replace(".prefab", ".asset");
        XEffectConfigObject config = ScriptableObject.CreateInstance<XEffectConfigObject>();
        config.effectFiles[0] = prefabPath.Replace("assets/resources/", "").Replace(".prefab", "");

        string directory = Path.GetDirectoryName(Path.Combine(Path.Combine(Application.dataPath, "resources/data/"),
                                        prefabPath.Replace("assets/resources/", "")));

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        AssetDatabase.CreateAsset(config, assetPath);

        Debug.Log(string.Format("<color=yellow>Create XEffect Config ==>  {0}</color>", assetPath));
    }
#endif
}
