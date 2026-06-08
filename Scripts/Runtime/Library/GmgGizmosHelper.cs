using System;
using UnityEngine;

namespace BlackStartX.GestureManager.Library
{
    public static class GmgGizmosHelper
    {
        private static readonly Vector3 Center = Vector3.zero;
        private static readonly Vector3 Size = Vector3.one;

        private class Matrix : IDisposable
        {
            private readonly Matrix4x4 _matrix;

            public Matrix(Matrix4x4 matrix)
            {
                _matrix = Gizmos.matrix;
                Gizmos.matrix = matrix;
            }

            public void Dispose() => Gizmos.matrix = _matrix;
        }

        public static void DrawCube(Vector3 pos, Quaternion rotation, Vector3 sSize)
        {
            using (new Matrix(Matrix4x4.TRS(pos, rotation, sSize))) Gizmos.DrawCube(Center, Size);
        }

        public static void DrawSphere(Vector3 pos, Quaternion rotation, float radius) => DrawCapsule(pos, rotation, radius, height: 0f);

        public static void DrawCapsule(Vector3 pos, Quaternion rotation, float radius, float height)
        {
            var topPosVector = pos + rotation * new Vector3(0f, Mathf.Max(0f, height -= radius *= 2) * 0.5f, 0f);
            var botPosVector = pos + rotation * new Vector3(0f, -(Mathf.Max(0f, height) * 0.5f), 0f);
            var fQuaternion = rotation * Quaternion.Euler(180f, 0f, 0f);
            var sScale = new Vector3(radius, radius, radius);
            using (new Matrix(Matrix4x4.TRS(topPosVector, rotation, sScale))) Gizmos.DrawMesh(GmgRuntimeMesh.HemisphereMesh);
            using (new Matrix(Matrix4x4.TRS(botPosVector, fQuaternion, sScale))) Gizmos.DrawMesh(GmgRuntimeMesh.HemisphereMesh);
            if (Mathf.Max(0f, height) <= 0.0001f) return;
            var sVector = new Vector3(radius, Mathf.Max(0f, height), radius);
            using (new Matrix(Matrix4x4.TRS(pos, rotation, sVector))) Gizmos.DrawMesh(GmgRuntimeMesh.OpenCylinderMesh);
        }
    }
}