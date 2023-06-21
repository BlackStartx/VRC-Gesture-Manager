#if VRC_SDK_VRCSDK3
using System;
using UnityEngine;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.Cache
{
    [Serializable]
    public class AvatarFile
    {
        public Parameters[] animationParameters;

        public static AvatarFile LoadData(string data)
        {
            try
            {
                return JsonUtility.FromJson<AvatarFile>(data);
            }
            catch
            {
                return null;
            }
        }
    }

    [Serializable]
    public class Parameters
    {
        public string name;
        public float value;
    }
}
#endif