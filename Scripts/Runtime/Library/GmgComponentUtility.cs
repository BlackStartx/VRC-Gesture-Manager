using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BlackStartX.GestureManager.Library
{
    public static class GmgComponentUtility
    {
        public static void RecreateComponent<T>(T component) where T : Component => MoveComponent(component, component.gameObject);

        public static T MoveComponent<T>(T source, GameObject destination) where T : Component
        {
            var destinationComponent = destination.AddComponent(source.GetType());
            CopyComponent(source, destinationComponent);
            destinationComponent.GetComponentIndex();
            Object.DestroyImmediate(source);
            return (T)destinationComponent;
        }

        private static void CopyComponent<T>(T source, T destination) where T : Component
        {
            var type = source.GetType();
            destination.hideFlags = source.hideFlags;
            foreach (var field in type.GetFields().Where(IsCopyFriendly)) field.SetValue(destination, field.GetValue(source));
        }

        private static bool IsCopyFriendly(FieldInfo field) => !field.IsLiteral && !field.IsInitOnly;
    }
}