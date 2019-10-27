using System.Collections.Generic;
using UnityEngine;

namespace GestureManager.Scripts.Core
{
    public class MyAnimatorControllerHelper {

        public static List<KeyValuePair<AnimationClip, AnimationClip>> GetOverrides(AnimatorOverrideController overrideController)
        {
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(overrides);
            return overrides;
        }
        
    }
}
