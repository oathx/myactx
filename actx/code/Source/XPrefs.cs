using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SLua;

[CustomLuaClass]
public sealed class XPrefs
{
    class PrefsData
    {
        public Dictionary<string, int> IntPrefs { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, string> 
            StrPrefs { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public PrefsData()
        {
            IntPrefs = new Dictionary<string, int>();
            StrPrefs = new Dictionary<string, string>();
        }
    }

    static PrefsData prefsData = null;

    /// <summary>
    /// 
    /// </summary>
    static XPrefs()
    {
        prefsData = new PrefsData();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static int GetInt(string key, int defaultValue = 0)
    {
        int value;
        if (prefsData.IntPrefs.TryGetValue(key, out value))
            return value;
        return defaultValue;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void SetInt(string key, int value)
    {
        prefsData.IntPrefs[key] = value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static string GetString(string key, string defaultValue = "")
    {
        string value;
        if (prefsData.StrPrefs.TryGetValue(key, out value))
            return value;
        return defaultValue;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void SetString(string key, string value)
    {
        prefsData.StrPrefs[key] = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public static void Load()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "preferences");
        if (System.IO.File.Exists(path))
        {
            System.IO.StreamReader sr = new System.IO.StreamReader(path);
            string content = sr.ReadToEnd();
            sr.Close();

            PrefsData data = LitJson.JsonMapper.ToObject<PrefsData>(content);
            if (data != null)
                prefsData = data;
        }

    }

    public static void Save()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "preferences");

        System.IO.FileStream fs = new System.IO.FileStream(path, 
            System.IO.FileMode.Create, System.IO.FileAccess.Write);
        if (fs != null)
        {
            StringBuilder sb = new StringBuilder();
            System.IO.StringWriter writer = new System.IO.StringWriter(sb);
            LitJson.JsonMapper.ToJson(prefsData,
                                      new LitJson.JsonWriter(writer)
                                      {
                                          PrettyPrint = true
                                      });

            byte[] buff = Encoding.UTF8.GetBytes(sb.ToString());
            fs.Write(buff, 0, buff.Length);
            fs.Close();
        }
        else
        {
            GLog.LogError("gamePrefs preferences file create error");
        }
    }
}
