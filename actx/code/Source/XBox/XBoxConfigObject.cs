using UnityEngine;

[CreateAssetMenuAttribute(fileName = "generalbox", menuName = "Boxes/BoxConfig")]
[System.Serializable]
public class XBoxConfigObject : ScriptableObject
{
    public int OffsetX;
    public int OffsetY;

    public int Width;
    public int Height;
}
