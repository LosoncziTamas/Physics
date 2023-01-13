using System;
using UnityEngine;

namespace Catlike
{
    [RequireComponent(typeof(Rigidbody))]
    public class MovingSphereRigidbody : MonoBehaviour
    {
        [SerializeField, Range(0f, 100f)] private float _maxSpeed = 10f;
        [SerializeField, Range(0f, 100f)] private float _maxAcceleration = 10f;
        
        private Vector3 _velocity;
        private Vector3 _startPosition;
        private Rigidbody _rigidbody;

        private void Start()
        {
            _startPosition = transform.position;
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void ResetToDefault()
        {
            _velocity = Vector3.zero;
            transform.position = _startPosition;
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Reset"))
            {
                ResetToDefault();
            }
        }

        private void Update()
        {
            var playerInput = Vector2.zero;
            playerInput.x = Input.GetAxis("Horizontal");
            playerInput.y = Input.GetAxis("Vertical");
            var desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * _maxSpeed;
            var maxSpeedChange = _maxAcceleration * Time.deltaTime;
            _velocity = _rigidbody.velocity;
            _velocity.x = Mathf.MoveTowards(_velocity.x, desiredVelocity.x, maxSpeedChange);
            _velocity.z = Mathf.MoveTowards(_velocity.z, desiredVelocity.z, maxSpeedChange);
            _rigidbody.velocity = _velocity;
        }
    }
}
