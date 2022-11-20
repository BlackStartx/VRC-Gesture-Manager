using System.Collections.Generic;
using GmgAvatarDescriptor =
#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
    VRC.SDKBase.VRC_AvatarDescriptor;
#else
    UnityEngine.Component;
#endif
using UnityEngine;
using UnityEngine.UIElements;

namespace BlackStartX.GestureManager.Runtime.Extra
{
    public abstract class ModuleBase
    {
        private List<HumanBodyBones> _bones;
        private List<HumanBodyBones> Bones => _bones ?? (_bones = PoseBones);

        private readonly Dictionary<HumanBodyBones, (Vector3, Quaternion)> _poseBones = new Dictionary<HumanBodyBones, (Vector3, Quaternion)>();

        private readonly GmgAvatarDescriptor _avatarDescriptor;

        private List<string> _errorList = new List<string>();
        private List<string> _warningList = new List<string>();

        public readonly GameObject Avatar;
        public readonly Animator AvatarAnimator;
        public readonly GestureManager Manager;

        protected int Right, Left;
        protected bool GestureDrag;
        public bool Active;

        protected ModuleBase(GestureManager manager, GmgAvatarDescriptor avatarDescriptor)
        {
            _avatarDescriptor = avatarDescriptor;

            Manager = manager;
            Avatar = avatarDescriptor.gameObject;
            AvatarAnimator = Avatar.GetComponent<Animator>();
        }

        public abstract void Update();
        public abstract void LateUpdate();
        public abstract void OnDrawGizmos();
        public abstract void InitForAvatar();
        public abstract void Unlink();
        public abstract void EditorHeader();
        public abstract void EditorContent(object editor, VisualElement element);
        protected abstract void OnNewLeft(int left);
        protected abstract void OnNewRight(int right);
        public abstract string GetGestureTextNameByIndex(int gestureIndex);
        public abstract Animator OnCustomAnimationPlay(AnimationClip clip);
        public abstract bool HasGestureBeenOverridden(int gesture);
        protected abstract List<HumanBodyBones> PoseBones { get; }

        public virtual bool IsInvalid() => !Avatar || !AvatarAnimator || !_avatarDescriptor;

        protected virtual List<string> CheckWarnings() => new List<string>();

        protected virtual List<string> CheckErrors()
        {
            var errors = new List<string>();
            if (GestureManager.ControlledAvatars.ContainsKey(Avatar)) errors.Add("- The avatar is already controlled by another Gesture Manager!");
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
    }
}