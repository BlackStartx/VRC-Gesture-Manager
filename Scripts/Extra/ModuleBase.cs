using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase;

namespace GestureManager.Scripts.Extra
{
    public abstract class ModuleBase
    {
        private List<string> _errorList = new List<string>();
        private List<string> _warningList = new List<string>();

        protected readonly GestureManager Manager;
        public readonly VRC_AvatarDescriptor AvatarDescriptor;

        public readonly GameObject Avatar;
        public readonly Animator AvatarAnimator;

        public abstract bool LateBoneUpdate { get; }
        public abstract bool RequiresConstantRepaint { get; }

        protected ModuleBase(GestureManager manager, VRC_AvatarDescriptor avatarDescriptor)
        {
            Manager = manager;
            AvatarDescriptor = avatarDescriptor;

            Avatar = avatarDescriptor.gameObject;
            AvatarAnimator = Avatar.GetComponent<Animator>();
        }

        protected virtual List<string> CheckWarnings() => new List<string>();

        protected virtual List<string> CheckErrors()
        {
            var errors = new List<string>();
            if (GestureManager.ControlledAvatars.ContainsKey(Avatar)) errors.Add("- The avatar is already controlled by another Gesture Manager!");
            if (!Avatar) errors.Add("- The GameObject has been deleted!");
            else if (!Avatar.activeInHierarchy) errors.Add("- The GameObject is disabled!");
            if (!AvatarAnimator) errors.Add("- The model doesn't have any animator!");
            return errors;
        }

        public abstract void Update();
        public abstract void InitForAvatar();
        public abstract void Unlink();
        public abstract AnimationClip GetEmoteByIndex(int emoteIndex);
        public abstract AnimationClip GetFinalGestureByIndex(GestureHand hand, int gestureIndex);
        public abstract Animator OnCustomAnimationPlay(AnimationClip clip);
        public abstract void EditorHeader();
        public abstract void EditorContent(object editor, VisualElement element);
        public abstract void SetValues(bool onCustomAnimation, int left, int right, int emote);
        public abstract bool HasGestureBeenOverridden(int gesture);
        public abstract void AddGestureToOverrideController(int gestureIndex, AnimationClip newAnimation);

        public bool IsValidDesc()
        {
            _errorList = CheckErrors();
            _warningList = CheckWarnings();
            return _errorList.Count == 0;
        }

        public virtual bool IsInvalid()
        {
            return !Avatar || !AvatarAnimator || !AvatarDescriptor;
        }

        public bool IsPerfectDesc() => IsValidDesc() && _warningList.Count == 0;

        public IEnumerable<string> GetErrors() => _errorList;

        public IEnumerable<string> GetWarnings() => _warningList;
    }
}