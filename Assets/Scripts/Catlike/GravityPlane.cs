using System;
using UnityEngine;

namespace Catlike
{
    public class GravityPlane : GravitySource
    {
        [SerializeField] private float _gravity = 9.81f;
        [SerializeField, Min(0.0f)] private float _range = 1.0f;


        public override Vector3 GetGravity(Vector3 position)
        {
            var trans = transform;
            var up = trans.up;
            var distance = Vector3.Dot(up, position - trans.position);
            if (distance > _range)
            {
                return Vector3.zero;
            }
            var g = _gravity;
            if (distance > 0f)
            {
                g *= 1f - distance / _range;
            }
            return -g * up;
        }

        private void OnDrawGizmos()
        {
            var trans = transform;
            var scale = trans.localScale;
            // Use the range to draw the gizmo above instead of the object's scale.
            scale.y = _range;
            Gizmos.matrix = Matrix4x4.TRS(trans.position, trans.rotation, scale);
            var size = new Vector3(1, 0, 1);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Vector3.zero, size);
            if (_range > 0.0f)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(Vector3.up, size);
            }
        }
    }
}