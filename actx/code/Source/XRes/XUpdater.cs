using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using ICSharpCode;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

public class XUpdater {
    public const float TIMEOUT = 10.0f;

    public enum Stage
    {
        None,
        LocalVersion,
        FetchVersion,
        FetchFat,
        CheckChange,
        DownloadRes,
        DownloadApk,
        ExternalRes,
        FetchNews,
        CheckDataIntegrity,
        Finish,
        DiskFull,
        LagNet,
        Unknow
    }

    public enum UpdateFlag { 
        None, 
        Game, 
        Resource 
    };

    public static UpdateFlag updateFlag = UpdateFlag.None;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <param name="progressCallback"></param>
    /// <returns></returns>
    public static IEnumerator DoCheckUpdate(string url, 
        Action<Stage, float, string> progressCallback) 
    {
        updateFlag = UpdateFlag.None;

        progressCallback(Stage.FetchVersion, 0.1f, string.Empty);

        float timeOut = 0.0f;
        
        WWW loader = new WWW(url);
        while (!loader.isDone)
        {
            timeOut = Math.Min(timeOut + Time.deltaTime, TIMEOUT);
            if (timeOut <= 0)
            {
                yield break;
            }
            else
            {
                progressCallback(Stage.FetchVersion, 0.1f + (timeOut / TIMEOUT) * 0.4f, string.Empty);
                yield return null;
            }
        }

        progressCallback(Stage.FetchVersion, 0.5f, string.Empty);
        yield return null;

        if (string.IsNullOrEmpty(loader.error))
        {

        }

    }

    public static IEnumerator DoUpdate(Action<Stage, float, string> progressCallback)
    {
        yield return XCoroutine.Run(XRes.Initialize(progressCallback));
    }
}