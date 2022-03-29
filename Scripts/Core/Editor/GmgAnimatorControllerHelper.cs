using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace GestureManager.Scripts.Core.Editor
{
    /**
     * Hi, you're a curious one!
     * 
     * What you're looking at are some of the methods of my Unity Libraries.
     * They do not contains all the methods otherwise the UnityPackage would have been so much bigger.
     * 
     * P.S: Gmg stands for GestureManager~
     */
    public static class GmgAnimatorControllerHelper
    {
        public static IEnumerable<KeyValuePair<AnimationClip, AnimationClip>> GetOverrides(AnimatorOverrideController overrideController)
        {
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(overrides);
            return overrides;
        }

        public static AnimatorController CreateControllerWith(IEnumerable<AnimationClip> clips)
        {
            var controller = CreateControllerWith(new AnimationClip {name = "Idle"});
            foreach (var clip in clips) AddMotion(controller, clip);
            return controller;
        }

        public static AnimatorController CreateControllerWith(AnimationClip clip)
        {
            var controller = new AnimatorController {layers = new[] {new AnimatorControllerLayer {stateMachine = new AnimatorStateMachine()}}};
            controller.AddMotion(clip);
            return controller;
        }

        private static void AddMotion(AnimatorController controller, Motion motion) => AddMotion(controller, motion, motion.name.Replace(".", "_"));

        private static void AddMotion(AnimatorController controller, Motion motion, string name)
        {
            var originalName = motion.name;
            motion.name = name;
            controller.AddMotion(motion);
            motion.name = originalName;
        }
    }
}