using UnityEngine;

namespace Catlike
{
    public class GravitySphere : GravitySource
    {
        [SerializeField] private float _gravity = 9.81f;
        [SerializeField, Min(0f)] private float _outerRadius = 10.0f;
        [SerializeField, Min(0f)] private float _outerFallOffRadius = 15.0f;

        private void Awake()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            _outerFallOffRadius = Mathf.Max(_outerRadius, _outerFallOffRadius);
        }

        public override Vector3 GetGravity(Vector3 position)
        {
            var offset = transform.position - position;
            var distance = offset.magnitude;
            if (distance > _outerFallOffRadius)
            {
                return Vector3.zero;
            }
            var g = _gravity / distance;
            if (distance > _outerFallOffRadius)
            {
                var outerFalloffFactor = 1f / (_outerFallOffRadius - _outerRadius);
                g *= 1f - (distance - _outerRadius) * outerFalloffFactor;
            }
            return g * offset;
        }

        private void OnDrawGizmos()
        {
            var center = transform.position;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, _outerRadius);
            if (_outerRadius < _outerFallOffRadius)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(center, _outerFallOffRadius);
            }
        }
    }
}