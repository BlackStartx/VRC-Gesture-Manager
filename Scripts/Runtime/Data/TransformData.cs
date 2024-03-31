using UnityEngine;

namespace BlackStartX.GestureManager.Data
{
    public class TransformData
    {
        private readonly Vector3 _position;
        private readonly Quaternion _rotation;
        private readonly Vector3 _localScale;

        private TransformData(Vector3 p, Quaternion r, Vector3 s)
        {
            _position = p;
            _rotation = r;
            _localScale = s;
        }

        public TransformData(Transform t) : this(t.position, t.rotation, t.localScale)
        {
        }

        public void AddTo(Transform t)
        {
            t.position += _position;
            t.rotation = _rotation * t.rotation;
            t.localScale += _localScale;
        }

        public void ApplyTo(Transform t)
        {
            t.position = _position;
            t.rotation = _rotation;
            t.localScale = _localScale;
        }

        public TransformData Difference(Transform t) => new(t.position - _position, t.rotation * Quaternion.Inverse(_rotation), t.localScale - _localScale);
    }
}