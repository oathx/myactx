using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class XTextStyleSheetObject : ScriptableObject
{

    [System.Serializable]
    public class StyleData
    {
        public string name;

        [Header("Font")]
        public String fontFamily = String.Empty;
        public Color color = Color.white;
        public int fontSize = 20;
        public FontStyle fontStyle = FontStyle.Normal;

        [Header("Outline")]
        public bool outline = false;
        public Vector2 outlineDistance = new Vector2(2, 2);
        public Color outlineColor = Color.gray;

        [Header("Shadow")]
        public bool shadow = false;
        public Vector2 shadowDistance = new Vector2(2, 2);
        public Color shadowColor = Color.black;
        public bool shadowUseGraphicAlpha = true;

        [Header("Gradient")]
        public bool gradient = false;
        public Color gradientTopColor = Color.white;
        public Color gradientBottomColor = Color.black;
    }

    public List<StyleData> styleSheet = new List<StyleData>();
    private Dictionary<string, StyleData> _map = new Dictionary<string, StyleData>();

    void OnEnable()
    {
        for (int i = 0; i < styleSheet.Count; i++)
        {
            StyleData data = styleSheet[i];
            if (!string.IsNullOrEmpty(data.name))
                _map.Add(data.name, data);
        }
    }

    public StyleData Get(string name)
    {
        StyleData data;
        if (_map.TryGetValue(name, out data))
            return data;
        return null;
    }

    public StyleData GetBySort(int sort)
    {
        if (styleSheet.Count >= sort)
            return styleSheet[sort - 1];
        return null;
    }

#if UNITY_EDITOR
    public int GetIndex(string name)
    {
        for (int i = 0; i < styleSheet.Count; i++)
        {
            if (styleSheet[i].name.Equals(name))
                return i;
        }

        return 0;
    }

    public string[] GetNameList()
    {
        List<string> list = new List<string>();
        list.Add("none");
        for (int i = 0; i < styleSheet.Count; i++)
        {
            list.Add(styleSheet[i].name);
        }
        return list.ToArray();
    }
#endif
}
