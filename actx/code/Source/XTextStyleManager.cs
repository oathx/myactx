using UnityEngine;
using System.Collections;

public class XTextStyleManager
{
    /// <summary>
    /// 
    /// </summary>
    public static XTextStyleManager Instance 
    { get; internal set; }

    /// <summary>
    /// 
    /// </summary>
    public XTextStyleSheetObject styleSheet;
    
    /// <summary>
    /// 
    /// </summary>
    static XTextStyleManager()
    {
        Instance = new XTextStyleManager();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Load()
    {
        XRes.LoadAsync<XTextStyleSheetObject>(typeof(XTextStyleSheetObject).Name.ToLower(), delegate(Object obj)
        {
            styleSheet = obj as XTextStyleSheetObject;
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public XTextStyleSheetObject.StyleData Get(string name)
    {
        if (styleSheet != null)
            return styleSheet.Get(name);
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sort"></param>
    /// <returns></returns>
    public XTextStyleSheetObject.StyleData GetBySort(int sort)
    {
        if (styleSheet != null)
            return styleSheet.GetBySort(sort);
        return null;
    }
}
