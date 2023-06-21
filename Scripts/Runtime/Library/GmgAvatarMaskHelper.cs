using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlackStartX.GestureManager.Library
{
    public static class GmgAvatarMaskHelper
    {
        private static readonly List<AvatarMaskBodyPart> BodyParts = Enum.GetValues(typeof(AvatarMaskBodyPart)).Cast<AvatarMaskBodyPart>().Where(part => part != AvatarMaskBodyPart.LastBodyPart).ToList();

        public static AvatarMask CreateMaskWith(string name, IEnumerable<AvatarMaskBodyPart> array)
        {
            var mask = new AvatarMask { name = name };
            foreach (var part in BodyParts) mask.SetHumanoidBodyPartActive(part, false);
            foreach (var part in array) mask.SetHumanoidBodyPartActive(part, true);
            return mask;
        }

        public static AvatarMask CreateMaskWithout(string name, IEnumerable<AvatarMaskBodyPart> array)
        {
            var mask = new AvatarMask { name = name };
            foreach (var part in array) mask.SetHumanoidBodyPartActive(part, false);
            return mask;
        }

        public static AvatarMask CreateEmptyMask(string name) => CreateMaskWith(name, Array.Empty<AvatarMaskBodyPart>());
    }
}