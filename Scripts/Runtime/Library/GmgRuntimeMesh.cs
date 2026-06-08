using UnityEngine;

namespace BlackStartX.GestureManager.Library
{
    public static class GmgRuntimeMesh
    {
        private static Mesh _hemisphereMesh;
        public static Mesh HemisphereMesh => !_hemisphereMesh ? _hemisphereMesh = GetHemisphereMesh() : _hemisphereMesh;

        private static Mesh _openCylinderMesh;
        public static Mesh OpenCylinderMesh => !_openCylinderMesh ? _openCylinderMesh = GetOpenCylinderMesh() : _openCylinderMesh;

        private static float F0(int rin, int r) => Mathf.PI * 0.5f * r / rin;

        private static float F2(int seg, int s) => Mathf.PI * 2f * s / seg;

        private static Mesh GetHemisphereMesh(int seg = 24, int rin = 8)
        {
            var verts = new Vector3[(seg + 1) * (rin + 1)];
            var tris = new int[seg * rin * 6];
            const int six = 6;

            for (var r = 0; r <= rin; r++)
            for (var s = 0; s <= seg; s++)
                verts[r * (seg + 1) + s] = HemVerts(F2(seg, s), F0(rin, r));

            for (var r = 0; r < rin; r++)
            for (var s = 0; s < seg; s++)
            for (var i = 0; i < six; i++)
                tris[(r * seg + s) * 6 + i] = HemTris(seg, r, s, i);

            var mesh = new Mesh { name = "[Gesture Manager] Hemisphere", vertices = verts, triangles = tris };
            mesh.RecalculateNormals();
            return mesh;
        }

        private static Vector3 HemVerts(float f2, float f0) => new(Mathf.Cos(f2) * Mathf.Sin(f0) * 0.5f, Mathf.Cos(f0) * 0.5f, Mathf.Sin(f2) * Mathf.Sin(f0) * 0.5f);

        private static int HemTris(int seg, int r, int s, int i) => r * (seg + 1) + s + (i != 0 && i != 2 && i != 5 ? 1 : 0) + (i >= 2 && i != 3 ? seg + 1 : 0);

        private static Mesh GetOpenCylinderMesh(int seg = 24)
        {
            var verts = new Vector3[(seg + 1) * 2];
            var tris = new int[seg * 6];
            const int one = 1;
            const int six = 6;

            for (var s = 0; s <= seg; s++)
            for (var i = 0; i <= one; i++)
                verts[s * 2 + i] = CylVert(F2(seg, s), i);

            for (var s = 0; s < seg; s++)
            for (var i = 0; i < six; i++)
                tris[s * 6 + i] = CylTris(s, i);

            var mesh = new Mesh { name = "[Gesture Manager] Open Cylinder", vertices = verts, triangles = tris };
            mesh.RecalculateNormals();
            return mesh;
        }

        private static Vector3 CylVert(float f2, int i) => new(Mathf.Cos(f2) * 0.5f, i - 0.5f, Mathf.Sin(f2) * 0.5f);

        private static int CylTris(int s, int i) => s * 2 + (i != 0 && i != 2 && i != 5 ? 1 : 0) + (i >= 2 && i != 3 ? 2 : 0);
    }
}