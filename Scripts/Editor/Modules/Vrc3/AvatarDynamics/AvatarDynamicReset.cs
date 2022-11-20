#if VRC_SDK_VRCSDK3
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.AvatarDynamics
{
    public static class AvatarDynamicReset
    {
        private const string TriggerManagerName = "TriggerManager";
        private const string PhysBoneManagerName = "PhysBoneManager";

        public static void CheckSceneCollisions()
        {
            if (!ContactManager.Inst) ResumeContactManager();
            if (!PhysBoneManager.Inst) ResumePhysBoneManager();
        }

        private static void RecreateComponent<T>(T original) where T : Component
        {
            var type = original.GetType();
            var component = original.gameObject.AddComponent(type);
            foreach (var field in type.GetFields()) field.SetValue(component, field.GetValue(original));
            Object.DestroyImmediate(original);
        }

        private static void ResumeContactManager()
        {
            Object.DestroyImmediate(GameObject.Find(TriggerManagerName));
            var obj = new GameObject(TriggerManagerName);
            Object.DontDestroyOnLoad(obj);
            obj.AddComponent<ContactManager>();
            foreach (var contact in Resources.FindObjectsOfTypeAll<ContactBase>()) RecreateComponent(contact);
        }

        private static void ResumePhysBoneManager()
        {
            Object.DestroyImmediate(GameObject.Find(PhysBoneManagerName));
            var obj = new GameObject(PhysBoneManagerName);
            Object.DontDestroyOnLoad(obj);
            obj.AddComponent<PhysBoneManager>();
            PhysBoneManager.Inst.IsSDK = true;
            PhysBoneManager.Inst.Init();
            obj.AddComponent<PhysBoneGrabHelper>();
            foreach (var physBone in Resources.FindObjectsOfTypeAll<VRCPhysBoneBase>()) RecreateComponent(physBone);
        }
    }
}
#endif