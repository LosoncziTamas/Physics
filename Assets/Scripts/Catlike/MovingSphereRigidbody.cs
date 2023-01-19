using System;
using UnityEngine;

namespace Catlike
{
    [RequireComponent(typeof(Rigidbody))]
    public class MovingSphereRigidbody : MonoBehaviour
    {
        [SerializeField, Range(0f, 100f)] private float _maxSpeed = 10f;
        [SerializeField, Range(0f, 100f)] private float _maxAcceleration = 10f;
        [SerializeField, Range(0f, 100f)] float _maxAirAcceleration = 1f;
        [SerializeField, Range(0f, 10f)] private float _jumpHeight = 2f;
        [SerializeField, Range(0, 5)] private int _maxAirJumpCount = 0;
        [SerializeField, Range(0f, 90f)] float _maxGroundAngle = 25f;
        
        private Vector3 _velocity;
        private Vector3 _startPosition;
        private Vector3 _desiredVelocity;
        private Rigidbody _rigidbody;
        private Vector3 _contactNormal;
        private bool _desiredJump;
        private bool _onGround;
        private int _activeJumpCount;
        private float _minGroundDotProduct;

        private void Awake()
        {
            OnValidate();
        }

        private void Start()
        {
            _startPosition = transform.position;
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void OnValidate()
        {
            _minGroundDotProduct = Mathf.Cos(_maxGroundAngle * Mathf.Deg2Rad);
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
            _desiredJump |= Input.GetButtonDown("Jump");
            _desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * _maxSpeed;
        }

        private void FixedUpdate()
        {
            var acceleration = _onGround ? _maxAcceleration : _maxAirAcceleration;
            var maxSpeedChange = acceleration * Time.deltaTime;
            _velocity = _rigidbody.velocity;
            if (_onGround)
            {
                _activeJumpCount = 0;
            }
            else
            {
                _contactNormal = Vector3.up;
            }
            _velocity.x = Mathf.MoveTowards(_velocity.x, _desiredVelocity.x, maxSpeedChange);
            _velocity.z = Mathf.MoveTowards(_velocity.z, _desiredVelocity.z, maxSpeedChange);
            if (_desiredJump)
            {
                _desiredJump = false;
                Jump();
            }
            _rigidbody.velocity = _velocity;
            _onGround = false;
        }

        private void Jump()
        {
            var canJump = _onGround || _activeJumpCount < _maxAirJumpCount;
            if (canJump)
            {
                _activeJumpCount++;
                var jumpVelocity = CalculateGravitationalEscapeVelocity(_jumpHeight);
                var alignedSpeed = Vector3.Dot(_velocity, _contactNormal);
                if (alignedSpeed > 0.0f)
                {
                    jumpVelocity = Mathf.Max(jumpVelocity - alignedSpeed, 0.0f);
                }
                _velocity += _contactNormal * jumpVelocity;
            }
        }

        private static float CalculateGravitationalEscapeVelocity(float height)
        {
            return Mathf.Sqrt(-2f * Physics.gravity.y * height);
        }

        private void OnCollisionStay(Collision collision)
        {
            EvaluateCollision(collision);
        }

        private void OnCollisionEnter(Collision collision)
        {
            EvaluateCollision(collision);
        }

        private void EvaluateCollision(Collision collision)
        {
            for (var i = 0; i < collision.contactCount; i++) 
            {
                var normal = collision.GetContact(i).normal;
                if (normal.y > _minGroundDotProduct)
                {
                    _onGround = true;
                    _contactNormal = normal;
                }
            }
        }
    }
}
