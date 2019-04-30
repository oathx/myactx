using UnityEngine;
using System.Collections;

/// <summary>
/// 
/// </summary>
public class XBufferAsset : ScriptableObject
{
    /// <summary>
    /// 
    /// </summary>
    private byte[]      buffer;

    /// <summary>
    /// 
    /// </summary>
    public byte[]       bytes
    {
        get { return buffer; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="length"></param>
    public void         init(int length)
    {
        buffer = new byte[length];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    public void         init(TextAsset text)
    {
        if (text != null)
            buffer = text.bytes;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bytes"></param>
    public void         init(byte[] bytes)
    {
        buffer = bytes;
    }
}
