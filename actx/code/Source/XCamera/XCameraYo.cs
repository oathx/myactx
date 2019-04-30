using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public enum JoyStickStatus
{
    LEFT_DOWN = 1,
    DOWN, RIGHT_DOWN,
    LEFT,
    CENTER,
    RIGHT,
    LEFT_UP,
    UP,
    RIGHT_UP
}

/// <summary>
/// 
/// </summary>
[CreateAssetMenu]
public class XCameraYo : ScriptableObject
{
    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class YoSectionData
    {
        public float startAngle = 0;
        public float endAngle = 0;
        public JoyStickStatus outputCode = JoyStickStatus.CENTER;
    }

    /// <summary>
    /// 
    /// </summary>
    public float centerActive = 0;

    /// <summary>
    /// 
    /// </summary>
    public List<YoSectionData> sectionDatas = new List<YoSectionData>();
   
}
