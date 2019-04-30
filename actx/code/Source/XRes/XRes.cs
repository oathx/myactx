using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using SLua;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CustomLuaClass]
public static class XRes
{
	private static AsyncOperation cleanupTask = null;

	/// <summary>
	/// Starts the coroutine.
	/// </summary>
	/// <returns>The coroutine.</returns>
	/// <param name="em">Em.</param>
    private static Coroutine 	StartCoroutine(IEnumerator em)
    {
        return XCoroutine.Run(em);
    }

	/// <summary>
	/// Find the specified path.
	/// </summary>
	/// <param name="path">Path.</param>
    private static Object 		Find(string path)
    {
        return null;
    }

	/// <summary>
	/// Load the specified path.
	/// </summary>
	/// <param name="path">Path.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
    [DoNotToLua]
    public static T 			Load<T>(string path) where T : Object
    {
        return (Load(path, typeof(T)) as T);
    }

	/// <summary>
	/// Load the specified path and type.
	/// </summary>
	/// <param name="path">Path.</param>
	/// <param name="type">Type.</param>
    [DoNotToLua]
    public static Object 		Load(string path, System.Type type)
    {
        if (string.IsNullOrEmpty(path))
        {
            GLog.LogError("[XRes.Load] sent empty/null path!");
            return null;
        }

		string name = path.ToLower ();

		Object asset = Find(name);
        if (!asset)
        {
           XSheet.XAssetInfo info = XSheet.Instance.Find(name);
           if (info != null)
           {
               switch (info.locationType)
               {
                   case XSheet.XLocationType.Resource:
                       if (type == typeof(XBufferAsset))
                       {
                           XBufferAsset bufferAsset = ScriptableObject.CreateInstance<XBufferAsset>();
                           if (bufferAsset)
                           {
                               bufferAsset.init(
                                   Resources.Load<TextAsset>(name)
                                   );
                               asset = bufferAsset;
                           }
                       }
                       else
                       {
                           asset = Resources.Load(name, type);
                       }
                       break;

                   case XSheet.XLocationType.Bundle:
                       GLog.LogError("[XRes.Load] can't load bundle in sync load {0}", name);
                       break;

                   case XSheet.XLocationType.Data:
			            string filePath = System.IO.Path.Combine(Application.persistentDataPath,info.fullName);
					    if( System.IO.File.Exists(filePath) )
					    {
						    System.IO.FileStream fs = System.IO.File.OpenRead(filePath);
                            if (fs.CanRead)
                            {
                                XBufferAsset bufferAsset = ScriptableObject.CreateInstance<XBufferAsset>();
                                if (bufferAsset)
                                {
                                    bufferAsset.init((int)fs.Length);
                                    fs.Read(
                                        bufferAsset.bytes, 0, (int)fs.Length
                                        );
                                    fs.Close();

                                    if (type == typeof(Texture2D))
                                    {
                                        Texture2D buffTexture = new Texture2D(1, 1);
                                        buffTexture.LoadImage(bufferAsset.bytes);

                                        asset = buffTexture;
                                    }
                                    else
                                    {
                                        asset = bufferAsset;
                                    }
                                }
                            }
					    }
                       break;

                   default:
                       GLog.LogError("[XRes] no imp resource type {0}", info.locationType.ToString());
                       break;
               }
           }
        }
        else
        {
            if (type == typeof(XBufferAsset))
            {
                XBufferAsset bufferAsset = ScriptableObject.CreateInstance<XBufferAsset>();
                bufferAsset.init(
                    Resources.Load<TextAsset>(name)
                );

                asset = bufferAsset;
            }
            else
            {
                asset = Resources.Load(name, type);
            }
        }


        return asset;
    }

	/// <summary>
	/// Loads the async.
	/// </summary>
	/// <returns>The async.</returns>
	/// <param name="path">Path.</param>
	/// <param name="callback">Callback.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
    public static Coroutine		LoadAsync<T>(string path, System.Action<Object> callback) where T : Object
    {
        return LoadAsync(path, typeof(T), callback);
    }

	/// <summary>
	/// Loads the async.
	/// </summary>
	/// <returns>The async.</returns>
	/// <param name="path">Path.</param>
	/// <param name="type">Type.</param>
	/// <param name="callback">Callback.</param>
    public static Coroutine 	LoadAsync(string path, System.Type type, System.Action<Object> callback)
    {
        return StartCoroutine(DoLoadAsync(path, type, callback));
    }

	/// <summary>
	/// Dos the load async.
	/// </summary>
	/// <returns>The load async.</returns>
	/// <param name="path">Path.</param>
	/// <param name="type">Type.</param>
	/// <param name="callback">Callback.</param>
    private static IEnumerator 	DoLoadAsync(string path, System.Type type, System.Action<Object> callback)
    {
		if(string.IsNullOrEmpty(path))
		{
			GLog.LogError("[XRes::Load] sent empty/null path!");
			yield break;
		}
		
		string name = path.ToLower();

		UnityEngine.Object obj = Find(name);
		if(!obj)
		{
            XSheet.XAssetInfo info = XSheet.Instance.Find(name);
			if( info != null )
			{
                switch (info.locationType)
                {
                    case XSheet.XLocationType.Resource:
                        ResourceRequest request = null;
                        if (type == typeof(XBufferAsset))
                        {
                            request = Resources.LoadAsync<TextAsset>(name);
                            yield return request;

                            XBufferAsset asset = ScriptableObject.CreateInstance<XBufferAsset>();
                            asset.init((TextAsset)request.asset);
                            obj = asset;
                        }
                        else
                        {
                            request = Resources.LoadAsync(name, type);
                            yield return request;
                            obj = request.asset;
                        }
                        break;

                    case XSheet.XLocationType.Bundle:
                        yield return XBundleManager.Instance.LoadAssetAsync(info,
                            type, delegate(Object result) { obj = result; });
                        break;

                    case XSheet.XLocationType.Data:
                        string fileUrl = XSheet.GetLocalFileUrl(System.IO.Path.Combine(Application.persistentDataPath, info.fullName));
                        WWW fileLoader = new WWW(fileUrl);
                        yield return fileLoader;

                        if (!string.IsNullOrEmpty(fileLoader.error))
                        {
                            XBufferAsset asset = ScriptableObject.CreateInstance<XBufferAsset>();
                            asset.init(
                                fileLoader.bytes
                                );
                            
                            if (type == typeof(Texture2D))
                            {
                                Texture2D texture = new Texture2D(1, 1);
                                texture.LoadImage(asset.bytes);

                                obj = texture;
                            }
                            else
                            {
                                obj = asset;
                            }
                        }
                        break;

                    case XSheet.XLocationType.Streaming:
                        string streamUrl = XSheet.GetLocalFileUrl(System.IO.Path.Combine(Application.streamingAssetsPath, info.fullName));
                        if (Application.platform != RuntimePlatform.Android)
                            streamUrl = XSheet.GetLocalFileUrl(streamUrl);

                        WWW streamLoader = new WWW(streamUrl);
                        yield return streamLoader;

                        if (!string.IsNullOrEmpty(streamLoader.error))
                        {
                            XBufferAsset asset = ScriptableObject.CreateInstance<XBufferAsset>();
                            asset.init(
                                streamLoader.bytes
                                );
                          
                            if (type == typeof(Texture2D))
                            {
                                Texture2D texture = new Texture2D(1, 1);
                                texture.LoadImage(asset.bytes);
                                obj = texture;
                            }
                            else
                            {
                                obj = asset;
                            }
                        }
                        break;
                }
            }
            else
            {
                ResourceRequest request = Resources.LoadAsync(name, type);
                yield return request;
                obj = request.asset;
            }

            if (!obj)
            {
                if (info != null)
                {
                    GLog.LogError("[XRes.Load] Can't find {0} in Location({1})", name, info.locationType);
                }
                else
                {
                    GLog.LogError("[XRes.Load] Can't find {0} in Resources", name);
                }
            }
        }

		callback(obj);
    }

	/// <summary>
	/// Loads the async.
	/// </summary>
	/// <returns>The async.</returns>
	/// <param name="path">Path.</param>
	/// <param name="type">Type.</param>
	/// <param name="callback">Callback.</param>
	/// <param name="param">Parameter.</param>
    private static Coroutine 	LoadAsync(string path, System.Type type, System.Action<Object, int> callback, int param)
    {
        return StartCoroutine(DoLoadAsync(path, type, delegate(Object obj)
        {
            callback(obj, param);
        }));
    }

	/// <summary>
	/// Loads the multi async.
	/// </summary>
	/// <returns>The multi async.</returns>
	/// <param name="paths">Paths.</param>
	/// <param name="callback">Callback.</param>
    public static Coroutine 	LoadMultiAsync(string[] paths, System.Action<Object[]> callback)
    {
        return StartCoroutine(DoLoadMultiAsync(paths, callback));
    }

	/// <summary>
	/// Dos the load multi async.
	/// </summary>
	/// <returns>The load multi async.</returns>
	/// <param name="paths">Paths.</param>
	/// <param name="callback">Callback.</param>
    private static IEnumerator 	DoLoadMultiAsync(string[] paths, System.Action<Object[]> callback)
    {
        if (paths.Length <= 0)
            yield return null;

        Object[] results = new Object[paths.Length];
        bool[] loadDone = new bool[paths.Length];
        for (int i = 0; i < paths.Length; i++)
        {
            loadDone[i] = false;
            LoadAsync(paths[i], typeof(Object), delegate(Object obj, int index)
            {
                results[index] = obj;
                loadDone[index] = true;
            }, i);
        }

        for (int i = 0; i < paths.Length; i++)
            while (!loadDone[i])
                yield return null;

        callback(results);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    public static void LoadScene(string name)
    {
        XBundleManager.Instance.ReleaseSceneCachedBundleOnSceneSwitch();
        XSheet.XScene info = XSheet.Instance.GetScene(name);

        if (info != null)
        {
            XSheet.XPack packInfo = XSheet.Instance.GetPack(info.bundleName);
            switch (packInfo.locationType)
            {
                case XSheet.XLocationType.Resource:
                    GLog.LogError("XRes.LoadScene can't load bundled scene {0} in resource!", name);
                    break;

                default: 
                    GLog.LogError("[XRes] can't load scene in sync load {0}", name);
                    break;
            }
        }
        else
        {
            SceneManager.LoadScene(name);
        }
    }

	/// <summary>
	/// Loads the scene async.
	/// </summary>
	/// <returns>The scene async.</returns>
	/// <param name="name">Name.</param>
	/// <param name="callback">Callback.</param>
    public static Coroutine 	LoadSceneAsync(string name, System.Action<bool> callback)
    {
        XBundleManager.Instance.ReleaseSceneCachedBundleOnSceneSwitch();
        return XCoroutine.Run(DoLoadSceneAsync(name, callback));
    }

	/// <summary>
	/// Dos the load scene async.
	/// </summary>
	/// <returns>The load scene async.</returns>
	/// <param name="name">Name.</param>
	/// <param name="callback">Callback.</param>
    private static IEnumerator DoLoadSceneAsync(string name, System.Action<bool> callback)
    {
        yield return null;

#if UNITY_EDITOR
        yield return SceneManager.LoadSceneAsync(name);
#else
        XSheet.XScene info = XSheet.Instance.GetScene(name);
        if (info != null)
        {
            XSheet.XPack packInfo = XSheet.Instance.GetPack(info.bundleName);
            switch (packInfo.locationType)
            {
                case XSheet.XLocationType.Resource:
                    GLog.LogError("XRes.LoadSceneAsync can't load bundled scene {0} in resource!", name);
                    break;

                default:
                    yield return XBundleManager.Instance.LoadSceneAsync(info);
                    break;
            }
        }
        else
        {
            yield return SceneManager.LoadSceneAsync(name);
        }
#endif

        Scene scene = SceneManager.GetActiveScene();
        callback(scene.name.ToLower() == name);
    }

	/// <summary>
	/// Unload the specified path.
	/// </summary>
	/// <param name="path">Path.</param>
    public static bool Unload(string path)
    {
        return XAssetCache.Instance.Unload(path.ToLower());
    }

	/// <summary>
	/// Unload the specified asset.
	/// </summary>
	/// <param name="asset">Asset.</param>
    public static bool Unload(Object asset)
    {
        return XAssetCache.Instance.Unload(asset);
    }

	/// <summary>
	/// Unloads the unused assets.
	/// </summary>
	/// <returns>The unused assets.</returns>
    private static AsyncOperation UnloadUnusedAssets()
    {
        if ((cleanupTask == null) || cleanupTask.isDone)
        {
            GLog.Log("[XRes.UnloadUnusedAssets] Running cleanup task");
            cleanupTask = Resources.UnloadUnusedAssets();
        }
        return cleanupTask;
    }

	/// <summary>
	/// Flushs all and unload.
	/// </summary>
	/// <returns>The all and unload.</returns>
    [DoNotToLua]
    public static AsyncOperation FlushAllAndUnload()
    {
        XAssetCache.Instance.ClearLoaded();

        return UnloadUnusedAssets();
    }

    [DoNotToLua]
    public static IEnumerator Initialize()
    {
        yield return Initialize(null);
    }


    [DoNotToLua]
    public static IEnumerator Initialize(System.Action<XUpdater.Stage, float, string> progressCallback)
    {
        XSheet.Instance.Initialize();
        yield return null;

        XBundleManager.Instance.Initialize();
        yield return null;
    }
		
	/// <summary>
	/// Update this instance.
	/// </summary>
    [DoNotToLua]
    public static void Update()
    {
    }
}
