using UnityEngine;

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
            _rigidbody.AddForce(CustomGravity.GetGravity(_rigidbody.position), ForceMode.Acceleration);
        }
    }
}