using System;
using UnityEngine;

namespace Catlike
{
    public class MovingSphere : MonoBehaviour
    {
        public enum MovementType
        {
            VelocityBased,
            Accelerated,
            Combined
        }
        
        [SerializeField, Range(0f, 100f)] private float _maxSpeed = 10f;
        [SerializeField, Range(0f, 100f)] private float _maxAcceleration = 10f;
        [SerializeField] private MovementType _movementType = MovementType.Combined;
        [SerializeField] private Rect _allowedArea = new(-4.5f, -4.5f, 10f, 10f);
        [SerializeField, Range(0f, 1f)] private float _bounciness = 0.5f;
        
        private Vector3 _velocity;
        private Vector3 _startPosition;

        private void Start()
        {
            _startPosition = transform.position;
        }

        private void ResetToDefault()
        {
            _velocity = Vector3.zero;
            transform.position = _startPosition;
        }

        private void OnGUI()
        {
            GUILayout.Label($"Current movement type: {_movementType}");
            foreach (MovementType movementType in Enum.GetValues(typeof(MovementType)))
            {
                if (movementType == _movementType)
                {
                    continue;
                }
                if (GUILayout.Button(movementType.ToString()))
                {
                    _movementType = movementType;
                }
            }
            if (GUILayout.Button("Reset"))
            {
                ResetToDefault();
            }
        }

        private void Update()
        {
            var newPosition = _movementType switch
            {
                MovementType.VelocityBased => VelocityBasedMovement(),
                MovementType.Accelerated => AcceleratedMovement(),
                MovementType.Combined => CombinedMovement(),
                _ => throw new ArgumentOutOfRangeException()
            };
            if (!_allowedArea.Contains(new Vector2(newPosition.x, newPosition.z)))
            {
                if (newPosition.x < _allowedArea.xMin) 
                {
                    newPosition.x = _allowedArea.xMin;
                    _velocity.x = -_velocity.x * _bounciness;
                }
                else if (newPosition.x > _allowedArea.xMax) 
                {
                    newPosition.x = _allowedArea.xMax;
                    _velocity.x = -_velocity.x * _bounciness;
                }
                if (newPosition.z < _allowedArea.yMin) 
                {
                    newPosition.z = _allowedArea.yMin;
                    _velocity.z = -_velocity.z * _bounciness;
                }
                else if (newPosition.z > _allowedArea.yMax) 
                {
                    newPosition.z = _allowedArea.yMax;
                    _velocity.z = -_velocity.z * _bounciness;
                }
            }
            transform.localPosition = newPosition;
        }

        private Vector3 VelocityBasedMovement()
        {
            var playerInput = Vector2.zero;
            playerInput.x = Input.GetAxis("Horizontal");
            playerInput.y = Input.GetAxis("Vertical");
            playerInput = Vector2.ClampMagnitude(playerInput, 1f);
            var velocity = new Vector3(playerInput.x, 0f, playerInput.y) * _maxSpeed;
            var displacement = velocity * Time.deltaTime;
            return transform.localPosition + displacement;
        }

        private Vector3 AcceleratedMovement()
        {
            var playerInput = Vector2.zero;
            playerInput.x = Input.GetAxis("Horizontal");
            playerInput.y = Input.GetAxis("Vertical");
            playerInput = Vector2.ClampMagnitude(playerInput, 1f);
            var acceleration = new Vector3(playerInput.x, 0f, playerInput.y) * _maxSpeed;
            _velocity += acceleration * Time.deltaTime;
            var displacement = _velocity * Time.deltaTime;
            return transform.localPosition + displacement;
        }

        private Vector3 CombinedMovement()
        {
            var playerInput = Vector2.zero;
            playerInput.x = Input.GetAxis("Horizontal");
            playerInput.y = Input.GetAxis("Vertical");
            var desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * _maxSpeed;
            var maxSpeedChange = _maxAcceleration * Time.deltaTime;
            _velocity.x = Mathf.MoveTowards(_velocity.x, desiredVelocity.x, maxSpeedChange);
            _velocity.z = Mathf.MoveTowards(_velocity.z, desiredVelocity.z, maxSpeedChange);
            var displacement = _velocity * Time.deltaTime;
            return transform.localPosition + displacement;
        }
    }
}
