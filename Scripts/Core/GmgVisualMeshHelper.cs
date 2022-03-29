using UnityEngine;
using UnityEngine.Experimental.UIElements;
using Random = System.Random;

namespace GestureManager.Scripts.Core
{
    public static class GmgVisualMesh
    {
        // public static Vertex VertexOf(float x, float y) => VertexOfStrict(x, y);
        // public static Vertex VertexOf(Vector3 vector) => VertexOfStrict(vector.x, vector.y);
        // private static Vertex VertexOfStrict(float x, float y) => VertexOfStrict(new Vector3(x, y, Vertex.nearZ));
        // private static Vertex VertexOfStrict(Vector3 vector) => new Vertex {position = vector, tint = Color.white};

        public static class CenterRelative
        {
            // public static Vector3 PositionOf(float x, float y, float w, float h) => new Vector3((x / 2 + 0.5f) * w, (y / 2 + 0.5f) * h, Vertex.nearZ);
            // public static Vertex VertexOf(float x, float y, float w, float h) => VertexOfStrict((x / 2 + 0.5f) * w, (y / 2 + 0.5f) * h);
            // public static Vertex VertexOf(Vector3 vector, float w, float h) => VertexOfStrict((vector.x / 2 + 0.5f) * w, (vector.y / 2 + 0.5f) * h);
        }
    }
}