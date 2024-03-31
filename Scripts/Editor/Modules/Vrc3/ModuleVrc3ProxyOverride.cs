#if VRC_SDK_VRCSDK3
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BlackStartX.GestureManager.Editor.Data;
using UnityEngine;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3
{
    public static class Vrc3ProxyOverride
    {
        public static RuntimeAnimatorController OverrideController(RuntimeAnimatorController controller)
        {
            if (!controller) return controller;
            var overrideController = new AnimatorOverrideController(controller);
            overrideController.ApplyOverrides(controller.animationClips.Select(OverrideClip).Where(pair => pair.Value).ToList());
            return overrideController;
        }

        private static KeyValuePair<AnimationClip, AnimationClip> OverrideClip(AnimationClip clip)
        {
            OverrideOf.TryGetValue(clip ? clip.name : "", out var oClip);
            return new KeyValuePair<AnimationClip, AnimationClip>(clip, oClip);
        }

        private static Dictionary<string, AnimationClip> _overrideOf;

        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        private static Dictionary<string, AnimationClip> OverrideOf => _overrideOf ??= new Dictionary<string, AnimationClip>
        {
            // Locomotion
            { "proxy_low_crawl_still", null },
            { "proxy_low_crawl_forward", null },
            { "proxy_low_crawl_right", null },
            { "proxy_sprint_forward", null },
            { "proxy_run_forward", null },
            { "proxy_walk_forward", null },
            { "proxy_stand_still", null },
            { "proxy_walk_backward", null },
            { "proxy_run_backward", null },
            { "proxy_strafe_right", null },
            { "proxy_strafe_right_45", null },
            { "proxy_strafe_right_135", null },
            { "proxy_run_strafe_right", null },
            { "proxy_run_strafe_right_45", null },
            { "proxy_run_strafe_right_135", null },
            { "proxy_crouch_still", null },
            { "proxy_crouch_walk_forward", null },
            { "proxy_crouch_walk_right", null },
            { "proxy_crouch_walk_right_45", null },
            { "proxy_crouch_walk_right_135", null },
            // Fall & Landing
            { "proxy_fall_short", null },
            { "proxy_landing", null },
            { "proxy_fall_long", null },
            { "proxy_land_quick", null },
            { "proxy_idle", null },
            // Gesture
            { "proxy_hands_idle", null },
            { "proxy_hands_fist", GestureManagerStyles.Animations.Gesture.Fist },
            { "proxy_hands_open", GestureManagerStyles.Animations.Gesture.Open },
            { "proxy_hands_point", GestureManagerStyles.Animations.Gesture.Point },
            { "proxy_hands_peace", GestureManagerStyles.Animations.Gesture.Peace },
            { "proxy_hands_rock", GestureManagerStyles.Animations.Gesture.Rock },
            { "proxy_hands_gun", GestureManagerStyles.Animations.Gesture.Gun },
            { "proxy_hands_thumbs_up", GestureManagerStyles.Animations.Gesture.ThumbsUp },
            // Emotes [Standing]
            { "proxy_stand_wave", GestureManagerStyles.Animations.Emote.Standing.Wave },
            { "proxy_stand_clap", GestureManagerStyles.Animations.Emote.Standing.Clap },
            { "proxy_stand_point", GestureManagerStyles.Animations.Emote.Standing.Point },
            { "proxy_stand_cheer", GestureManagerStyles.Animations.Emote.Standing.Cheer },
            { "proxy_dance", GestureManagerStyles.Animations.Emote.Standing.Dance },
            { "proxy_backflip", GestureManagerStyles.Animations.Emote.Standing.BackFlip },
            { "proxy_die", GestureManagerStyles.Animations.Emote.Standing.Die },
            { "proxy_stand_sadkick", GestureManagerStyles.Animations.Emote.Standing.SadKick },
            // Emotes [Seated]
            { "proxy_seated_laugh", GestureManagerStyles.Animations.Emote.Seated.Laugh },
            { "proxy_seated_point", GestureManagerStyles.Animations.Emote.Seated.Point },
            { "proxy_seated_raise_hand", GestureManagerStyles.Animations.Emote.Seated.RaiseHand },
            { "proxy_seated_drum", GestureManagerStyles.Animations.Emote.Seated.Drum },
            { "proxy_seated_clap", GestureManagerStyles.Animations.Emote.Seated.Clap },
            { "proxy_seated_shake_fist", GestureManagerStyles.Animations.Emote.Seated.ShakeFist },
            { "proxy_seated_disbelief", GestureManagerStyles.Animations.Emote.Seated.Disbelief },
            { "proxy_seated_disapprove", GestureManagerStyles.Animations.Emote.Seated.Disapprove },
            // Cool Animations
            { "proxy_afk", null },
            { "proxy_sit", null },
            { "proxy_tpose", null },
            { "proxy_ikpose", null },
            // Extra
            { "proxy_supine_getup", null },
            { "proxy_eyes_die", null },
            { "proxy_eyes_open", null },
            { "proxy_eyes_shut", null },
            { "proxy_mood_neutral", null },
            { "proxy_mood_happy", null },
            { "proxy_mood_surprised", null },
            { "proxy_mood_sad", null },
            { "proxy_mood_angry", null }
        };
    }
}
#endif