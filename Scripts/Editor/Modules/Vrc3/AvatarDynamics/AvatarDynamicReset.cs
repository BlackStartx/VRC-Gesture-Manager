#if VRC_SDK_VRCSDK3
using System.Collections.Generic;
using System.Linq;
using BlackStartX.GestureManager.Library;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.PhysBone;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.AvatarDynamics
{
    public static class AvatarDynamicReset
    {
        private const string ContactManagerName = "ContactManager";
        private const string PhysBoneManagerName = "PhysBoneManager";

        private const VRCAvatarDescriptor.ColliderConfig.State Disabled = VRCAvatarDescriptor.ColliderConfig.State.Disabled;

        private static readonly Dictionary<HumanBodyBones, (System.Func<VRCAvatarDescriptor, VRCAvatarDescriptor.ColliderConfig>, string[])> Bones = new()
        {
            { HumanBodyBones.Head, (vrc => vrc.collider_head, new[] { "Head" }) },
            { HumanBodyBones.Chest, (vrc => vrc.collider_torso, new[] { "Torso" }) },
            { HumanBodyBones.LeftFoot, (vrc => vrc.collider_footL, new[] { "Foot", "FootL" }) },
            { HumanBodyBones.RightFoot, (vrc => vrc.collider_footR, new[] { "Foot", "FootR" }) },
            { HumanBodyBones.LeftHand, (vrc => vrc.collider_handL, new[] { "Hand", "HandL" }) },
            { HumanBodyBones.RightHand, (vrc => vrc.collider_handR, new[] { "Hand", "HandR" }) },
            { HumanBodyBones.LeftRingProximal, (vrc => vrc.collider_fingerRingL, new[] { "Finger", "FingerL", "FingerRing", "FingerRingL" }) },
            { HumanBodyBones.RightRingProximal, (vrc => vrc.collider_fingerRingR, new[] { "Finger", "FingerR", "FingerRing", "FingerRingR" }) },
            { HumanBodyBones.LeftIndexProximal, (vrc => vrc.collider_fingerIndexL, new[] { "Finger", "FingerL", "FingerIndex", "FingerIndexL" }) },
            { HumanBodyBones.RightIndexProximal, (vrc => vrc.collider_fingerIndexR, new[] { "Finger", "FingerR", "FingerIndex", "FingerIndexR" }) },
            { HumanBodyBones.LeftLittleProximal, (vrc => vrc.collider_fingerLittleL, new[] { "Finger", "FingerL", "FingerLittle", "FingerLittleL" }) },
            { HumanBodyBones.RightLittleProximal, (vrc => vrc.collider_fingerLittleR, new[] { "Finger", "FingerR", "FingerLittle", "FingerLittleR" }) },
            { HumanBodyBones.LeftMiddleProximal, (vrc => vrc.collider_fingerMiddleL, new[] { "Finger", "FingerL", "FingerMiddle", "FingerMiddleL" }) },
            { HumanBodyBones.RightMiddleProximal, (vrc => vrc.collider_fingerMiddleR, new[] { "Finger", "FingerR", "FingerMiddle", "FingerMiddleR" }) }
        };

        public static void CheckOrRestartManagers()
        {
            if (!ContactManager.Inst) RestartContactManager();
            if (!PhysBoneManager.Inst) RestartPhysBoneManager();
        }

        private static bool IsNotPersistent(Object uObject) => !UnityEditor.EditorUtility.IsPersistent(uObject);

        private static void RestartContactManager()
        {
            Object.DestroyImmediate(GameObject.Find($"/{ContactManagerName}"));
            var obj = new GameObject(ContactManagerName);
            Object.DontDestroyOnLoad(obj);
            obj.AddComponent<ContactManager>();
            foreach (var contact in Resources.FindObjectsOfTypeAll<ContactBase>().Where(IsNotPersistent)) GmgComponentUtility.RecreateComponent(contact);
        }

        private static void RestartPhysBoneManager()
        {
            Object.DestroyImmediate(GameObject.Find($"/{PhysBoneManagerName}"));
            var obj = new GameObject(PhysBoneManagerName);
            Object.DontDestroyOnLoad(obj);
            obj.AddComponent<PhysBoneManager>();
            PhysBoneManager.Inst.IsSDK = true;
            PhysBoneManager.Inst.Init();
            obj.AddComponent<PhysBoneGrabHelper>();
            foreach (var physBone in Resources.FindObjectsOfTypeAll<VRCPhysBoneBase>().Where(IsNotPersistent)) GmgComponentUtility.RecreateComponent(physBone);
        }

        public static void ReinstallAvatarColliders(ModuleVrc3 module)
        {
            var sEnumerable = module.Avatar.GetComponentsInChildren<VRCContactSender>().Where(sender => sender.hideFlags == HideFlags.HideAndDontSave);
            foreach (var sender in sEnumerable) Object.DestroyImmediate(sender);
            var avatar = module.AvatarAnimator.avatar;
            if (!avatar || !avatar.isHuman) return;
            foreach (var pair in Bones)
            {
                var transform = module.AvatarAnimator.GetBoneTransform(pair.Key);
                var collider = pair.Value.Item1(module.AvatarDescriptor);
                if (!transform || collider.state == Disabled) continue;
                Install(transform, collider, pair.Value.Item2);
            }
        }

        private static void Install(Transform transform, VRCAvatarDescriptor.ColliderConfig collider, string[] tags)
        {
            var sender = transform.gameObject.AddComponent<VRCContactSender>();
            sender.shapeType = ContactBase.ShapeType.Capsule;
            sender.hideFlags = HideFlags.HideAndDontSave;
            sender.position = collider.position;
            sender.rotation = collider.rotation;
            sender.collisionTags.AddRange(tags);
            sender.rootTransform = transform;
            sender.height = collider.height;
            sender.radius = collider.radius;
        }
    }
}
#endif