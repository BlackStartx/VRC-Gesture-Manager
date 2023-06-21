using UnityEditor;
using UnityEngine;

namespace BlackStartX.GestureManager.Editor.Library
{
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