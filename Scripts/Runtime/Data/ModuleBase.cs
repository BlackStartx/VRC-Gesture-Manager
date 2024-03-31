using System.Collections.Generic;
using BlackStartX.GestureManager.Modules;
using GmgAvatarDescriptor =
#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
    VRC.SDKBase.VRC_AvatarDescriptor;
#else
    UnityEngine.Component;
#endif
using UnityEngine;
using UnityEngine.UIElements;

namespace BlackStartX.GestureManager.Data
{
    public abstract class ModuleBase
    {
        private List<HumanBodyBones> _bones;
        private List<HumanBodyBones> Bones => _bones ??= PoseBones;
        public string Name => Avatar != null ? Avatar.name : null;

        private readonly Dictionary<HumanBodyBones, (Vector3, Quaternion)> _poseBones = new();

        private readonly GmgAvatarDescriptor _avatarDescriptor;

        private List<string> _errorList = new();
        private List<string> _warningList = new();

        public readonly GameObject Avatar;
        public readonly Animator AvatarAnimator;

        private TransformData _beforeEmote;
        public bool PlayingCustomAnimation { get; private set; }
        public AnimationClip CustomAnim;

        public ModuleSettings Settings;
        protected bool GestureDrag;
        protected int Right, Left;
        public bool Active;

        protected ModuleBase(GmgAvatarDescriptor avatarDescriptor)
        {
            _avatarDescriptor = avatarDescriptor;

            Avatar = avatarDescriptor.gameObject;
            AvatarAnimator = Avatar.GetComponent<Animator>();
        }

        public abstract void Update();
        public abstract void LateUpdate();
        public abstract void OnDrawGizmos();
        public abstract void InitForAvatar();
        public abstract void EditorHeader();
        public abstract void EditorContent(object editor, VisualElement element);
        protected abstract void Unlink();
        protected abstract void OnNewLeft(int left);
        protected abstract void OnNewRight(int right);
        public abstract string GetGestureTextNameByIndex(int gestureIndex);
        protected abstract Animator OnCustomAnimationPlay(AnimationClip clip);
        protected abstract List<HumanBodyBones> PoseBones { get; }

        public virtual bool IsInvalid() => !Avatar || !AvatarAnimator || !_avatarDescriptor;

        protected virtual List<string> CheckWarnings()
        {
            var warnings = new List<string>();
            if (GestureManager.ControlledAvatars.ContainsKey(Avatar)) warnings.Add("- The avatar is already controlled by another Gesture Manager!");
            return warnings;
        }

        protected virtual List<string> CheckErrors()
        {
            var errors = new List<string>();
            if (!Avatar) errors.Add("- The GameObject has been deleted!");
            else if (!Avatar.activeInHierarchy) errors.Add("- The GameObject is disabled!");
            if (!AvatarAnimator) errors.Add("- The model doesn't have any animator!");
            if (!_avatarDescriptor) errors.Add("- The VRC_AvatarDescriptor has been deleted!");
            return errors;
        }

        public bool IsValidDesc()
        {
            _errorList = CheckErrors();
            _warningList = CheckWarnings();
            return _errorList.Count == 0;
        }

        public void OnNewHand(GestureHand hand, int i)
        {
            if (hand == GestureHand.Left) OnNewLeft(i);
            else OnNewRight(i);
        }

        public void SavePose(Animator animator)
        {
            foreach (var bodyBone in Bones)
            {
                var boneTransform = animator.GetBoneTransform(bodyBone);
                if (boneTransform) _poseBones[bodyBone] = (boneTransform.localPosition, boneTransform.localRotation);
            }
        }

        public void SetPose(Animator animator)
        {
            foreach (var bodyBone in _poseBones.Keys)
            {
                var boneTransform = animator.GetBoneTransform(bodyBone);
                if (!boneTransform) continue;
                var (boneVector, boneQuaternion) = _poseBones[bodyBone];
                boneTransform.localRotation = boneQuaternion;
                boneTransform.localPosition = boneVector;
            }
        }

        public bool IsPerfectDesc() => IsValidDesc() && _warningList.Count == 0;

        public IEnumerable<string> GetErrors() => _errorList;

        public IEnumerable<string> GetWarnings() => _warningList;

        public void StopCustomAnimation() => SetCustomAnimation(clip: null, play: true, save: false, PlayingCustomAnimation);

        public void SetCustomAnimation(AnimationClip clip) => SetCustomAnimation(clip, play: false, save: true, PlayingCustomAnimation);

        public void PlayCustomAnimation(AnimationClip clip) => SetCustomAnimation(clip, play: true, save: true, PlayingCustomAnimation);

        private void SetCustomAnimation(AnimationClip clip, bool play, bool save, bool playing)
        {
            PlayingCustomAnimation = (PlayingCustomAnimation || play) && clip;
            if (save) CustomAnim = clip;
            var customAnimator = PlayingCustomAnimation || playing && !clip ? OnCustomAnimationPlay(clip) : null;
            if (!customAnimator) return;
            if (playing) _beforeEmote.ApplyTo(customAnimator.gameObject.transform);
            else _beforeEmote = new TransformData(customAnimator.gameObject.transform);
        }

        public void Connect(ModuleSettings set)
        {
            Active = true;
            Settings = set;
            InitForAvatar();
            GestureManager.ControlledAvatars[Avatar] = this;
        }

        public void Disconnect()
        {
            Unlink();
            Active = false;
            Settings = null;
            GestureManager.ControlledAvatars.Remove(Avatar);
        }
    }
}