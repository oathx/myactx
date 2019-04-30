using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SLua;

public class XAvatarCacheInfo
{
    public GameObject prefab;
    public string skeleton;
    public List<string> elements;
}

/// <summary>
/// 
/// </summary>
[CustomLuaClass]
public class XAvatarSystem
{
    private static Coroutine StartCoroutine(IEnumerator em)
    {
        return XCoroutine.Run(em);
    }

    /// <summary>
    /// 
    /// </summary>
    private static List<XAvatarCacheInfo> cacheList = new List<XAvatarCacheInfo>();
    private static int MaxCacheCount = 5;
    private static Transform immortalAvatarSystem;

    private static void InitialImmortalCache()
    {
        if (immortalAvatarSystem != null)
            return;

        GameObject immortalGo = GameObject.Find(typeof(XAvatarSystem).Name);
        if (immortalGo == null)
        {
            immortalGo = new GameObject(typeof(XAvatarSystem).Name);
            GameObject.DontDestroyOnLoad(immortalGo);
        }

        immortalAvatarSystem = immortalGo.transform;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="skeleton"></param>
    /// <param name="elements"></param>
    /// <param name="prefab"></param>
    /// <returns></returns>
    private static void AddCacheAvatar(string skeleton, string[] elements, GameObject prefab)
    {
        if (prefab)
        {
            Object.DontDestroyOnLoad(prefab);

            prefab.transform.parent = immortalAvatarSystem;

            XAvatarCacheInfo info = new XAvatarCacheInfo();
            info.elements = new List<string>();
            info.elements.AddRange(elements);

            Animator animator = prefab.GetComponent<Animator>();
            if (animator)
                animator.enabled = false;

            info.prefab = prefab;
            info.skeleton = skeleton;

            if (cacheList.Count >= MaxCacheCount)
            {
                XAvatarCacheInfo first = cacheList[0];
                if (first.prefab)
                    Object.Destroy(first.prefab);

                first.elements.Clear();
                first.skeleton = string.Empty;

                cacheList[0] = info;
            }
            else
            {
                cacheList.Add(info);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="skeleton"></param>
    /// <param name="elements"></param>
    /// <returns></returns>
    private static XAvatarCacheInfo GetCacheAvatar(string skeleton, string[] elements)
    {
        for(int idx=0; idx<cacheList.Count; idx++)
        {
            XAvatarCacheInfo info = cacheList[idx];
            if (info.skeleton == skeleton && info.elements.Count == elements.Length)
            {
                bool conformably = true;
                for (int j=0; j<elements.Length; j++)
                {
                    if (info.elements[j] != elements[j])
                    {
                        conformably = false;
                        break;
                    }
                }

                if (conformably)
                {
                    return info;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    public static void ResetCacheAvatar()
    {
        for(int idx=0; idx<cacheList.Count; idx++)
        {
            XAvatarCacheInfo info = cacheList[idx];
            if (info.prefab)
                GameObject.Destroy(info.prefab);

            info.elements.Clear();
            info.skeleton = string.Empty;
        }

        cacheList.Clear();
    }

    static bool TryGetCache(string name, string[] elementList)
    {
        XAvatarCacheInfo cache = GetCacheAvatar(name, elementList);
        if (cache != null && cache.prefab)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    static IEnumerator OnLoadAsync(string name, string[] elementList, System.Action<Object> callabck)
    {
        XAvatarCacheInfo cacheInfo = GetCacheAvatar(name, elementList);
        if (cacheInfo != null && cacheInfo.prefab != null)
        {
            if (callabck != null)
            {
                GameObject prefab = cacheInfo.prefab;

                prefab.transform.position = Vector3.one * 1000;
                prefab.transform.localScale = Vector3.one;
                prefab.transform.rotation = Quaternion.identity;

                callabck(prefab);
            }

            yield break;
        }

        List<Object> elements = new List<Object>();

        // load all elements
        yield return XRes.LoadMultiAsync(elementList, delegate (Object[] objs)
        {
            for (int i = 0; i < objs.Length; i++)
            {
                elements.Add(objs[i]);
            }
        });
        
        yield return XRes.LoadAsync<GameObject>(name, delegate (Object obj)
        {
            GameObject skeleton = GameObject.Instantiate(obj) as GameObject;
            if (skeleton && elements.Count > 0)
            {
                List<CombineInstance> combineInstances = new List<CombineInstance>();
                Transform[] trans = skeleton.GetComponentsInChildren<Transform>();
                List<Matrix4x4> bindPoses = new List<Matrix4x4>();
                List<Transform> boneTrans = new List<Transform>();
                List<Material> shardMatList = new List<Material>();

                for (int i = 0; i < elements.Count; i++)
                {
                    XAvatarElement element = elements[i] as XAvatarElement;
                    if (element && element.Prefab)
                    {
                        GameObject prefab = GameObject.Instantiate(element.Prefab) as GameObject;
                        if (prefab)
                        {
                            SkinnedMeshRenderer skin = prefab.GetComponent<SkinnedMeshRenderer>();
                            CombineInstance ci = new CombineInstance();
                            ci.mesh = skin.sharedMesh;
                            ci.transform = element.SmrLocalToWorldMatrix;
                            combineInstances.Add(ci);

                            List<Transform> curBones = new List<Transform>();
                            foreach (string transName in element.BoneNames)
                            {
                                for (int transIndex = 0; transIndex < trans.Length; transIndex++)
                                {
                                    if (transName == trans[transIndex].name)
                                    {
                                        curBones.Add(trans[transIndex]);
                                        bindPoses.Add(trans[transIndex].worldToLocalMatrix * skeleton.transform.localToWorldMatrix);
                                        break;
                                    }
                                }
                            }

                            boneTrans.AddRange(curBones);

                            Dictionary<string, int> boneWeightDic = element.GenBoneWeightsDic();
                            foreach (BoneWeight boneWeight in skin.sharedMesh.boneWeights)
                            {
                                BoneWeight bw = boneWeight;

                                bw.boneIndex0 = boneWeightDic[curBones[boneWeight.boneIndex0].name];
                                bw.boneIndex1 = boneWeightDic[curBones[boneWeight.boneIndex1].name];
                                bw.boneIndex2 = boneWeightDic[curBones[boneWeight.boneIndex2].name];
                                bw.boneIndex3 = boneWeightDic[curBones[boneWeight.boneIndex3].name];
                            }

                            shardMatList.AddRange(element.SharedMaterials);

                            GameObject.DestroyImmediate(prefab);
                        }
                    }
                }

                skeleton.transform.position = Vector3.one * 1000;
                skeleton.name = obj.name;
                Transform model = skeleton.transform.Find(XActorElementName.shape.ToString());
                if (!model)
                    model = skeleton.transform;

                GameObject render = new GameObject(typeof(Renderer).Name);

                render.transform.parent = model;
                render.transform.localScale = Vector3.one;
                render.transform.localPosition = Vector3.zero;
                render.transform.localRotation = Quaternion.identity;

                SkinnedMeshRenderer smr = render.AddComponent<SkinnedMeshRenderer>();
                smr.sharedMesh = new Mesh();
                smr.sharedMesh.CombineMeshes(combineInstances.ToArray(), false, false);
                smr.bones = boneTrans.ToArray();
                smr.sharedMaterials = shardMatList.ToArray();

                Animator animator = skeleton.GetComponent<Animator>();
                if (animator)
                {
                    smr.rootBone = animator.GetBoneTransform(HumanBodyBones.Hips);
                }

                smr.sharedMesh.RecalculateBounds();

                // add avatar cache
                AddCacheAvatar(name, elementList, skeleton);
                if (callabck != null)
                    callabck(skeleton);
            }
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="skeletonName"></param>
    /// <param name="elementList"></param>
    /// <param name="callback"></param>
    public static Coroutine LoadAsync(string skeletonName, string[] elementList, System.Action<Object> callback)
    {
        InitialImmortalCache();
        return StartCoroutine(OnLoadAsync(skeletonName, elementList, callback));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="confs"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    static IEnumerator DoLoadMultiAsync(XAvaterConfigure[] confs, System.Action<Object[]> callback)
    {
        Object[] results = new Object[confs.Length];
        bool[] loadDone = new bool[confs.Length];

        for (int i = 0; i < confs.Length; i++)
        {
            loadDone[i] = false;
            int index = i;
            LoadAsync(confs[i].skeleton, confs[i].elements, delegate (Object obj) {
                results[index] = obj;
                loadDone[index] = true;
            });
        }

        for (int i = 0; i < confs.Length; i++)
        {
            while (!loadDone[i])
                yield return null;
        }

        callback(results);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="confs"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public static Coroutine LoadMultiAsync(XAvaterConfigure[] confs, System.Action<Object[]> callback)
    {
        InitialImmortalCache();
        return StartCoroutine(DoLoadMultiAsync(confs, callback));
    }
}