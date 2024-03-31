using System;
using UnityEngine;
using UnityEngine.UIElements;
using CR = BlackStartX.GestureManager.Library.GmgVisualMesh.CenterRelative;

namespace BlackStartX.GestureManager.Library.VisualElements
{
    public class GmgCircleElement : VisualElement
    {
        private const float Tolerance = 0.00001f;
        private float _radialProgress = Mathf.PI * 2;

        private float _progress = 1f;

        public float Progress
        {
            set
            {
                if (Math.Abs(_progress - value) < Tolerance) return;
                _progress = value;
                _radialProgress = _progress * Mathf.PI * 2;
                MarkDirtyRepaint();
            }
        }

        private float _borderWidth = 2f;

        public float BorderWidth
        {
            set
            {
                if (Math.Abs(_borderWidth - value) < Tolerance) return;
                _borderWidth = value;
                MarkDirtyRepaint();
            }
        }

        private Color _borderColor;

        public Color BorderColor
        {
            set
            {
                if (_borderColor == value) return;
                _borderColor = value;
                MarkDirtyRepaint();
            }
        }

        private Color _vertexColor = Color.white;

        public Color VertexColor
        {
            set
            {
                if (_vertexColor == value) return;
                _vertexColor = value;
                MarkDirtyRepaint();
            }
        }

        private Color _centerColor;

        public Color CenterColor
        {
            set
            {
                if (_centerColor == value) return;
                _centerColor = value;
                MarkDirtyRepaint();
            }
        }

        public GmgCircleElement()
        {
            generateVisualContent += OnGenerateVisualContent;
        }

        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            PolyMesh(100, out var vertices, out var indices);
            var writeData = mgc.Allocate(vertices.Length, indices.Length);
            writeData.SetAllVertices(vertices);
            writeData.SetAllIndices(indices);
        }

        private void PolyMesh(int n, out Vertex[] vertices, out ushort[] indices)
        {
            if (_borderWidth != 0) BorderPolyMesh(n, out vertices, out indices);
            else SimplePolyMesh(n, out vertices, out indices);
        }

        private void SimplePolyMesh(int n, out Vertex[] vertices, out ushort[] indices)
        {
            var w = contentRect.width;
            var h = contentRect.height;

            vertices = new Vertex[n + 1];
            vertices[0] = new Vertex { position = CR.PositionOf(0, 0, w, h), tint = _centerColor };
            for (var i = 1; i < n + 1; i++)
            {
                var angle = _radialProgress * (i - 1) / (n - 1);
                vertices[i] = new Vertex { position = CR.PositionOf(Mathf.Sin(angle), -Mathf.Cos(angle), w, h), tint = _vertexColor };
            }

            indices = new ushort[n * 3];
            for (var i = 0; i < n - 1; i++)
            {
                indices[i * 3 + 0] = 0;
                indices[i * 3 + 1] = (ushort)(i + 1);
                indices[i * 3 + 2] = (ushort)(i + 2);
            }
        }

        private void BorderPolyMesh(int n, out Vertex[] vertices, out ushort[] indices)
        {
            var w = contentRect.width;
            var h = contentRect.height;
            var bw = contentRect.width - _borderWidth * 2;
            var bh = contentRect.height - _borderWidth * 2;

            vertices = new Vertex[n * 2 + 1];
            vertices[0] = new Vertex { position = CR.PositionOf(0, 0, w, h), tint = _centerColor };

            for (var i = 1; i < n + 1; i++)
            {
                var angle = _radialProgress * (i - 1) / (n - 1);
                vertices[i] = new Vertex { position = CR.PositionOf(Mathf.Sin(angle), -Mathf.Cos(angle), w, h), tint = _borderColor };
                vertices[n + i] = new Vertex { position = CR.PositionOf(Mathf.Sin(angle), -Mathf.Cos(angle), bw, bh) + new Vector3(_borderWidth, _borderWidth), tint = _vertexColor };
            }

            indices = new ushort[n * 6];
            for (var i = 0; i < n - 1; i++)
            {
                var ixn = i + n;
                indices[i * 3 + 0] = 0;
                indices[i * 3 + 1] = (ushort)(i + 1);
                indices[i * 3 + 2] = (ushort)(i + 2);
                indices[ixn * 3 + 0] = 0;
                indices[ixn * 3 + 1] = (ushort)(ixn + 1);
                indices[ixn * 3 + 2] = (ushort)(ixn + 2);
            }
        }
    }
}