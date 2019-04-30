using UnityEngine;
using System.Collections;

public class XBytecodeFilePicker
{
    static XBytecodeBigFileBufferAsset  asset;

    /// <summary>
    /// 
    /// </summary>
    public static string                AssetPath = "Bytecode/XBytecodeBigFileBufferAsset";

    /// <summary>
    /// 
    /// </summary>
    public static void                  InitPicker()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        asset = Resources.Load<XBytecodeBigFileBufferAsset>(XBytecodeFilePicker.AssetPath);
        if (asset == null)
        {
            Debug.LogError("Highest alert XBytecodeBigFileBufferAsset NULL");
            return;
        }

        Debug.Log("InitPicker Success!");
#endif
    }

#if UNITY_EDITOR
    public static string                bytecodeFilePickerPath = "Assets/Resources/bytecode/XBytecodeBigFileBufferAsset.asset";
#endif

    /// <summary>
    /// 
    /// </summary>
    public static void                  ReleasePicker()
    {
        asset = null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static byte[]                GetBytecodeByFileName(string fileName)
    {
        if (asset == null)
            return null;

        return InternalGetBytecodeByFileName(fileName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    static byte[]                       InternalGetBytecodeByFileName(string fileName)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        XBytecodeBigFileBufferAsset.FileSign sign = asset.FileSigns.Find(delegate(XBytecodeBigFileBufferAsset.FileSign obj)
            {
                return obj.FileName == fileName;
            });
        if (sign != null)
        {
            byte[] bytes = new byte[sign.Length];
            System.Array.Copy(asset.BigBytes, sign.Offset, bytes, 0, sign.Length);
            return bytes;
        }
        else{

            Debug.LogError("Highest alert!!! Can't find bytecode file :  " + fileName);
        }
#endif
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static XBufferAsset          LoadBytecodeAsset(string fileName)
    {
        XBufferAsset bufferAsset = null;

#if UNITY_ANDROID && !UNITY_EDITOR
        string name = fileName.ToLower();
        XSheet.XAssetInfo info = XSheet.Instance.Find(name);
		if (info != null && info.locationType == XSheet.XLocationType.Resource)
        {
            if (asset != null)
            {
				Debug.LogError(fileName);
                byte[] bytes = GetBytecodeByFileName(fileName);
                if (bytes != null)
                {
                    bufferAsset = ScriptableObject.CreateInstance<XBufferAsset>();
                    bufferAsset.init(bytes);
                }
            }
            else
            {
                bufferAsset = XRes.Load<XBufferAsset>(fileName);
            }
        }
        else
        {
            bufferAsset = XRes.Load<XBufferAsset>(fileName);
        }
#else
        bufferAsset = XRes.Load<XBufferAsset>(fileName);
#endif

        return bufferAsset;
    }
}
