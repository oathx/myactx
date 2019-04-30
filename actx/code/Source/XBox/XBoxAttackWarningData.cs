using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XBoxAttackWarningData
{
    public int CreateState = 0;
    public XBoxConfigObject Box = null;

    public void Clear()
    {
        CreateState = 0;
        Box = null;
    }

    public bool Valid()
    {
        return Box != null;
    }
}