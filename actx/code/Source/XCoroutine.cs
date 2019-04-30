using UnityEngine;
using System.Collections;

/// <summary>
/// 
/// </summary>
public class XCoroutine
{
    /// <summary>
    /// 
    /// </summary>
    private class CoreCoroutine : MonoBehaviour {
    }

    /// <summary>
    /// 
    /// </summary>
    private static CoreCoroutine core = null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="function"></param>
    /// <returns></returns>
    public static Coroutine Run(IEnumerator function)
    {
        if (!core)
        {
            GameObject target = new GameObject(typeof(CoreCoroutine).Name);

            target.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(target);

            core = target.AddComponent<CoreCoroutine>();
        }

        return core.StartCoroutine(function);
    }

    /// <summary>
    /// 
    /// </summary>
    public static void StopAll()
    {
        if (!core)
            core.StopAllCoroutines();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="c"></param>
    public static void StopCoroutine(Coroutine c)
    {
        if (!core)
            core.StopCoroutine(c);
    }
}
