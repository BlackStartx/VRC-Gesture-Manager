using System.Collections.Generic;
using UnityEngine;

namespace GestureManager.Scripts.Core
{
    /**
     * 	Hi!
     * 	You're a curious one? :3
     * 
     *  Anyway, you're looking at some of the methods of my Core Unity Library OwO
     *  I didn't included all of them (files and methods) because otherwise the UnityPackage would have been so much bigger >.<
     *
     *  P.S: Gmg stands for GestureManager UwU
     */
    public static class GmgMyAnimatorControllerHelper {

        public static IEnumerable<KeyValuePair<AnimationClip, AnimationClip>> GetOverrides(AnimatorOverrideController overrideController)
        {
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(overrides);
            return overrides;
        }
        
    }
}
