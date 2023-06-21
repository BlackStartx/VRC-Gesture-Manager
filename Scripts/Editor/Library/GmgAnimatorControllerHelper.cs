using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace BlackStartX.GestureManager.Editor.Library
{
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
            var controller = CreateControllerWith(new AnimationClip { name = "[SELECT YOUR ANIMATION!]" });
            foreach (var clip in clips) AddMotion(controller, clip);
            return controller;
        }

        public static AnimatorController CreateControllerWith(AnimationClip clip)
        {
            var controller = new AnimatorController { layers = new[] { new AnimatorControllerLayer { stateMachine = new AnimatorStateMachine() } } };
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