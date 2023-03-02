using System;
using UnityEngine;

namespace Catlike
{
    [RequireComponent(typeof(Rigidbody))]
    public class MovingSphereRigidbody : MonoBehaviour
    {
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");
        
        [SerializeField, Range(0f, 100f)] private float _maxSpeed = 10f;
        [SerializeField, Range(0f, 100f)] private float _maxAcceleration = 10f;
        [SerializeField, Range(0f, 100f)] float _maxAirAcceleration = 1f;
        [SerializeField, Range(0f, 10f)] private float _jumpHeight = 2f;
        [SerializeField, Range(0, 5)] private int _maxAirJumpCount;
        [SerializeField, Range(0f, 90f)] private float _maxGroundAngle = 25f;
        [SerializeField, Range(0f, 90f)] private float _maxStairsAngle = 50f;
        [SerializeField, Range(0f, 100f)] private float _maxSnapSpeed = 100f;
        [SerializeField, Min(0f)] private float _probeDistance = 1f;
        [SerializeField] private LayerMask _probeMask = -1;
        [SerializeField] private LayerMask _stairsMask = -1;
        [SerializeField] private Transform _playerInputSpace = default;
        [SerializeField] private bool _drawVelocityGizmo = true;
        
        private Vector3 _upAxis;
        private Vector3 _rightAxis;
        private Vector3 _forwardAxis;
        
        private Vector3 _velocity;
        private Vector3 _desiredVelocity;
        private Rigidbody _rigidbody;
        private Vector3 _contactNormal;
        private Vector3 _steepNormal;
        private int _groundContactCount;
        private int _steepContactCount;
        private bool _desiredJump;
        private int _activeJumpCount;
        private float _minGroundDotProduct;
        private float _minStairsDotProduct;
        private int _stepsSinceLastGrounded;
        private int _stepsSinceLastJump;
        private Renderer _renderer;

        private bool OnGround => _groundContactCount > 0;
        private bool OnSteep => _steepContactCount > 0;

        private void Awake()
        {
            OnValidate();
        }

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _renderer = GetComponent<Renderer>();
            _upAxis = -Physics.gravity.normalized;
        }

        private void OnValidate()
        {
            _minGroundDotProduct = Mathf.Cos(_maxGroundAngle * Mathf.Deg2Rad);
            _minStairsDotProduct = Mathf.Cos(_maxStairsAngle * Mathf.Deg2Rad);
        }
        
        private void Update()
        {
            var playerInput = ReadInput();
            if (_playerInputSpace)
            {
                // Determine direction of movement
                _rightAxis = ProjectDirectionOnPlane(_playerInputSpace.right, _upAxis);
                _forwardAxis = ProjectDirectionOnPlane(_playerInputSpace.forward, _upAxis);
                _desiredVelocity = (_forwardAxis * playerInput.y + _rightAxis * playerInput.x) * _maxSpeed;
            }
            else
            {
                _rightAxis = ProjectDirectionOnPlane(Vector3.right, _upAxis);
                _forwardAxis = ProjectDirectionOnPlane(Vector3.forward, _upAxis);
                _desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * _maxSpeed;
            }
            UpdateColor();
        }
        
        private Vector2 ReadInput()
        {
            var playerInput = Vector2.zero;
            playerInput.x = Input.GetAxis("Horizontal");
            playerInput.y = Input.GetAxis("Vertical");
            _desiredJump |= Input.GetButtonDown("Jump");
            return playerInput;
        }

        private void UpdateColor()
        {
            _renderer.material.SetColor(ColorProperty, OnGround ? Color.black : Color.white);
        }

        private void FixedUpdate()
        {
            _upAxis = -Physics.gravity.normalized;
            UpdateState();
            AdjustVelocity();
            if (_desiredJump)
            {
                _desiredJump = false;
                Jump();
            }
            _rigidbody.velocity = _velocity;
            ResetState();
        }

        private void UpdateState()
        {
            _stepsSinceLastGrounded++;
            _stepsSinceLastJump++;
            _velocity = _rigidbody.velocity;
            if (OnGround || SnapToGround() || CheckSteepContacts())
            {
                _stepsSinceLastGrounded = 0;
                // Checking false landing.
                if (_stepsSinceLastJump > 1)
                {
                    _activeJumpCount = 0;
                }
                if (_groundContactCount > 1)
                {
                    _contactNormal.Normalize();
                }
            }
            else
            {
                _contactNormal = _upAxis;
            }
        }

        private bool SnapToGround()
        {
            var speed = _velocity.magnitude;
            if (_stepsSinceLastGrounded > 1 || _stepsSinceLastJump <= 2)
            {
                return false;
            }
            if (speed > _maxSnapSpeed) 
            {
                return false;
            }
            if (!Physics.Raycast(_rigidbody.position, -_upAxis, out var hit, _probeDistance, _probeMask))
            {
                return false;
            }
            var upDot = Vector3.Dot(_upAxis, hit.normal);
            if (upDot < GetMinDot(hit.collider.gameObject.layer))
            {
                return false;
            }
            _groundContactCount = 1;
            _contactNormal = hit.normal;
            var dot = Vector3.Dot(_velocity, hit.normal);
            var pointingUpward = dot > 0;
            if (pointingUpward)
            {
                // adjust velocity to align with the ground
                _velocity = (_velocity - hit.normal * dot).normalized * speed;
            }
            return true;
        }

        private void ResetState()
        {
            _groundContactCount = _steepContactCount = 0;
            _contactNormal = _steepNormal = Vector3.zero;
        }

        private bool TryGetJumpDirection(out Vector3 jumpDirection)
        {
            if (OnGround)
            {
                jumpDirection = _contactNormal;
                return true;
            }
            if (OnSteep)
            {
                _activeJumpCount = 0;
                jumpDirection = _steepNormal;
                return true;
            }
            var airJumpIsPossible = _maxAirJumpCount > 0 && _activeJumpCount <= _maxAirJumpCount;
            if (airJumpIsPossible)
            {
                var falling = _activeJumpCount == 0;
                if (falling)
                {
                    _activeJumpCount = 1;
                }
                // This should be set to Vector3.up in this case.
                jumpDirection = _contactNormal;
                return true;
            }
            jumpDirection = Vector3.zero;
            return false;
        }

        private void Jump()
        {
            if (!TryGetJumpDirection(out var jumpDirection))
            {
                return;
            }
            jumpDirection = (jumpDirection + _upAxis).normalized;
            _stepsSinceLastJump = 0;
            _activeJumpCount++;
            var jumpSpeed = CalculateGravitationalEscapeSpeed(_jumpHeight);
            var alignedSpeed = Vector3.Dot(_velocity, jumpDirection);
            if (alignedSpeed > 0.0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0.0f);
            }
            _velocity += jumpDirection * jumpSpeed;
        }

        private static float CalculateGravitationalEscapeSpeed(float height)
        {
            return Mathf.Sqrt(2f * Physics.gravity.magnitude * height);
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
            var minDot = GetMinDot(collision.gameObject.layer);
            for (var i = 0; i < collision.contactCount; i++) 
            {
                var normal = collision.GetContact(i).normal;
                var upDot = Vector3.Dot(_upAxis, normal);
                var isGround = upDot > minDot;
                if (isGround)
                {
                    _groundContactCount++;
                    _contactNormal += normal;
                    continue;
                }
                var isSteep = normal.y > -0.01f;
                if (isSteep)
                {
                    _steepContactCount++;
                    _steepNormal += normal;
                }
            }
        }

        private bool CheckSteepContacts()
        {
            if (_steepContactCount <= 1)
            {
                return false;
            }
            _steepNormal.Normalize();
            var upDot = Vector3.Dot(_upAxis, _steepNormal);
            if (upDot >= _minGroundDotProduct)
            {
                _groundContactCount++;
                _contactNormal = _steepNormal;
                return true;
            }
            return false;
        }

        private Vector3 _offset;
        
        private void AdjustVelocity()
        {
            // Get z and x axis in terms of the contact plane coordinates.
            var xAxis = ProjectDirectionOnPlane(_rightAxis, _contactNormal);
            var zAxis = ProjectDirectionOnPlane(_forwardAxis, _contactNormal);
            // Determine the current velocity in these dimensions.
            var currentX = Vector3.Dot(_velocity, xAxis);
            var currentZ = Vector3.Dot(_velocity, zAxis);
            var acceleration = OnGround ? _maxAcceleration : _maxAirAcceleration;
            var maxSpeedChange = acceleration * Time.deltaTime;
            // Determine new velocity.
            var newX = Mathf.MoveTowards(currentX, _desiredVelocity.x, maxSpeedChange);
            var newZ = Mathf.MoveTowards(currentZ, _desiredVelocity.z, maxSpeedChange);
            // Calculate the difference.
            var deltaX = newX - currentX;
            var deltaZ = newZ - currentZ;
            // TODO: remove
            _offset = xAxis * deltaX + zAxis * deltaZ;
            // Add to current velocity in the appropriate dimensions.
            _velocity += xAxis * deltaX + zAxis * deltaZ;
        }
        
        
        private void OnDrawGizmos()
        {
            if (_drawVelocityGizmo)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transform.position + _rightAxis * 2.0f);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.position + _forwardAxis * 2.0f);
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, transform.position + _desiredVelocity.normalized * 2.0f);
                Gizmos.color = Color.white;
                Gizmos.DrawLine(transform.position, transform.position + _offset.normalized * 2.0f);
            }
        }

        private static Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
        {
            var result = direction - (normal * Vector3.Dot(direction, normal));
            return result.normalized;
        }

        private float GetMinDot(int layerIndex)
        {
            return (_stairsMask & (1 << layerIndex)) == 0 ? _minGroundDotProduct : _minStairsDotProduct;
        }
    }
}
