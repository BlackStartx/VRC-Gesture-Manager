using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GestureManager.Scripts.Core
{
    /**
     * Hi, you're a curious one!
     * 
     * What you're looking at are some of the methods of my Unity Libraries.
     * They do not contains all the methods otherwise the UnityPackage would have been so much bigger.
     * 
     * Those methods are currently unused and will probably be deleted soon since Unity 2019 finally added a toggle button for Gizmos.
     * 
     * P.S: Gmg stands for GestureManager~
     */
    public static class GmgGizmosHelper
    {
        public static void SavePreset(string name)
        {
            var annotationUtility = Type.GetType("UnityEditor.AnnotationUtility, UnityEditor");
            var savePreset = annotationUtility?.GetMethod("SavePreset", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            savePreset?.Invoke(null, new object[] {name});
        }

        private static IEnumerable<string> GetPresetList()
        {
            var annotationUtility = Type.GetType("UnityEditor.AnnotationUtility, UnityEditor");
            var savePreset = annotationUtility?.GetMethod("GetPresetList", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            return savePreset?.Invoke(null, new object[] { }) as string[];
        }

        public static bool HavePreset(string name)
        {
            return GetPresetList().Contains(name);
        }

        public static void LoadPreset(string name)
        {
            if (name == null) return;

            var annotationUtility = Type.GetType("UnityEditor.AnnotationUtility, UnityEditor");
            var loadPreset = annotationUtility?.GetMethod("LoadPreset", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            loadPreset?.Invoke(null, new object[] {name});
        }

        public static void DeletePreset(string name)
        {
            if (name == null) return;

            var annotationUtility = Type.GetType("UnityEditor.AnnotationUtility, UnityEditor");
            var deletePreset = annotationUtility?.GetMethod("DeletePreset", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            deletePreset?.Invoke(null, new object[] {name});
        }

        public static void DisableAllGizmos()
        {
            var annotation = Type.GetType("UnityEditor.Annotation, UnityEditor");
            var classId = annotation?.GetField("classID");
            var scriptClass = annotation?.GetField("scriptClass");

            var annotationUtility = Type.GetType("UnityEditor.AnnotationUtility, UnityEditor");
            var getAnnotations = annotationUtility?.GetMethod("GetAnnotations", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            var setGizmosEnabled = annotationUtility?.GetMethod("SetGizmoEnabled", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            var setIconEnabled = annotationUtility?.GetMethod("SetIconEnabled", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            var annotations = (Array) getAnnotations?.Invoke(null, null);
            if (annotations == null) return;
            foreach (var a in annotations)
            {
                var id = (int) (classId?.GetValue(a) ?? 0);
                var script = (string) scriptClass?.GetValue(a);

                setGizmosEnabled?.Invoke(null, new object[] {id, script, 0});
                setIconEnabled?.Invoke(null, new object[] {id, script, 0});
            }
        }
    }
}