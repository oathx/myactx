using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum XHugType
{
    Normal,
    Super,
    SuperIgnoreReaction,
}

[System.Serializable]
public class XBoxHugConfigObject : XBoxConfigObject
{
    public XHugType HugType;
}


