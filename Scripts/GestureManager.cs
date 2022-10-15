using System.Collections.Generic;
using GestureManager.Scripts.Extra;
using UnityEngine;

namespace GestureManager.Scripts
{
    public class GestureManager : MonoBehaviour
    {
        public static readonly Dictionary<GameObject, ModuleBase> ControlledAvatars = new Dictionary<GameObject, ModuleBase>();
        public static bool InWebClientRequest;

        private TransformData _managerTransform;
        private TransformData _beforeEmote;
        private bool _drag;

        public List<ModuleBase> LastCheckedActiveModules = new List<ModuleBase>();
        public ModuleBase Module;

        public bool PlayingCustomAnimation { get; private set; }
        public AnimationClip customAnim;

        private void OnDisable() => UnlinkModule();

        private void OnDrawGizmos() => Module?.OnDrawGizmos();

        private void Update()
        {
            if (Module == null) return;
            if (Module.IsInvalid()) UnlinkModule();
            else ModuleUpdate();
        }

        private void ModuleUpdate()
        {
            if (_drag) _managerTransform.Difference(transform).AddTo(Module.Avatar.transform);
            _managerTransform = new TransformData(transform);
            Module.Update();
        }

        private void LateUpdate()
        {
            _managerTransform = new TransformData(transform);
            Module?.LateUpdate();
        }

        public void SetDrag(bool drag) => _drag = drag;

        public void UnlinkModule() => SetModule(null);

        public void SetModule(ModuleBase module)
        {
            if (Module != null)
            {
                Module.Unlink();
                Module.Active = false;
                ControlledAvatars.Remove(Module.Avatar);
            }

            Module = module;
            Module?.Avatar.transform.ApplyTo(transform);
            if (Module == null) return;

            Module.Active = true;
            Module.InitForAvatar();
            ControlledAvatars[module.Avatar] = module;
            _managerTransform = new TransformData(transform);
        }

        /*
         *  Custom Animation
         */

        public void StopCustomAnimation() => SetCustomAnimation(clip: null, play: true, save: false, PlayingCustomAnimation);

        public void SetCustomAnimation(AnimationClip clip) => SetCustomAnimation(clip, play: false, save: true, PlayingCustomAnimation);

        public void PlayCustomAnimation(AnimationClip clip) => SetCustomAnimation(clip, play: true, save: true, PlayingCustomAnimation);

        private void SetCustomAnimation(AnimationClip clip, bool play, bool save, bool playing)
        {
            PlayingCustomAnimation = (PlayingCustomAnimation || play) && clip != null;
            if (save) customAnim = clip;
            var customAnimator = PlayingCustomAnimation || playing && !clip ? Module.OnCustomAnimationPlay(clip) : null;
            if (!customAnimator) return;
            if (playing) _beforeEmote.ApplyTo(customAnimator.gameObject.transform);
            else _beforeEmote = new TransformData(customAnimator.gameObject.transform);
        }
    }
}