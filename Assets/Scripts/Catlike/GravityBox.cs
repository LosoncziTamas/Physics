using System;
using UnityEngine;

namespace Catlike
{
    public class GravityBox : GravitySource
    {
        [SerializeField] private float _gravity = 9.81f;
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
                result *= 1f - (distanceToNearestFace - _innerDistance) * _innerFalloffDistance;
            }
            return coordinateRelativeToBoxCenter > 0 ? -result : result;
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