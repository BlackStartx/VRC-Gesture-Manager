﻿#if VRC_SDK_VRCSDK3
using BlackStartX.GestureManager.Editor.Lib;
using UnityEngine;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.Tools
{
    public abstract class GmgDynamicFunction
    {
        private bool _value;
        protected internal abstract string Name { get; }
        protected abstract string Description { get; }
        protected internal abstract bool Active { get; }

        internal void Display(ModuleVrc3 module)
        {
            using (new GmgLayoutHelper.GuiBackground(Active ? Color.green : GUI.backgroundColor))
            using (new GUILayout.VerticalScope(GestureManagerStyles.EmoteError))
            {
                GUILayout.Label(Name, GestureManagerStyles.ToolHeader);
                GUILayout.Label(Description, GestureManagerStyles.SubHeader);
                GUILayout.Space(10);
                Gui(module);
            }
        }

        internal void OnUpdate(ModuleVrc3 module)
        {
            if (_value != (_value = Active)) module.UpdateRunning();
            if (_value) Update(module);
        }

        internal void OnLateUpdate(ModuleVrc3 module)
        {
            if (Active) LateUpdate(module);
        }

        internal void OnDrawGizmos()
        {
            if (Active) DrawGizmos();
        }

        protected internal abstract void Toggle(ModuleVrc3 module);

        protected abstract void Gui(ModuleVrc3 module);

        protected virtual void Update(ModuleVrc3 module)
        {
        }

        protected virtual void LateUpdate(ModuleVrc3 module)
        {
        }

        protected virtual void DrawGizmos()
        {
        }
    }
}
#endif