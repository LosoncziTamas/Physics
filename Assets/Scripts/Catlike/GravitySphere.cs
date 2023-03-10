using UnityEngine;

namespace Catlike
{
    public class GravitySphere : GravitySource
    {
        [SerializeField] private float _gravity = 9.81f;
        [SerializeField, Min(0f)] private float _outerRadius = 10.0f;
        [SerializeField, Min(0f)] private float _outerFallOffRadius = 15.0f;
        [SerializeField, Min(0f)] private float _innerFalloffRadius = 1.0f;
        [SerializeField, Min(0f)] private float _innerRadius = 5.0f;

        private void Awake()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            _innerFalloffRadius = Mathf.Max(_innerFalloffRadius, 0f);
            _innerRadius = Mathf.Max(_innerRadius, _innerFalloffRadius);
            _outerRadius = Mathf.Max(_outerRadius, _innerRadius);
            _outerFallOffRadius = Mathf.Max(_outerRadius, _outerFallOffRadius);
        }

        public override Vector3 GetGravity(Vector3 position)
        {
            var offset = transform.position - position;
            var distance = offset.magnitude;
            if (distance > _outerFallOffRadius || distance < _innerFalloffRadius)
            {
                return Vector3.zero;
            }
            var g = _gravity / distance;
            // Lerp the gravity value
            if (distance > _outerRadius)
            {
                var outerFalloffFactor = 1f / (_outerFallOffRadius - _outerRadius);
                g *= 1f - (distance - _outerRadius) * outerFalloffFactor;
            } else if (distance < _innerRadius)
            {
                var innerFalloffFactor = 1f / (_innerRadius - _innerFalloffRadius);
                g *= 1f - (_innerRadius - distance) * innerFalloffFactor;
            }
            return g * offset;
        }

        private void OnDrawGizmos()
        {
            var center = transform.position;
            if (_innerFalloffRadius > 0 && _innerFalloffRadius < _innerRadius)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(center, _innerFalloffRadius);
            }
            Gizmos.color = Color.yellow;
            if (_innerRadius > 0f && _innerRadius < _outerRadius)
            {
                Gizmos.DrawWireSphere(center, _innerRadius);
            }
            Gizmos.DrawWireSphere(center, _outerRadius);
            if (_outerRadius < _outerFallOffRadius)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(center, _outerFallOffRadius);
            }
        }
    }
}