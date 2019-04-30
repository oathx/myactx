using UnityEngine;
using SLua;

[CustomLuaClass]
public class XBoxAlive : MonoBehaviour {

    public XActorComponent.BoneNogPoint nogPoint;
    public AnimationClip camSkillReaction;
    public XBoxAnimationLinkConfigObject LinkConfig;
    public GameObject footstepFallAudio;
    public float SquatHeight = 1.2f;

    public void GenerateBoneNogPoint()
    {
        XBoxComponent bc = this.GetComponent<XBoxComponent>();
        if (bc)
        {
            nogPoint = bc.NogPoint;
            camSkillReaction = bc.CamSkillReaction;
            LinkConfig = bc.LinkConfig;
            footstepFallAudio = bc.FootstepFallAudio;
            SquatHeight = bc.SquatHeight;
        }
    }
}
