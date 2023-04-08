using System;
using UnityEngine;
using static Catlike.Utility;

namespace Catlike
{
    [RequireComponent(typeof(Rigidbody))]
    public class StableFloatingRigidbody : MonoBehaviour
    {
        [SerializeField] private bool _floatToSleep;
        [SerializeField] private float _submergenceOffset = 0.5f;
        [SerializeField, Min(0.1f)] private float _submergenceRange = 1f;
        [Tooltip("With zero buoyancy sinks like a rock, an object with a buoyancy of 1 is in equilibrium, buoyancy greater than 1 floats to the surface.")]
        [SerializeField, Min(0)] private float _buoyancy = 1f;
        [SerializeField, Range(0f, 10f)] private float _waterDrag = 1f;
        [SerializeField] private LayerMask _waterMask = 0;
        [SerializeField] private Vector3[] _buoyancyOffsets = default;
        [SerializeField] private bool _safeFloating = false;

        private Rigidbody _rigidbody;
        private float _floatDelay;
        private float[] _submergence;
        private Vector3 _gravity;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;
            _submergence = new float[_buoyancyOffsets.Length];
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
            var dragFactor = _waterDrag * Time.deltaTime / _buoyancyOffsets.Length;
            var buoyancyFactor = -_buoyancy / _buoyancyOffsets.Length;
            for (var i = 0; i < _buoyancyOffsets.Length; i++)
            {
                if (_submergence[i] > 0)
                {
                    var drag = Mathf.Max(0f, 1f - dragFactor * _submergence[i]);
                    _rigidbody.velocity *= drag;
                    _rigidbody.angularVelocity *= drag;
                    var buoyancy = _gravity * (buoyancyFactor * _submergence[i]);
                    var buoyancyCenter = transform.TransformPoint(_buoyancyOffsets[i]);
                    // Push offset point to the top. 
                    _rigidbody.AddForceAtPosition(buoyancy, buoyancyCenter,  ForceMode.Acceleration);
                    _submergence[i] = 0;
                }
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
            var down = _gravity.normalized;
            var offset = down * -_submergenceOffset;
            // This is needed to counter invalid value when moving out of the water.
            var additionalRangeOffset = 1.0f;
            for (var index = 0; index < _buoyancyOffsets.Length; index++)
            {
                var buoyancyOffset = _buoyancyOffsets[index];
                var origin = offset + transform.TransformPoint(buoyancyOffset);
                if (Physics.Raycast(origin, down, out var hit, _submergenceRange + additionalRangeOffset, _waterMask, QueryTriggerInteraction.Collide))
                {
                    _submergence[index] = 1f - hit.distance / _submergenceRange;
                }
                else if (!_safeFloating || Physics.CheckSphere(origin, 0.01f, _waterMask, QueryTriggerInteraction.Collide))
                {
                    // fully submerged
                    _submergence[index] = 1f;
                }
            }
        }

        private void OnDrawGizmos()
        {
            for (var index = 0; index < _buoyancyOffsets.Length; index++)
            {
                var submergence = 0f;
                if (_submergence is { Length: > 0 })
                {
                    submergence = _submergence[index];
                }
                var buoyancyOffset = _buoyancyOffsets[index];
                var origin = buoyancyOffset + transform.TransformPoint(buoyancyOffset);
                Gizmos.color = Color.Lerp(Color.white, Color.blue, submergence);
                Gizmos.DrawSphere(origin, 0.15f * submergence);
            }
        }
    }
}