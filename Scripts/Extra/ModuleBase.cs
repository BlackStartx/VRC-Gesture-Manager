using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using VRC.SDKBase;

namespace GestureManager.Scripts.Extra
{
    public abstract class ModuleBase
    {
        private readonly VRC_AvatarDescriptor _avatarDescriptor;

        private List<string> _errorList = new List<string>();
        private List<string> _warningList = new List<string>();

        public readonly GameObject Avatar;
        public readonly Animator AvatarAnimator;
        public readonly GestureManager Manager;

        public abstract bool RequiresConstantRepaint { get; }

        protected int Right, Left;

        protected ModuleBase(GestureManager manager, VRC_AvatarDescriptor avatarDescriptor)
        {
            _avatarDescriptor = avatarDescriptor;

            Manager = manager;
            Avatar = avatarDescriptor.gameObject;
            AvatarAnimator = Avatar.GetComponent<Animator>();
        }

        public abstract void Update();
        public abstract void LateUpdate();
        public abstract void InitForAvatar();
        public abstract void Unlink();
        public abstract void EditorHeader();
        public abstract void EditorContent(object editor, VisualElement element);
        protected abstract void OnNewLeft(int left);
        protected abstract void OnNewRight(int right);
        public abstract AnimationClip GetFinalGestureByIndex(int gestureIndex);
        public abstract Animator OnCustomAnimationPlay(AnimationClip clip);
        public abstract bool HasGestureBeenOverridden(int gesture);
        public abstract void AddGestureToOverrideController(int gestureIndex, AnimationClip newAnimation);

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

        public bool IsPerfectDesc() => IsValidDesc() && _warningList.Count == 0;

        public IEnumerable<string> GetErrors() => _errorList;

        public IEnumerable<string> GetWarnings() => _warningList;
    }
}