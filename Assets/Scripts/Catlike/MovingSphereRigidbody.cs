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
        }

        private void OnValidate()
        {
            _minGroundDotProduct = Mathf.Cos(_maxGroundAngle * Mathf.Deg2Rad);
            _minStairsDotProduct = Mathf.Cos(_maxStairsAngle * Mathf.Deg2Rad);
        }

        private void Update()
        {
            var playerInput = Vector2.zero;
            playerInput.x = Input.GetAxis("Horizontal");
            playerInput.y = Input.GetAxis("Vertical");
            _desiredJump |= Input.GetButtonDown("Jump");
            _desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * _maxSpeed;
            UpdateColor();
        }

        private void UpdateColor()
        {
            _renderer.material.SetColor(ColorProperty, OnGround ? Color.black : Color.white);
        }

        private void FixedUpdate()
        {
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
                _contactNormal = Vector3.up;
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
            if (!Physics.Raycast(_rigidbody.position, Vector3.down, out var hit, _probeDistance, _probeMask))
            {
                return false;
            }
            if (hit.normal.y < GetMinDot(hit.collider.gameObject.layer))
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

        private Vector3 GetJumpDirection()
        {
            if (OnGround)
            {
                return _contactNormal;
            }
            if (OnSteep)
            {
                _activeJumpCount = 0;
                return _steepNormal;
            }
            var airJumpIsPossible = _maxAirJumpCount > 0 && _activeJumpCount <= _maxAirJumpCount;
            if (airJumpIsPossible)
            {
                _activeJumpCount = _activeJumpCount == 0 ? 1 : _activeJumpCount;
                // This should be set to Vector3.up in this case.
                return _contactNormal;
            }
            return Vector3.zero;
        }

        private void Jump()
        {
            var jumpDirection = (GetJumpDirection() + Vector3.up).normalized;
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
            var minDot = GetMinDot(collision.gameObject.layer);
            for (var i = 0; i < collision.contactCount; i++) 
            {
                var normal = collision.GetContact(i).normal;
                var isGround = normal.y > minDot;
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
            if (_steepNormal.y >= _minGroundDotProduct)
            {
                _groundContactCount++;
                _contactNormal = _steepNormal;
                return true;
            }
            return false;
        }

        private void AdjustVelocity()
        {
            // Get z and x axis in terms of the plane coordinates.
            var xAxis = ProjectOnContactPlane(Vector3.right).normalized;
            var zAxis = ProjectOnContactPlane(Vector3.forward).normalized;
            // Determine the current velocity in these dimensions.
            var currentX = Vector3.Dot(_velocity, xAxis);
            var currentZ = Vector3.Dot(_velocity, zAxis);
            var acceleration = OnGround ? _maxAcceleration : _maxAirAcceleration;
            var maxSpeedChange = acceleration * Time.deltaTime;
            // Determine new velocity.
            // TODO: check what if _desiredVelocity is also projected.
            var newX = Mathf.MoveTowards(currentX, _desiredVelocity.x, maxSpeedChange);
            var newZ = Mathf.MoveTowards(currentZ, _desiredVelocity.z, maxSpeedChange);
            // Calculate the difference.
            var deltaX = newX - currentX;
            var deltaZ = newZ - currentZ;
            // Add to current velocity in the appropriate dimensions.
            _velocity += xAxis * deltaX + zAxis * deltaZ;
        }

        
        private Vector3 ProjectOnContactPlane(Vector3 vector)
        {
            return vector - _contactNormal * Vector3.Dot(vector, _contactNormal);
        }

        private float GetMinDot(int layerIndex)
        {
            return (_stairsMask & (1 << layerIndex)) == 0 ? _minGroundDotProduct : _minStairsDotProduct;
        }
    }
}
