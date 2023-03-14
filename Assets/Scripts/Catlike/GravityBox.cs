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

        private void Awake()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            _boundaryDistance = Vector3.Max(_boundaryDistance, Vector3.zero);
            var minDimension = Mathf.Min(_boundaryDistance.x, _boundaryDistance.y, _boundaryDistance.z);
            _innerDistance = Mathf.Min(_innerDistance, minDimension);
            _innerFalloffDistance = Mathf.Max(Mathf.Min(_innerFalloffDistance, minDimension), _innerDistance);
        }

        public float GetGravityComponent(float coordinateRelativeToBoxCenter, float distanceToNearestFace)
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
            // Get box relative position. InverseTransformDirection is used to support arbitrary rotation.
            position = transform.InverseTransformDirection(position - transform.position);
            var vector = Vector3.zero;
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
        }
    }
}