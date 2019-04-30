using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum XProjectDriectory
{
    res, update, bytecode, Resources, Bytecode, Assets, actx_Data, StaticRes, Art, Hero, Prefab, Monster, Element, Avatar, Merge, PrefabHero
}


public sealed class XBundleManager {
    /// <summary>
    /// 
    /// </summary>
	public static XBundleManager            Instance 
	{ get; internal set; }

	/// <summary>
	/// Initializes the <see cref="XBundleManager"/> class.
	/// </summary>
	static XBundleManager(){
		Instance = new XBundleManager ();
	}

    /// <summary>
    /// 
    /// </summary>
	public static string BundlePrefix 	    = typeof(AssetBundle).Name.ToLower();

    /// <summary>
    /// 
    /// </summary>
	public static string dataBasePath       = System.IO.Path.Combine(Application.persistentDataPath, BundlePrefix);
    public static string streamBasePath     = System.IO.Path.Combine(
        Application.streamingAssetsPath, BundlePrefix).Replace("\\", "/");

    /// <summary>
    /// 
    /// </summary>
    public static string assetBasePath      = string.Format("{0}/{1}", 
        XProjectDriectory.Assets.ToString(), XProjectDriectory.Resources.ToString()).ToLower();

    /// <summary>
    /// 
    /// </summary>
    public class XAssetBundleInfo
    {
        public XSheet.XPack pack
        { get; set; }

        public AssetBundle  bundle
        { get; set; }

        public bool         isDone
        { get; set; }

        public bool         isLoading
        { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public HashSet<string>
            depended = new HashSet<string>();

        /// <summary>
        /// 
        /// </summary>
        public int         refCount;
        public float       lastReadTime;


        public XAssetBundleInfo(XSheet.XPack info)
        {
            pack = info;
            bundle = null;
            isDone = false;
            isLoading = false;
            refCount = 0;
            lastReadTime = 0;
        }

        public int Ref() 
        { 
            return refCount; 
        }

        public void IncRef(float time)
        {
            refCount++;
            lastReadTime = time;
        }

        public void DecRef() 
        { 
            refCount--; 
        }

        public bool Unused(float time)
        {
            return refCount <= 0;
        }
        public void ResetRef()
        {
            refCount = 0;
            lastReadTime = 0;
        }

        public int DependedCount() 
        {
            return depended.Count; 
        }

        public void AddDepended(string bundleName)
        {
            depended.Add(bundleName);
            lastReadTime = Time.time;
        }

        public void RemoveDepended(string bundleName)
        {
            depended.Remove(bundleName);
        }

        public string path
        {
            get
            {
                return System.IO.Path.Combine(XBundleManager.dataBasePath, pack.name);
                
            }
        }

        public string url
        {
            get
            {
                switch (pack.locationType)
                {
                    case XSheet.XLocationType.Data:
                        {
                            return System.IO.Path.Combine(XBundleManager.dataBasePath, pack.name);
                        }

                    case XSheet.XLocationType.Streaming:
                        {
                            switch (Application.platform)
                            {
                                case RuntimePlatform.Android:
#if EXTERNAL_BUNDLE
                                    return System.IO.Path.Combine(ProcessPackRes.OutCachePath, pack.name);
#else
                                    return System.IO.Path.Combine(XBundleManager.streamBasePath, pack.name);
#endif
                                default:
                                    return System.IO.Path.Combine(XBundleManager.streamBasePath, pack.name);
                            }
                        }

                    default:
                        return string.Empty;
                }
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    private List<XAssetBundleInfo> allInfo = new List<XAssetBundleInfo>();

    /// <summary>
    /// 
    /// </summary>
    private Dictionary<string, XAssetBundleInfo> 
        bundles = new Dictionary<string, XAssetBundleInfo>();

    /// <summary>
    /// 
    /// </summary>
    public void Initialize()
    {
        foreach (KeyValuePair<string, XAssetBundleInfo> item in bundles)
        {
            XAssetBundleInfo info = item.Value;
            if (info.isDone)
                info.bundle.Unload(false);
        }

        bundles.Clear();
        allInfo.Clear();

        foreach (KeyValuePair<string, XSheet.XPack> item in XSheet.Instance.GetPacks())
        {
            XAssetBundleInfo info = new XAssetBundleInfo(item.Value);
            bundles.Add(item.Key, info);
            allInfo.Add(info);
        }

        Resources.UnloadUnusedAssets();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Update()
    {
        ReleaseBundle(XSheet.XCacheType.None);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ReleaseSceneCachedBundleOnSceneSwitch()
    {
        ReleaseBundle(XSheet.XCacheType.InScene);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bundleName"></param>
    /// <returns></returns>
    public XAssetBundleInfo GetAssetBundleInfo(string bundleName)
    {
        XAssetBundleInfo loaded = null;
        bundles.TryGetValue(bundleName, out loaded);

        return loaded;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cacheType"></param>
    private void ReleaseBundle(XSheet.XCacheType cacheType)
    {
        float now = Time.time;
        for (int i = 0; i < allInfo.Count; i++)
        {
            XAssetBundleInfo info = allInfo[i];
            if (info.isDone && info.Unused(now))
            {
                if (info.pack.cacheType != cacheType && info.pack.cacheType != XSheet.XCacheType.None)
                    continue;
#if !RELEASE
                if (info.Ref() < 0)
                    GLog.LogError("XAssetManager.Unload bundle:{0} refCount({1}) incorrect!", info.pack.name, info.Ref());
#endif

                info.ResetRef();

                for (int depIndex = 0; depIndex < info.pack.dependencies.Length; depIndex++)
                {
                    string dependence = info.pack.dependencies[depIndex];
                    XAssetBundleInfo dependenceInfo = GetAssetBundleInfo(dependence);
                    if (dependenceInfo != null)
                        dependenceInfo.RemoveDepended(info.pack.name);
                }
            }
        }

        for (int i = 0; i < allInfo.Count; i++)
        {
            XAssetBundleInfo info = allInfo[i];
            if (info.isDone && info.Unused(now))
            {
                if (info.pack.cacheType != cacheType && info.pack.cacheType != XSheet.XCacheType.None)
                    continue;

                if (info.DependedCount() == 0)
                {
                    info.bundle.Unload(false);
                    info.bundle = null;
                    info.isDone = false;

                    GLog.Log("AssetManager.UnloadAssetBundle bundle:{0} unloaded ref:{1} cache type: {2}", 
                        info.pack.name, info.Ref(), info.pack.cacheType.ToString());
                }
            }
        }
    }

    private IEnumerator LoadDependenciesAsync(XSheet.XPack pack)
    {
        for (int i = 0; i < pack.dependencies.Length; i++)
        {
            XAssetBundleInfo bundleInfo = GetAssetBundleInfo(pack.dependencies[i]);
            if (bundleInfo != null)
            {
                if (!bundleInfo.isDone)
                    XCoroutine.Run(LoadAssetBundleAsync(bundleInfo, false));

                bundleInfo.AddDepended(pack.name);
            }
        }
        for (int i = 0; i < pack.dependencies.Length; i++)
        {
            XAssetBundleInfo bundleInfo = GetAssetBundleInfo(pack.dependencies[i]);
            if (bundleInfo != null)
            {
                while (bundleInfo.isLoading)
                    yield return null;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bundleInfo"></param>
    /// <param name="loadDependence"></param>
    /// <returns></returns>
    private IEnumerator LoadAssetBundleAsync(XAssetBundleInfo bundleInfo, bool loadDependence = true)
    {
        while (bundleInfo.isLoading)
            yield return null;

        if (bundleInfo.isDone)
            yield break;

        GLog.Log("XBundleManager.LoadAssetBundleAsync {0}", bundleInfo.url);

        bundleInfo.isLoading = true;
        if (loadDependence)
            yield return XCoroutine.Run(LoadDependenciesAsync(bundleInfo.pack));

        string bundleName = bundleInfo.pack.name;
        AssetBundleCreateRequest loader = AssetBundle.LoadFromFileAsync(bundleInfo.url); 
        yield return loader;

        if (!loader.isDone)
        {
            bundleInfo.isLoading = false;
            GLog.LogError("XBundleManager.LoadAssetBundle can't async load bundle: {0} reason: {1}",
                bundleName, "NOT FOUND!");
        }
        else
        {
            bundleInfo.isLoading = false;
            if (loader.assetBundle != null)
            {
                bundleInfo.bundle = loader.assetBundle;
                bundleInfo.isDone = true;
                GLog.Log("XBundleManager.LoadAssetBundle async load done bundle: {0}", bundleName);
            }
            else
            {
                GLog.LogError("AssetBundleManager.LoadAssetBundle can't async load bundle: {0}", bundleName);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <param name="type"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public Coroutine LoadAssetAsync(XSheet.XAssetInfo info, System.Type type, System.Action<Object> callback)
    {
        return XCoroutine.Run(DoLoadAssetAsync(info, type, callback));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <param name="type"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public IEnumerator DoLoadAssetAsync(XSheet.XAssetInfo info, System.Type type, System.Action<Object> callback)
    {
        XAssetBundleInfo bundleInfo = GetAssetBundleInfo(info.bundleName);
        if (!bundleInfo.isDone)
        {
            yield return XCoroutine.Run(LoadAssetBundleAsync(bundleInfo));
        }

        if (bundleInfo.isDone)
        {
            bundleInfo.IncRef(Time.time);

            AssetBundleRequest request = null;
            Object obj = null;
            if (type == typeof(XBufferAsset))
            {
                request = bundleInfo.bundle.LoadAssetAsync<TextAsset>(System.IO.Path.Combine(assetBasePath, info.fullName));
                yield return request;
                XBufferAsset asset = ScriptableObject.CreateInstance<XBufferAsset>();
                asset.init((TextAsset)request.asset);
                obj = asset;
            }
            else
            {
                request = bundleInfo.bundle.LoadAssetAsync(System.IO.Path.Combine(assetBasePath, info.fullName), type);
                yield return request;
                obj = request.asset;
            }

            bundleInfo.DecRef();

            callback(obj);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public Coroutine LoadSceneAsync(XSheet.XScene info)
    {
        return XCoroutine.Run(DoLoadSceneAsync(info));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private IEnumerator DoLoadSceneAsync(XSheet.XScene info)
    {
        XAssetBundleInfo bundleInfo = GetAssetBundleInfo(info.bundleName);
        if (!bundleInfo.isDone)
        {
            yield return XCoroutine.Run(LoadAssetBundleAsync(bundleInfo));
        }

        if (bundleInfo.isDone)
        {
            bundleInfo.IncRef(Time.time);

            yield return SceneManager.LoadSceneAsync(info.name);

            bundleInfo.DecRef();
        }
    }

}
