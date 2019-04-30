using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SLua;

/// <summary>
/// 
/// </summary>
public class XLanguage : MonoBehaviour
{
    static LuaFunction luaTextLocalizeFunc  = null;

    /// <summary>
    /// 
    /// </summary>
    public static void  Reinit()
    {
        luaTextLocalizeFunc = null;
    }

    [System.Serializable]
    public class FontOverride
    {
        public int size = 0;
    }

    /// <summary>
    /// 
    /// </summary>
    public string           langTextID;
    public FontOverride[]   fontOverride = new FontOverride[(int)XRegion.XLanguageType.Max];

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        Text text = GetComponent<Text>();
        if (text)
        {
            luaTextLocalizeFunc = LuaState.main.getFunction("resmng.LangText");
            if (luaTextLocalizeFunc != null)
            {
                text.text = (string)luaTextLocalizeFunc.call(langTextID);
            } 

            FontOverride fo = fontOverride[(int)XRegion.GetLanguageType()];
            if (fo.size > 0)
            {
                text.fontSize = fo.size;

                XTextStyleComponent textStyle = GetComponent<XTextStyleComponent>();
                if (textStyle)
                    textStyle.applySize = false;
            }
        }
    }
}
