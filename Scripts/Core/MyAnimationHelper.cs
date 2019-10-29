using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GestureManager.Scripts.Core
{
    /**
     * 	Hi!
     * 	You're a curious one? :3
     * 
     *  Anyway, you're looking at some of the methods of my Core Unity Library OwO
     *  I didn't included all of them (files and methods) because otherwise the UnityPackage would have been so much bigger >.<
     *  And actually the "Clone Animation Asset" is way different... but i need only to clone frames and not other stuffs, so... OwO 
     */
    public class MyAnimationHelper : MonoBehaviour
    {
        public static AnimationClip CloneAnimationAsset(AnimationClip toClone)
        {
            var toRet = new AnimationClip
            {
                name = toClone.name,
                frameRate = toClone.frameRate,
                legacy = toClone.legacy,
                localBounds = toClone.localBounds,
                wrapMode = toClone.wrapMode,
            };

            var curveBindings = AnimationUtility.GetCurveBindings(toClone);
            var referBindings = AnimationUtility.GetObjectReferenceCurveBindings(toClone);

            foreach (var bind in curveBindings) AnimationUtility.SetEditorCurve(toRet, bind, AnimationUtility.GetEditorCurve(toClone, bind));
            foreach (var bind in referBindings) AnimationUtility.SetObjectReferenceCurve(toRet, bind, AnimationUtility.GetObjectReferenceCurve(toClone, bind));

            return toRet;
        }
    }
}
