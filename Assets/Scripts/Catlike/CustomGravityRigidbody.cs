using System;
using UnityEngine;

namespace Catlike
{
    [RequireComponent(typeof(Rigidbody))]
    public class CustomGravityRigidbody : MonoBehaviour
    {
        [SerializeField] private bool _floatToSleep;
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