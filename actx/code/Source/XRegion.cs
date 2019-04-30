using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRegion
{
    public enum XLanguageType
    {
        Chinese, English, Max
    }

    public static XLanguageType GetLanguageType()
    {
#if _LANGUAGE_CN
        return XLanguageType.Chinese;
#else
        return XLanguageType.English;
#endif
    }
}
