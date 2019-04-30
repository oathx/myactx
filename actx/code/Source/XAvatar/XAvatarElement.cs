using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class XBoneWeightRecord
{
    public string BoneName;
    public int WeightIndex;
}

/// <summary>
/// 
/// </summary>
public class XAvatarElement : XScriptAssetUpdateObject
{
    [SerializeField]
    public string Name;

    [SerializeField]
    public Object Prefab;

    [SerializeField]
    public Matrix4x4 SmrLocalToWorldMatrix;

    [SerializeField]
    public List<string> BoneNames;

    [SerializeField]
    public List<Material> SharedMaterials;

    [SerializeField]
    public List<Matrix4x4> BindPoses;

    [SerializeField]
    public List<XBoneWeightRecord> BoneWeights;

    public Dictionary<string, int> GenBoneWeightsDic()
    {
        Dictionary<string, int> boneWeightDic = new Dictionary<string, int>();

        if (BoneWeights != null)
        {
            for (int i = 0; i < BoneWeights.Count; i++)
            {
                boneWeightDic[BoneWeights[i].BoneName] = BoneWeights[i].WeightIndex;
            }
        }

        return boneWeightDic;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="element"></param>
    public void Clone(XAvatarElement element)
    {
        Name = element.Name;
        Prefab = element.Prefab;
        SmrLocalToWorldMatrix = element.SmrLocalToWorldMatrix;

        if (element.BoneNames != null)
            BoneNames = new List<string>(element.BoneNames);

        if (element.SharedMaterials != null)
            SharedMaterials = new List<Material>(element.SharedMaterials);

        if (element.BindPoses != null)
            BindPoses = new List<Matrix4x4>(element.BindPoses);

        if (element.BoneWeights != null)
            BoneWeights = new List<XBoneWeightRecord>(element.BoneWeights);
    }
}
