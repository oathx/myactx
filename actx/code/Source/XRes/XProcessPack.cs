using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using SevenZip.Compression.LZMA;

/// <summary>
/// 
/// </summary>
public class XProcessPack 
{
    public static XProcessPack Instance
    { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public static string OutCachePath;
    public static string PackPathPrefix = string.Empty;
    public static string BundleZipName = typeof(XProcessPack).Name.ToLower();
    /// <summary>
    /// 
    /// </summary>
    static XProcessPack() {
#if UNITY_ANDROID && !UNITY_EDITOR
		OutCachePath = Path.Combine(Application.persistentDataPath, typeof(XProcessPack).Name.ToLower());
#else
        OutCachePath = Path.Combine(Application.temporaryCachePath, typeof(XProcessPack).Name.ToLower());
#endif

        Instance = new XProcessPack();

        try
        {
            string streamPath = Application.streamingAssetsPath;
            string[] splits = streamPath.Split('!');
            if (splits != null && splits.Length == 2)
            {
                PackPathPrefix = splits[1];
                while (PackPathPrefix.StartsWith("\\") || PackPathPrefix.StartsWith("/") || PackPathPrefix.StartsWith("!"))
                {
                    PackPathPrefix = PackPathPrefix.Substring(1);
                }
            }

            if (!Directory.Exists(OutCachePath)) {
                Directory.CreateDirectory(OutCachePath);
            }
        }
        catch (System.Exception e) {
            Debug.Log(e.Message);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool CheckSheetOut()
    {
        return File.Exists(System.IO.Path.Combine(OutCachePath, XSheet.name));
    }
}
