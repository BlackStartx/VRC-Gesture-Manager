using UnityEditor.Animations;
using UnityEngine;

namespace BlackStartX.GestureManager.Editor.Library
{
    public static class GmgAnimatorControllerHelper
    {
        private static readonly Vector3 ExitPosition = new(50, 150);
        private static readonly Vector3 Position = new(30, 65);

        private const string Old = ".";
        private const string New = "_";

        public static AnimatorController CreateController()
        {
            var controller = new AnimatorController { layers = new[] { new AnimatorControllerLayer { stateMachine = new AnimatorStateMachine() } } };
            controller.layers[0].stateMachine.exitPosition = ExitPosition;
            return controller;
        }

        public static AnimatorController CreateControllerWith(Motion motion)
        {
            var controller = CreateController();
            AddMotion(controller, motion);
            return controller;
        }

        public static AnimatorState AddMotion(AnimatorController controller, Motion motion) => AddMotion(controller, motion, motion.name.Replace(Old, New));

        private static AnimatorState AddMotion(AnimatorController controller, Motion motion, string name)
        {
            var state = controller.layers[0].stateMachine.AddState(name, Position);
            state.motion = motion;
            return state;
        }
    }
}