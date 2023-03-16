using System;
using UnityEngine;

namespace Catlike
{
    public class GravityBox : GravitySource
    {
        [SerializeField] private float _gravity = 9.81f;
        
        [Tooltip("Distances from the center straight to the faces")]
        [SerializeField] private Vector3 _boundaryDistance = Vector3.one;
        
        [SerializeField, Min(0f)] private float _innerDistance = 0f;
        [SerializeField, Min(0f)] private float _innerFalloffDistance = 0f;
        [SerializeField, Min(0f)] private float _outerDistance = 0f;
        [SerializeField, Min(0f)] private float _outerFalloffDistance = 0f;

        private void Awake()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            _outerFalloffDistance = Mathf.Max(_outerFalloffDistance, _outerDistance);
            _boundaryDistance = Vector3.Max(_boundaryDistance, Vector3.zero);
            var minDimension = Mathf.Min(_boundaryDistance.x, _boundaryDistance.y, _boundaryDistance.z);
            _innerDistance = Mathf.Min(_innerDistance, minDimension);
            _innerFalloffDistance = Mathf.Max(Mathf.Min(_innerFalloffDistance, minDimension), _innerDistance);
        }

        private float GetGravityComponent(float coordinateRelativeToBoxCenter, float distanceToNearestFace)
        {
            if (distanceToNearestFace > _innerFalloffDistance)
            {
                return 0f;
            }
            var result = _gravity;
            if (distanceToNearestFace > _innerDistance)
            {
                var innerFalloffFactor = 1f / (_innerFalloffDistance - _innerDistance);
                result *= 1f - (distanceToNearestFace - _innerDistance) * innerFalloffFactor;
            }
            return coordinateRelativeToBoxCenter > 0 ? -result : result;
        }

        public override Vector3 GetGravity(Vector3 position)
        {
            // Get box relative position. 
            var boxRelativePosition = position - transform.position;
            // InverseTransformDirection is used to support arbitrary box rotation.
            position = transform.InverseTransformDirection(boxRelativePosition);
            var vector = Vector3.zero;
            // Determine outside face count and vector
            var outside = 0;
            if (position.x > _boundaryDistance.x)
            {
                vector.x = _boundaryDistance.x - position.x;
                outside = 1;
            }
            else if (position.x < -_boundaryDistance.x)
            {
                vector.x = -_boundaryDistance.x - position.x;
                outside = 1;
            }
            if (position.y > _boundaryDistance.y)
            {
                vector.y = _boundaryDistance.y - position.y;
                outside += 1;
            }
            else if (position.y < -_boundaryDistance.y)
            {
                vector.y = -_boundaryDistance.y - position.y;
                outside += 1;
            }
            if (position.z > _boundaryDistance.z)
            {
                vector.z = _boundaryDistance.z - position.z;
                outside += 1;
            }
            else if (position.z < -_boundaryDistance.z)
            {
                vector.z = -_boundaryDistance.z - position.z;
                outside += 1;
            }

            if (outside > 0)
            {
                // Micro optimization in case of being outside of only one face.
                var distance = outside == 1 ? Mathf.Abs(vector.x + vector.y + vector.z) : vector.magnitude;                
                if (distance > _outerFalloffDistance)
                {
                    return Vector3.zero;
                }
                var g = _gravity / distance;
                if (distance > _outerDistance)
                {
                    var outerFalloffFactor = 1 / (_outerFalloffDistance - _outerDistance);
                    g *= 1f - (distance - _outerDistance) * outerFalloffFactor;
                }
                return transform.TransformDirection(g * vector);
            }
            
            // Determine absolute distance.
            Vector3 distances;
            distances.x = _boundaryDistance.x - Mathf.Abs(position.x);
            distances.y = _boundaryDistance.y - Mathf.Abs(position.y);
            distances.z = _boundaryDistance.z - Mathf.Abs(position.z);
            // Getting the gravity for the nearest face.
            if (distances.x < distances.y)
            {
                if (distances.x < distances.z)
                {
                    vector.x = GetGravityComponent(position.x, distances.x);
                }
                else
                {
                    vector.z = GetGravityComponent(position.z, distances.z);
                }
            }
            else if (distances.y < distances.z)
            {
                vector.y = GetGravityComponent(position.y, distances.y);
            }
            else
            {
                vector.z = GetGravityComponent(position.z, distances.z);
            }
            // Rotate back the gravity.
            return transform.TransformDirection(vector);
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Vector3 size;
            if (_innerFalloffDistance > _innerDistance) 
            {
                Gizmos.color = Color.cyan;
                size.x = 2f * (_boundaryDistance.x - _innerFalloffDistance);
                size.y = 2f * (_boundaryDistance.y - _innerFalloffDistance);
                size.z = 2f * (_boundaryDistance.z - _innerFalloffDistance);
                Gizmos.DrawWireCube(Vector3.zero, size);
            }
            if (_innerDistance > 0f) 
            {
                Gizmos.color = Color.yellow;
                size.x = 2f * (_boundaryDistance.x - _innerDistance);
                size.y = 2f * (_boundaryDistance.y - _innerDistance);
                size.z = 2f * (_boundaryDistance.z - _innerDistance);
                Gizmos.DrawWireCube(Vector3.zero, size);
            }
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Vector3.zero, 2f * _boundaryDistance);
            if (_outerDistance > 0f) {
                Gizmos.color = Color.yellow;
                DrawGizmosOuterCube(_outerDistance);
            }
            if (_outerFalloffDistance > _outerDistance) {
                Gizmos.color = Color.cyan;
                DrawGizmosOuterCube(_outerFalloffDistance);
            }
        }
        
        private static void DrawGizmosRect(Vector3 a, Vector3 b, Vector3 c, Vector3 d) 
        {
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, d);
            Gizmos.DrawLine(d, a);
        }
        
        private void DrawGizmosOuterCube(float distance) 
        {
            Vector3 a, b, c, d;
            a.y = b.y = _boundaryDistance.y;
            d.y = c.y = -_boundaryDistance.y;
            b.z = c.z = _boundaryDistance.z;
            d.z = a.z = -_boundaryDistance.z;
            // Draw right face
            a.x = b.x = c.x = d.x = _boundaryDistance.x + distance;
            DrawGizmosRect(a, b, c, d);
            // Draw left face
            a.x = b.x = c.x = d.x = -a.x;
            DrawGizmosRect(a, b, c, d);
            
            a.x = d.x = _boundaryDistance.x;
            b.x = c.x = -_boundaryDistance.x;
            a.z = b.z = _boundaryDistance.z;
            c.z = d.z = -_boundaryDistance.z;
            a.y = b.y = c.y = d.y = _boundaryDistance.y + distance;
            // Draw upper face
            DrawGizmosRect(a, b, c, d);
            // Draw lower face
            a.y = b.y = c.y = d.y = -a.y;
            DrawGizmosRect(a, b, c, d);

            a.x = d.x = _boundaryDistance.x;
            b.x = c.x = -_boundaryDistance.x;
            a.y = b.y = _boundaryDistance.y;
            c.y = d.y = -_boundaryDistance.y;
            a.z = b.z = c.z = d.z = _boundaryDistance.z + distance;
            // Draw front face
            DrawGizmosRect(a, b, c, d);
            // Draw back face
            a.z = b.z = c.z = d.z = -a.z;
            DrawGizmosRect(a, b, c, d);
            
            // √(1/3).
            distance *= 0.5773502692f;
            var size = _boundaryDistance;
            size.x = 2f * (size.x + distance);
            size.y = 2f * (size.y + distance);
            size.z = 2f * (size.z + distance); 
            Gizmos.DrawWireCube(Vector3.zero, size);
        }
    }
}