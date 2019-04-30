using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XBoxAnimationLinkConfigObject : MonoBehaviour {
    public List<XAttackLevelAnimations> Links;

    [System.Serializable]
    public class XAttackLevelAnimations
    {
        public string AttackLevelName;
        public List<AnimationClip> Animations;
    }
}
