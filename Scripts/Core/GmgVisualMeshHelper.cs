using UnityEngine;
using UnityEngine.UIElements;
using Random = System.Random;

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
    public static class GmgVisualMesh
    {
        public static class CenterRelative
        {
            public static Vector3 PositionOf(float x, float y, float w, float h) => new Vector3((x / 2 + 0.5f) * w, (y / 2 + 0.5f) * h, Vertex.nearZ);
        }
    }
}