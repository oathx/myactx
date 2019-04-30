using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class XBundleExtraCacheInfo
{
    public string[] NotCache;
    public string[] CacheInScene;
    public string[] CacheAllTime;

    public string[] NotCompress;
    public string[] CompressLZMA;
    public string[] CompressLZ4;

    // in default, we decide compress type by cache type
    public void ResetCompressToDefault()
    {
        this.NotCompress = this.CacheAllTime;
        this.CompressLZMA = this.NotCache;
        this.CompressLZ4 = this.CacheInScene;
    }
}

[System.Serializable]
public class XBundleExtraPackageInfo
{
    public string[] PackgesCategory1;
    public string[] PackgesCategory2;
    public string[] PackgesCategory3;
}

/// <summary>
/// 
/// </summary>
public class XBundleExtraInfoUtils
{
#if UNITY_EDITOR
    public static string BundleExtraCacheInfoPath   = typeof(XBundleExtraCacheInfo).Name.ToLower();
    public static string BundleExtraPackageInfoPath = typeof(XBundleExtraPackageInfo).Name.ToLower();
#endif

    /// <summary>
    /// 
    /// </summary>
    public static string BundleExtraInfoName        = "BundleExtraInfo";

#if UNITY_EDITOR
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static string GetBundleExtraCacheInfoText()
    {
        return GetBundleExtraText(BundleExtraCacheInfoPath);
    }
#endif

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetBundleExtraText(string path)
    {
        string data     = string.Empty;
        string extPath  = path;
        if (System.IO.File.Exists(extPath))
        {
            System.IO.StreamReader sr = new System.IO.StreamReader(extPath);
            data = sr.ReadToEnd();
            sr.Close();
        }
        else
        {
            TextAsset text = Resources.Load<TextAsset>(BundleExtraInfoName);
            if (text != null)
            {
                data = text.text;
            }
        }

        return data;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static List<T> ArrayToList<T>(T[] arr)
    {
        if (arr != null)
        {
            List<T> generated = new List<T>();
            for (int i = 0; i < arr.Length; i++)
            {
                generated.Add(arr[i]);
            }
            return generated;
        }
        return null;
    }

}

/// <summary>
/// X sheet.
/// </summary>
public sealed class XSheet {
	/// <summary>
	/// location resource type. XRes.Load
	/// </summary>
	public enum XLocationType{
		Resource, Data, Streaming, Bundle
	};

	/// <summary>
	/// cache type.
	/// </summary>
	public enum XCacheType { None, InScene, AllTime};
	public enum XEntryType {
		None, Pack, File, Scene
	};

	/// <summary>
	/// X pack type.
	/// </summary>
	public enum XPackType { None, P1, P2, P3};

	/// <summary>
	/// compress type. default zip
	/// </summary>
	public enum XCompressType {
		None, Zip
	};
		
	/// <summary>
	/// X entry.
	/// </summary>
	public class XEntry {
		/// <summary>
		/// The type of the entry.
		/// </summary>
		protected XEntryType 	entryType = XEntryType.None;

		/// <summary>
		/// Initializes a new instance of the <see cref="XSheet+XEntry"/> class.
		/// </summary>
		public XEntry(){
		}

		/// <summary>
		/// Type this instance.
		/// </summary>
		public XEntryType		Type() {
			return entryType;
		}

		public uint				checksum
		{ get; set; }
		public string 			name
		{ get; set; }
		public uint 			size
		{ get; set; }
		public XLocationType	locationType
		{ get; set; }
		public XCacheType 		cacheType
		{ get; set; }
		public XCompressType 	compressType
		{ get; set; }
		public XPackType 		packType
		{ get; set; }
	}

	/// <summary>
	/// default file.
	/// </summary>
	public class XFile : XEntry {
		public XFile() {
			entryType = XEntryType.File;
		}
	}

	/// <summary>
	/// X pack.
	/// </summary>
	public class XPack : XEntry {
		public string[] files
		{ get; set; }

		public string[] dependencies
		{ get; set; }

		public XPack(){
			entryType = XEntryType.Pack;
		}

		public bool Nobundle(){
			return locationType == XLocationType.Resource;
		}
	}

	/// <summary>
	/// X scene.
	/// </summary>
	public class XScene : XEntry {
		public string bundleName
		{ get; set; }

		public XScene(){
			entryType = XEntryType.Scene;
		}
	}

	/// <summary>
	/// sheet file.
	/// </summary>
	public class XSheetInfo
    {
		/// <summary>
		/// Parse the specified data.
		/// </summary>
		/// <param name="data">Data.</param>
		public static XSheetInfo Parse(string data){
			return LitJson.JsonMapper.ToObject<XSheetInfo> (data);
		}

		/// <summary>
		/// Gets or sets the packs.
		/// </summary>
		/// <value>The packs.</value>
		public List<XPack> 		packs
		{ get; set;}

		public List<XFile> 		files
		{ get; set; }

		public List<XScene>		scenes
		{ get; set;}

		/// <summary>
		/// Xs the sheet file.
		/// </summary>
		public XSheetInfo(){
			Init();
		}

		/// <summary>
		/// Init this instance.
		/// </summary>
		public void Init(){
			packs 	= new List<XPack> ();
			files 	= new List<XFile> ();
			scenes 	= new List<XScene> ();			
		}
	}

	/// <summary>
	/// asset info.
	/// </summary>
	public class XAssetInfo{
		public XLocationType 	locationType
		{ get; set; }

		public string 			bundleName
		{ get; set; }

		public string 			fullName
		{ get; set; }
	}

	/// <summary>
	/// entry diff.
	/// </summary>
	public class XEntryDiff{
		public XEntryType 		entryType = XEntryType.None;
		public XEntry 			local;
		public XEntry 			remote;
	}


#if UNITY_EDITOR
    public static string GetPlatformFolder(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.Android:
                return "android";
            case BuildTarget.iOS:
                return "ios";
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return "windows";
            case BuildTarget.StandaloneOSXIntel:
            case BuildTarget.StandaloneOSXIntel64:
            case BuildTarget.StandaloneOSXUniversal:
                return "osx";
            default:
                return null;
        }
    }
#endif

    /// <summary>
    /// Gets the platform folder.
    /// </summary>
    /// <returns>The platform folder.</returns>
    /// <param name="platform">Platform.</param>
    public static string GetPlatformFolder(RuntimePlatform platform) {
		switch(platform)
		{
		case RuntimePlatform.Android:
			return "android";
		case RuntimePlatform.IPhonePlayer:
			return "ios";
		case RuntimePlatform.WindowsPlayer:
			return "windows";
		case RuntimePlatform.OSXPlayer:
			return "osx";
		default:
			return null;
		}
	}


	/// <summary>
	/// Gets the platform folder.
	/// </summary>
	/// <returns>The platform folder.</returns>
	public static string GetPlatformFolder() {
		#if UNITY_EDITOR
		return GetPlatformFolder(EditorUserBuildSettings.activeBuildTarget);
		#else
		return GetPlatformFolder(Application.platform);
		#endif
	}

	/// <summary>
	/// Initializes the <see cref="XSheet"/> class.
	/// </summary>
	public static XSheet Instance 
	{ get; internal set; }
		
	/// <summary>
	/// Initializes the <see cref="XSheet"/> class.
	/// </summary>
	static XSheet(){
		Instance = new XSheet ();
	}

	/// <summary>
	/// The packs.
	/// </summary>
	private Dictionary<string, XPack> packs 	= new Dictionary<string, XPack>();
	private Dictionary<string, XFile> files 	= new Dictionary<string, XFile>();
	private Dictionary<string, XScene> scenes 	= new Dictionary<string, XScene>();

	/// <summary>
	/// The fast indexs.
	/// </summary>
	private Dictionary<string, XAssetInfo> 
		fastIndexs = new Dictionary<string, XAssetInfo>();
#if UNITY_EDITOR
	/// <summary>
	/// Gets the index of the fast.
	/// </summary>
	/// <returns>The fast index.</returns>
	public Dictionary<string, XAssetInfo> GetFastIndex(){
		return fastIndexs;
	}
#endif

    public static string name = typeof(XSheet).Name.ToLower();
    
    /// <summary>
    /// Initialize this instance.
    /// </summary>
    public bool Initialize(){
		Clearup ();

		string data = string.Empty;

#if UNITY_STANDALONE
        string extPath = Path.Combine(Path.Combine(Path.Combine(Application.streamingAssetsPath, GetPlatformFolder()), XBundleManager.BundlePrefix), name);
#else
		string extPath = Path.Combine(Application.persistentDataPath, name);
#endif

#if UNITY_EDITOR
        Debug.Log("Use local resource sheet");
#else
        if (System.IO.File.Exists (extPath)) {
			data = System.IO.File.ReadAllText (extPath);

            if (string.IsNullOrEmpty(data))
            {
                System.IO.File.Delete(extPath);
            }
		}

		if (string.IsNullOrEmpty (data)) 
        {
			TextAsset text = Resources.Load<TextAsset> (name);
			if (text) {
				data = text.text;
			}                              
        }
#endif

		if (string.IsNullOrEmpty (data)) {
			return false;
		}

		XSheetInfo info = XSheetInfo.Parse (data);
		if (info != null) {
			for (int i = 0; i < info.files.Count; i++) {
				Add (info.files [i]);
			}

			for (int i = 0; i < info.packs.Count; i++) {
				Add (info.packs [i]);
			}

			for (int i = 0; i < info.scenes.Count; i++) {
				Add (info.scenes [i]);
			}
		} 
		else 
		{
			Debug.LogError ("XSheet.cs parse sheet failed");
		}

		return true;
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public XAssetInfo Find(string name)
    {
        XAssetInfo index = null;
        fastIndexs.TryGetValue(name, out index);

        return index;
    }

	/// <summary>
	/// Remove the specified entry.
	/// </summary>
	/// <param name="entry">Entry.</param>
	public void Remove( XEntry entry ) {
		switch(entry.Type())
		{
		case XEntryType.Pack:
			packs.Remove(entry.name);
			break;
		case XEntryType.File:
			files.Remove(entry.name);
			break;
		case XEntryType.Scene:
			scenes.Remove(entry.name);
			break;
		}
	}

	/// <summary>
	/// Add the specified entry.
	/// </summary>
	/// <param name="entry">Entry.</param>
	public void Add(XEntry entry ) {
		switch(entry.Type())
		{
		case XEntryType.Pack:
			packs.Add(entry.name, entry as XPack);
			break;
		case XEntryType.File:
			files.Add(entry.name, entry as XFile);
			break;
		case XEntryType.Scene:
			scenes.Add(entry.name, entry as XScene);
			break;
		}
	}

	/// <summary>
	/// Add the specified pack.
	/// </summary>
	/// <param name="pack">Pack.</param>
	public void Add( XPack pack )
    {
#if UNITY_EDITOR
        Debug.Log(string.Format("<color=red> XSheet add [XPath] {0}.{1}.{2} </color>", pack.name, pack.size, pack.checksum));
#endif

        packs.Add(pack.name, pack);

		XLocationType local = pack.Nobundle() ? XLocationType.Resource : XLocationType.Bundle;
		foreach (string file in pack.files) {
			AddFastIndex( file, new XAssetInfo() { locationType=local, bundleName=pack.name, fullName=file } );
		}
	}

	/// <summary>
	/// Add the specified file.
	/// </summary>
	/// <param name="file">File.</param>
	public void Add( XFile file ) {
#if UNITY_EDITOR
        Debug.Log(string.Format("<color=red> XSheet add [XFile] {0}.{1}.{2} </color>", file.name, file.size, file.checksum));
#endif
		files.Add(file.name, file );

		AddFastIndex( file.name, 
			new XAssetInfo() { locationType=file.locationType, fullName=file.name } );
	}

	/// <summary>
	/// Add the specified scene.
	/// </summary>
	/// <param name="scene">Scene.</param>
	public void Add( XScene scene ) {
#if UNITY_EDITOR
        Debug.Log(string.Format("<color=red> XSheet add [XScene] {0}.{1}.{2} </color>", scene.name, scene.size, scene.checksum));
#endif
		scenes.Add(scene.name, scene);
	}

	/// <summary>
	/// Adds the index of the fast.
	/// </summary>
	/// <param name="name">Name.</param>
	/// <param name="index">Index.</param>
	public void AddFastIndex(string name, XAssetInfo index) {
		string key = name;
		if( System.IO.Path.HasExtension(name) )
			key = System.IO.Path.ChangeExtension(name, null);

		fastIndexs[key] = index;
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public XPack GetPack(string name)
    {
        XPack pack = null;
        packs.TryGetValue(name, out pack);
        return pack;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public XFile GetFile(string name)
    {
        XFile file = null;
        files.TryGetValue(name, out file);
        return file;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public XScene GetScene(string name)
    {
        XScene scene = null;
        scenes.TryGetValue(name, out scene);
        return scene;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IEnumerable GetPacks()
    {
        return packs;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IEnumerable GetScenes()
    {
        return scenes;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IEnumerable GetFiles()
    {
        return files;
    }

    /// <summary>
    /// Clearup this instance.
    /// </summary>
    public void Clearup(){
		packs.Clear ();
		files.Clear ();
		scenes.Clear ();
		fastIndexs.Clear ();
	}

#if UNITY_EDITOR
    public bool Exists(string fullpath)
    {
        string key = fullpath;
        if (System.IO.Path.HasExtension(fullpath))
            key = System.IO.Path.ChangeExtension(fullpath, null);

        return fastIndexs.ContainsKey(key);
    }
#endif

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="pretty"></param>
    public void Save(string path, bool pretty = true)
    {
        Save(path, string.Empty, pretty);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="extraPackagePath"></param>
    /// <param name="pretty"></param>
    public void Save(string path, string extraPackagePath, bool pretty = true)
    {
        if (string.IsNullOrEmpty(path))
            path = System.IO.Path.Combine(Application.persistentDataPath, typeof(XSheet).Name.ToLower());

        List<XPack> xpacks = new List<XPack>();
        foreach (XPack pack in packs.Values)
        {
            xpacks.Add(pack);
        }

        List<XFile> xfiles = new List<XFile>();
        foreach (XFile file in files.Values)
        {
            xfiles.Add(file);
        }

        List<XScene> xscenes = new List<XScene>();
        foreach (XScene scene in scenes.Values)
        {
            xscenes.Add(scene);
        }

#if UNITY_EDITOR
        XBundleExtraCacheInfo cacheInfo = null;
        List<string> cacheInScene       = null;
        List<string> cacheAllTime       = null;

        string infoText = XBundleExtraInfoUtils.GetBundleExtraCacheInfoText();
        if (!string.IsNullOrEmpty(infoText))
        {
            cacheInfo = LitJson.JsonMapper.ToObject<XBundleExtraCacheInfo>(infoText);
            if (cacheInfo != null)
            {
                cacheInScene = XBundleExtraInfoUtils.ArrayToList<string>(cacheInfo.CacheInScene);
                if (cacheInScene == null)
                    cacheInScene = new List<string>();
                cacheAllTime = XBundleExtraInfoUtils.ArrayToList<string>(cacheInfo.CacheAllTime);
                if (cacheAllTime == null)
                    cacheAllTime = new List<string>();

                foreach (XPack pack in xpacks)
                {
                    SetupBundleCacheType(pack, cacheInScene, cacheAllTime);
                }

                foreach (XScene scene in xscenes)
                {
                    SetupBundleCacheType(scene, cacheInScene, cacheAllTime);
                }
            }
        }

        if (!string.IsNullOrEmpty(extraPackagePath))
        {
            XBundleExtraPackageInfo packageInfo = null;
            List<string> package1 = new List<string>();
            List<string> package2 = new List<string>();
            List<string> package3 = new List<string>();
            string packageInfoText = XBundleExtraInfoUtils.GetBundleExtraText(extraPackagePath);
            if (!string.IsNullOrEmpty(packageInfoText))
            {
                packageInfo = LitJson.JsonMapper.ToObject<XBundleExtraPackageInfo>(packageInfoText);
                if (packageInfo != null)
                {
                    package1 = XBundleExtraInfoUtils.ArrayToList<string>(packageInfo.PackgesCategory1);
                    package2 = XBundleExtraInfoUtils.ArrayToList<string>(packageInfo.PackgesCategory2);
                    package3 = XBundleExtraInfoUtils.ArrayToList<string>(packageInfo.PackgesCategory3);

                    foreach (XPack pack in xpacks)
                    {
                        SetupBundlePackageType(pack, package1, package2, package3);
                    }
                    foreach (XScene scene in xscenes)
                    {
                        SetupBundlePackageType(scene, package1, package2, package3);
                    }
                }
            }
        }

        Debug.Log(string.Format("<color=red> XSheet save path {0} extraPackagePath {1} done. </color>", path, extraPackagePath));
#endif

        XSheetInfo fat = new XSheetInfo();
        fat.packs   = xpacks;
        fat.files   = xfiles;
        fat.scenes  = xscenes;

        System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Create, System.IO.FileAccess.Write);
        if (fs != null)
        {
            StringBuilder sb = new StringBuilder();
            System.IO.StringWriter writer = new System.IO.StringWriter(sb);
            LitJson.JsonMapper.ToJson(fat,
                                      new LitJson.JsonWriter(writer)
                                      {
                                          PrettyPrint = pretty
                                      });

            byte[] buff = Encoding.UTF8.GetBytes(sb.ToString());
            fs.Write(buff, 0, buff.Length);
            fs.Close();
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="cacheInScene"></param>
    /// <param name="cacheAllTime"></param>
    private void SetupBundleCacheType(XEntry entry, List<string> cacheInScene, List<string> cacheAllTime)
    {
        if (cacheInScene.Contains(entry.name))
        {
            entry.cacheType = XCacheType.InScene;
        }
        else if (cacheAllTime.Contains(entry.name))
        {
            entry.cacheType = XCacheType.AllTime;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="packageCatrgory1"></param>
    /// <param name="packageCatrgory2"></param>
    /// <param name="packageCatrgory3"></param>
    private void SetupBundlePackageType(XEntry entry, List<string> packageCatrgory1, List<string> packageCatrgory2, List<string> packageCatrgory3)
    {
        if (packageCatrgory1.Contains(entry.name))
        {
            entry.packType = XPackType.P1;
        }
        else if (packageCatrgory2.Contains(entry.name))
        {
            entry.packType = XPackType.P2;
        }
        else if (packageCatrgory3.Contains(entry.name))
        {
            entry.packType = XPackType.P3;
        }
    }

    public void DeleteLocal()
    {
        string extPath = System.IO.Path.Combine(Application.persistentDataPath, name);
        string cachePath = System.IO.Path.Combine(XProcessPack.OutCachePath, name);
        if (System.IO.File.Exists(extPath))
            System.IO.File.Delete(extPath);

        if (System.IO.File.Exists(cachePath))
            System.IO.File.Delete(cachePath);
    }

    public Queue<XEntryDiff> GenerateSheetDiff(XSheetInfo newSheet)
    {
        Queue<XEntryDiff> diffFat = new Queue<XEntryDiff>();

        // pack diff
        for (int i = 0; i < newSheet.packs.Count; i++)
        {
            bool diff = true;
            XPack pack = newSheet.packs[i];
            XPack localPack;
            if (packs.TryGetValue(pack.name, out localPack))
            {
                if (localPack.checksum == pack.checksum)
                {
                    diff = false;
                }
            }
				
            if (diff)
            {
                diffFat.Enqueue(new XEntryDiff() {
                    entryType = XEntryType.File, local = localPack, remote = pack 
                });
            }
        }

        // file diff
        for (int i = 0; i < newSheet.files.Count; i++)
        {
            bool diff = true;
            XFile file = newSheet.files[i];
            XFile localFile;
            if (files.TryGetValue(file.name, out localFile))
            {
                if (localFile.checksum == file.checksum)
                    diff = false;
            }

            if (diff)
            {
                diffFat.Enqueue(new XEntryDiff() {
                    entryType = XEntryType.Pack, local = localFile, remote = file 
                });
            }
                
        }

        // scene diff
        for (int i = 0; i < newSheet.scenes.Count; i++)
        {
            bool diff = true;
            XScene scene = newSheet.scenes[i];
            XScene localScene;
            if (scenes.TryGetValue(scene.name, out localScene))
            {
                if (localScene.bundleName == scene.bundleName)
                    diff = false;
            }

            if (diff)
            {
                diffFat.Enqueue(new XEntryDiff() { 
                    entryType = XEntryType.Scene, local = localScene, remote = scene 
                });
            }
        }

        return diffFat;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetLocalFileUrl(string path)
    {
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
                return "file:///" + path;
            default:
                return "file://" + path;
        }
    }
}
