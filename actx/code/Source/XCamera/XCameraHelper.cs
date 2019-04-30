using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class XCameraHelper
{
    /// <summary>
    /// 
    /// </summary>
    public static XCameraConfigure confPvp;
    public static XCameraConfigure confPve;

    /// <summary>
    /// 
    /// </summary>
    public static XCameraYo yo;
    public static XCameraYo joy;

    /// <summary>
    /// 
    /// </summary>
    public static void Initializ()
    {
        string[] confList = new string[] {
            "data/camerapvp",
            "data/camerapve",
            "data/camerayo",
            "data/camerajoy"
        };

        LoadAsync(confList);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="confList"></param>
    public static void LoadAsync(string[] confList)
    {
        XRes.LoadMultiAsync(confList, delegate (Object[] objs)
        {
            confPvp = objs[0] as XCameraConfigure;
            if (confPvp)
                confPvp.Initialize();

            confPve = objs[1] as XCameraConfigure;
            if (confPve)
                confPve.Initialize();

            // joy config
            yo      = objs[2] as XCameraYo;
            joy     = objs[3] as XCameraYo;

#if UNITY_EDITOR
            foreach(Object obj in objs)
            {
                GLog.Log("[XCameraHelper:LoadAsync] " + obj);
            }
#endif
        });
    }
}
