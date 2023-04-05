using UnityEngine;
using static Catlike.Utility;

namespace Catlike
{
    [RequireComponent(typeof(Rigidbody))]
    public class CustomGravityRigidbody : MonoBehaviour
    {
        [SerializeField] private bool _floatToSleep;
        [SerializeField] private float _submergenceOffset = 0.5f;
        [SerializeField, Min(0.1f)] private float _submergenceRange = 1f;
        [Tooltip("With zero buoyancy sinks like a rock, an object with a buoyancy of 1 is in equilibrium, buoyancy greater than 1 floats to the surface.")]
        [SerializeField, Min(0)] private float _buoyancy = 1f;
        [SerializeField, Range(0f, 10f)] private float _waterDrag = 1f;
        [SerializeField] private LayerMask _waterMask = 0;

        private Rigidbody _rigidbody;
        private float _floatDelay;
        private float _submergence;
        private Vector3 _gravity;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;
        }

        private void FixedUpdate()
        {
            if (_floatToSleep)
            {
                if (_rigidbody.IsSleeping())
                {
                    _floatDelay = 0f;
                    return;
                }
                if (_rigidbody.velocity.sqrMagnitude < 0.0001f)
                {
                    _floatDelay += Time.deltaTime;
                    if (_floatDelay >= 1.0f)
                    {
                        return;
                    }
                }
                else
                {
                    _floatDelay = 0.0f;
                }
            }
            _gravity = CustomGravity.GetGravity(_rigidbody.position);
            if (_submergence > 0)
            {
                var drag = Mathf.Max(0f, 1f - _waterDrag * _submergence * Time.deltaTime);
                _rigidbody.velocity *= drag;
                _rigidbody.angularVelocity *= drag;
                _rigidbody.AddForce(_gravity * - (_buoyancy * _submergence), ForceMode.Acceleration);
                _submergence = 0;
            }
            _rigidbody.AddForce(_gravity, ForceMode.Acceleration);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (MaskIsSet(_waterMask, other.gameObject.layer))
            {
                EvaluateSubmergence();
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (_rigidbody.IsSleeping())
            {
                // Skip evaluating submergence if body is sleeping.
                return;
            }
            if (MaskIsSet(_waterMask, other.gameObject.layer))
            {
                EvaluateSubmergence();
            }
        }

        private void EvaluateSubmergence()
        {
            var upAxis = -_gravity.normalized;
            var origin = _rigidbody.position + upAxis * _submergenceOffset;
            // This is needed to counter invalid value when moving out of the water.
            var additionalRangeOffset = 1.0f;
            if (Physics.Raycast(origin, upAxis, out var hit, _submergenceRange + additionalRangeOffset, _waterMask, QueryTriggerInteraction.Collide))
            {
                _submergence = 1 - hit.distance / _submergenceRange;
            }
            else
            {
                _submergence = 1f;
            }
        }
    }
}