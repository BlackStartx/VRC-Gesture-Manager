using UnityEngine;
using UnityEngine.UIElements;

namespace BlackStartX.GestureManager.Library
{
    public static class GmgVisualMesh
    {
        public static class CenterRelative
        {
            public static Vector3 PositionOf(float x, float y, float w, float h) => new((x / 2 + 0.5f) * w, (y / 2 + 0.5f) * h, Vertex.nearZ);
        }
    }
}