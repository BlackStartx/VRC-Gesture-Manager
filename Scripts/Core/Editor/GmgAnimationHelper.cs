using UnityEditor;
using UnityEngine;

namespace GestureManager.Scripts.Core.Editor
{
    /**
     * Hi, you're a curious one!
     * 
     * What you're looking at are some of the methods of my Unity Libraries.
     * They do not contains all the methods otherwise the UnityPackage would have been so much bigger.
     * 
     * And actually the "Clone Animation Asset" is way different... but i need only to clone frames and not other stuffs, so who cares ;3
     * 
     * P.S: Gmg stands for GestureManager~
     */
    public static class GmgAnimationHelper
    {
        public static AnimationClip CloneAnimationAsset(AnimationClip toClone)
        {
            var toRet = new AnimationClip
            {
                name = toClone.name,
                frameRate = toClone.frameRate,
                legacy = toClone.legacy,
                localBounds = toClone.localBounds,
                wrapMode = toClone.wrapMode
            };

            var curveBindings = AnimationUtility.GetCurveBindings(toClone);
            var referBindings = AnimationUtility.GetObjectReferenceCurveBindings(toClone);

            foreach (var binding in curveBindings) AnimationUtility.SetEditorCurve(toRet, binding, AnimationUtility.GetEditorCurve(toClone, binding));
            foreach (var binding in referBindings) AnimationUtility.SetObjectReferenceCurve(toRet, binding, AnimationUtility.GetObjectReferenceCurve(toClone, binding));

            return toRet;
        }
    }
}