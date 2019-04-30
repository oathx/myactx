﻿using UnityEngine;

public class XScriptAssetUpdateObject : ScriptableObject
{
#if UNITY_EDITOR
    public event System.Action OnValuesUpdated;
    public bool autoUpdate = true;

    protected virtual void OnValidate()
    {
        if (autoUpdate)
        {
            UnityEditor.EditorApplication.update += NotifyOfUpdatedValues;
        }
    }

    public void NotifyOfUpdatedValues()
    {
        UnityEditor.EditorApplication.update -= NotifyOfUpdatedValues;
        if (OnValuesUpdated != null)
        {
            OnValuesUpdated();
        }
    }

#endif
}

