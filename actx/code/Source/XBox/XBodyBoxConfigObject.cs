using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class XBodyBoxConfigObject : ScriptableObject
{
    // body box
    [SerializeField]
    public int BodyBoxOffsetX;
    [SerializeField]
    public int BodyBoxOffsetY;

    [SerializeField]
    public int BodyBoxWidth;
    [SerializeField]
    public int BodyBoxHeight;

    // body receive damage box
    [SerializeField]
    public List<XBodyReceiveDamageBox> ReceiveDamageBoxes = new List<XBodyReceiveDamageBox>();

    [System.Serializable]
    public class XBodyReceiveDamageBox
    {
        [SerializeField]
        public int BoxOffsetX;
        [SerializeField]
        public int BoxOffsetY;

        [SerializeField]
        public int BoxWidth;
        [SerializeField]
        public int BoxHeight;

        public bool IsEmpty()
        {
            return BoxWidth == 0 || BoxHeight == 0;
        }
    }
}

public class XBodyBoxConfigDataTemple<T>
{
    // body box
    [SerializeField]
    public Vector2 BodyBoxOffset;
    [SerializeField]
    public T BodyBoxWidth;
    [SerializeField]
    public T BodyBoxHeight;

    // body receive damage box
    [SerializeField]
    public List<XBodyReceiveDamageBox> ReceiveDamageBoxes = new List<XBodyReceiveDamageBox>();

    [System.Serializable]
    public class XBodyReceiveDamageBox
    {
        [SerializeField]
        public Vector2 BoxOffset;

        [SerializeField]
        public T BoxWidth;
        [SerializeField]
        public T BoxHeight;

    }
}

public class XBoxAttackData
{
    public Vector2 Offset = Vector2.zero;
    public float Width = 0.5f;
    public float Height = 0.5f;

    public bool IsFollowBone = false;
    public HumanBodyBones FollowBone = HumanBodyBones.LastBone;

    public static XBoxAttackData CreateAttackBoxData(int offsetX, int offsetY, int width, int height)
    {
        XBoxAttackData data = new XBoxAttackData();
        data.Offset = new Vector2(offsetX / XBoxComponent.FLOAT_CORRECTION, offsetY / XBoxComponent.FLOAT_CORRECTION);
        data.Width = width / XBoxComponent.FLOAT_CORRECTION;
        data.Height = height / XBoxComponent.FLOAT_CORRECTION;

        return data;
    }
}
