using System.Collections.Generic;
using BlackStartX.GestureManager.Data;
using BlackStartX.GestureManager.Library;
using BlackStartX.GestureManager.Modules;
using JetBrains.Annotations;
using UnityEngine;

namespace BlackStartX.GestureManager
{
    public class GestureManager : MonoBehaviour
    {
        public const string Version = "Gesture Manager 3.9";
        public static readonly Dictionary<GameObject, ModuleBase> ControlledAvatars = new();
        public static List<ModuleBase> LastCheckedActiveModules = new();
        public static bool InWebClientRequest;

        private TransformData _managerTransform;
        private bool _drag;

        public ModuleBase Module;
        public ModuleSettings settings;

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

        public void UnlinkModule()
        {
            if (Module == null) return;
            Module.Disconnect();
            Module = null;
        }

        public void SetModule([NotNull] ModuleBase module)
        {
            if (!module.IsValidDesc()) return;

            Module?.Disconnect();
            Module = module;
            Module.Avatar.transform.ApplyTo(transform);

            Module.Connect(settings);
            _managerTransform = new TransformData(transform);
        }
    }
}