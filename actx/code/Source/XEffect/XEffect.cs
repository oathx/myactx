using UnityEngine;
using System.Collections;
using System.IO;
using SLua;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CustomLuaClass]
public class XEffect : XEffectComponent
{
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
