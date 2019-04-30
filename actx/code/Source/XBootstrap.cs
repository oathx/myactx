using UnityEngine;
using UnityEngine.UI;
using SLua;

[CustomLuaClass]
public class XBootstrap : MonoBehaviour {
    /// <summary>
    /// 
    /// </summary>
    static XBootstrap instance;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static XBootstrap GetSingleton()
    {
        return instance;
    }

    /// <summary>
    /// 
    /// </summary>
    public Text     progressValue;
    public Text     version;
    public Text     progressState;
    public Image    progressBar;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ver"></param>
    public void SetVersion(string ver){
        if (version)
        {
            version.text = ver;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="text"></param>
    public void SetProgress(float progress, string text, string state)
    {
        if (progressBar)
            XStaticDOTween.DOFillAmount(progressBar, progress, 0.1f);

        if (progressValue)
        {
            progressValue.text = text;
        }

        if (progressState)
        {
            progressState.text = state;
        }
    }
	
	/// <summary>
	/// 
	/// </summary>
    void OnDestroy()
    {
        StopAllCoroutines();
        instance = null;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Clearup()
    {
        GameObject.Destroy(gameObject);
    }
}
