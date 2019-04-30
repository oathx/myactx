using UnityEngine;
using System.Collections;
using SLua;

#if UNITY_IOS
using UnityEngine.iOS;
#endif

[CustomLuaClass]
public class XGameApp : MonoBehaviour {
	/// <summary>
	/// The lua server.
	/// </summary>
	private static LuaSvr	luaServer = null;
	/// <summary>
	/// The instance.
	/// </summary>
	private static XGameApp instance;

	/// <summary>
	/// Gets the singleton.
	/// </summary>
	/// <returns>The singleton.</returns>
	public static XGameApp 	GetSingleton(){
		return instance;
	}
		
	/// <summary>
	/// 
	/// </summary>
	private static string platformPath = string.Empty;

	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake(){
		instance = this;
	}

    /// <summary>
    /// 
    /// </summary>
    void Reinit()
    {
        XCoroutine.StopAll();

        DG.Tweening.DOTween.KillAll(false);

        XLanguage.Reinit();
    }

	/// <summary>
	/// Start this instance.
	/// </summary>
	IEnumerator Start(){
        yield return ResetReporter();
        yield return InitBootstrap();
      
        yield return StartCoroutine(XUpdater.DoUpdate(delegate(XUpdater.Stage stage, float progress, string message) {

        }));


        XCameraHelper.Initializ();


        XTcpServer.GetSingleton().Initliaze();

        // init slua
		InitSLua (delegate(int progress) {
			GameObject.DontDestroyOnLoad (gameObject);
		});
	}

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator ResetReporter()
    {
        Reporter reporter = Object.FindObjectOfType<Reporter>();
        if (!reporter)
        {
            GameObject prefab = Resources.Load<GameObject>(typeof(Reporter).Name);
            if (prefab != null)
                Object.Instantiate(prefab);
        }

        yield return reporter;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator InitBootstrap()
    {
        ResourceRequest req = Resources.LoadAsync<GameObject>(
            typeof(XBootstrap).Name.ToLower());
        yield return req;

        GameObject bootstrap = GameObject.Instantiate(req.asset as GameObject);
        if (bootstrap)
        {
            bootstrap.name = typeof(XBootstrap).Name;
            bootstrap.transform.SetParent(
                GameObject.Find("UI/Canvas").transform, false);
        }
    }

	/// <summary>
	/// Fixeds the update.
	/// </summary>
	void FixedUpdate()
	{
		XTcpServer.GetSingleton().Update();
	}

	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	void OnDestroy()
	{
		XTcpServer.GetSingleton().Shutdown();

		if (luaServer != null) {
			LuaSvr.mainState.run ("exit");
		}
	}

	/// <summary>
	/// Raises the appliaction quit event.
	/// </summary>
	void OnAppliactionQuit()
	{
		// close lua vm
		LuaSvr.mainState.Dispose ();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public static string GetPlatformBytecodeFloder()
	{
        switch (Application.platform)
        {
	        case RuntimePlatform.IPhonePlayer:
	        case RuntimePlatform.OSXEditor:
	        case RuntimePlatform.OSXPlayer:
		        return "jitgc64";
            case RuntimePlatform.Android:
                return "jitx86";
	        default:
		        return "jitx64";
        }
	}

	/// <summary>
	/// Loads the script.
	/// </summary>
	/// <returns>The script.</returns>
	/// <param name="fn">Fn.</param>
	public static byte[]    LoadScript(string fn, ref string str)
	{
		fn = fn.Replace(".", "/");

#if UNITY_STANDALONE || UNITY_EDITOR 
		XBufferAsset asset = ScriptableObject.CreateInstance<XBufferAsset>();

		// find config source code
		string filePath = System.IO.Path.Combine(Application.dataPath, string.Format("Config/{0}.lua", fn));
		if (!System.IO.File.Exists(filePath))
		{
			// find logic script
			filePath = System.IO.Path.Combine(Application.dataPath, string.Format("Code/Script/{0}.lua", fn));
			if (!System.IO.File.Exists(filePath))
			{
                string bytefn = fn.Replace("/", "@").ToLower();
				// find lua byte code
				filePath = System.IO.Path.Combine(Application.dataPath,
                    string.Format("Bytecode/{0}/{1}.bytes", platformPath, bytefn));
			}
		}
			
#if !RELEASE
		GLog.Log(string.Format("{0}", filePath));
#endif

		if (System.IO.File.Exists(filePath))
		{
			System.IO.FileStream fs = System.IO.File.OpenRead(filePath);
			asset.init((int)fs.Length);
			fs.Read(asset.bytes, 0, (int)fs.Length);
			fs.Close();

            return asset.bytes;
		}
		else 
		{
			Debug.LogError("can't find file : " + filePath);
		}
#else
		string bytefn = fn.Replace("/", "@").ToLower();
		string hashfn = XUtility.Md5Sum(string.Format("{0}/{1}", 
			platformPath, bytefn));
     
		XBufferAsset asset = XBytecodeFilePicker.LoadBytecodeAsset(string.Format("bytecode/{0}", hashfn));
        if (asset != null)
        {
            return asset.bytes;
        }
        else
        {
            Debug.LogError("Can't load bytecode " + platformPath + " " + bytefn);
        }
#endif
        return null;
	}

	/// <summary>
	/// Inits the S lua.
	/// </summary>
	/// <returns><c>true</c>, if S lua was inited, <c>false</c> otherwise.</returns>
	/// <param name="progress">Progress.</param>
	public static bool 		InitSLua(System.Action<int> progress){
		XBytecodeFilePicker.InitPicker ();

		luaServer = new LuaSvr ();

		platformPath = GetPlatformBytecodeFloder();
		if (!string.IsNullOrEmpty(platformPath))
			LuaSvr.mainState.loaderDelegate = LoadScript;

        Debug.LogFormat("BytecodeFloder : {0}", platformPath);

		luaServer.init(progress, () => {
			#if !RELEASE
			LuaSvr.mainState["_DEBUG"] = true;
			#endif

			#if UNITY_EDITOR 
			LuaSvr.mainState["_EDITOR"] = true;
			#endif

			#if UNITY_ANDROID
			LuaSvr.mainState["_ANDROID"] = true;
			#endif

			#if UNITY_IPHONE
			luaServer.luaState["_IPHONE"] = true;
			#endif

			#if _LANGUAGE_CN
			LuaSvr.mainState["_LANGUAGE_CN"] = true;
			#endif

			#if _LANGUAGE_EN
			LuaSvr.mainState["_LANGUAGE_EN"] = true;
			#endif

			#if _LOCAL_SERVER
			LuaSvr.mainState["_LOCAL_SERVER"] = true;
			#endif

			var success = luaServer.start("main");
			if( success == null || (bool)success != true )
			{
				Debug.LogError("Lua main intialize failed.");
			}
		}, LuaSvrFlag.LSF_BASIC | LuaSvrFlag.LSF_EXTLIB
			#if !RELEASE && LUA_DEBUG
			| LuaSvrFlag.LSF_DEBUG
			#endif
		);	
			
		return true;
	}
}


