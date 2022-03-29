using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GestureManager.Scripts.Core
{
    /**
     * Hi, you're a curious one!
     * 
     * What you're looking at are some of the methods of my Unity Libraries.
     * They do not contains all the methods otherwise the UnityPackage would have been so much bigger.
     * 
     * P.S: Gmg stands for GestureManager~
     */
    public static class GmgAvatarMaskHelper
    {
        private static readonly List<AvatarMaskBodyPart> BodyParts = Enum.GetValues(typeof(AvatarMaskBodyPart)).Cast<AvatarMaskBodyPart>().Where(part => part != AvatarMaskBodyPart.LastBodyPart).ToList();

        public static AvatarMask CreateMaskWith(string name, IEnumerable<AvatarMaskBodyPart> parts)
        {
            var mask = new AvatarMask {name = name};
            foreach (var part in BodyParts) mask.SetHumanoidBodyPartActive(part, false);
            foreach (var part in parts) mask.SetHumanoidBodyPartActive(part, true);
            return mask;
        }

        public static AvatarMask CreateMaskWithout(string name, IEnumerable<AvatarMaskBodyPart> parts)
        {
            var mask = new AvatarMask {name = name};
            foreach (var part in parts) mask.SetHumanoidBodyPartActive(part, false);
            return mask;
        }

        public static AvatarMask CreateEmptyMask(string name) => CreateMaskWith(name, Array.Empty<AvatarMaskBodyPart>());
    }
}