using UnityEngine;

namespace Catlike
{
    [RequireComponent(typeof(Camera))]
    public class OrbitCamera : MonoBehaviour
    {
        [SerializeField] private Transform _focus;
        [SerializeField, Range(1f, 20f)] private float _distance = 5f;
        [SerializeField, Range(0f, 1f)] private float _focusCentering = 0.5f;
        [SerializeField, Min(0f)] private float _focusRadius = 1f;
        [SerializeField, Range(1f, 360f)] private float _rotationSpeedInDegreesPerSecond = 90f;
        [SerializeField, Range(-89f, 89f)] private float _minVerticalAngle = -30f;
        [SerializeField, Range(-89f, 89f)] private float _maxVerticalAngle = 60f;
        [SerializeField, Min(0f)] private float _alignDelay = 5f;
        [SerializeField, Range(0f, 90f)] private float _alignSmoothRange = 45f;
        [SerializeField] private Camera _regularCamera;
        [SerializeField] private LayerMask _obstructionMask = -1;
        [SerializeField, Min(0f)] private float _upAlignmentSpeedInDegreesPerSecond = 360.0f;
        
        private Quaternion _gravityAlignment = Quaternion.identity;
        // Orbit rotation logic should be independent from gravity alignment.
        private Quaternion _orbitRotation = Quaternion.identity;
        private Vector3 _focusPoint;
        private Vector3 _previousFocusPoint;
        private Transform _cachedTransform;
        private float _lastManualRotationTime;
        private Vector2 _orbitAngles = new(45f, 0f);

        private Vector3 CameraHalfExtends
        {
            get
            {
                var y = _regularCamera.nearClipPlane * Mathf.Tan(0.5f * Mathf.Deg2Rad * _regularCamera.fieldOfView);
                var x = y * _regularCamera.aspect;
                var z = 0f;
                var halfExtends = new Vector3(x, y, z);
                return halfExtends;
            }
        }

        private void OnValidate()
        {
            if (_maxVerticalAngle < _minVerticalAngle) 
            {
                _maxVerticalAngle = _minVerticalAngle;
            }
        }
        
        private void ConstrainAngles() 
        {
            _orbitAngles.x = Mathf.Clamp(_orbitAngles.x, _minVerticalAngle, _maxVerticalAngle);
            if (_orbitAngles.y < 0f) 
            {
                _orbitAngles.y += 360f;
            }
            else if (_orbitAngles.y >= 360f) 
            {
                _orbitAngles.y -= 360f;
            }
        }

        private void Awake()
        {
            _focusPoint = _focus.position;
            _cachedTransform = transform;
            _cachedTransform.localRotation = _orbitRotation = Quaternion.Euler(_orbitAngles);
        }

        private void LateUpdate()
        {
            UpdateGravityAlignment();
            UpdateFocusPoint();
            if (ManualRotation() || AutomaticRotation())
            {
                ConstrainAngles();
                _orbitRotation =  Quaternion.Euler(_orbitAngles);
            }
            var lookRotation = _gravityAlignment * _orbitRotation;
            var lookDirection = lookRotation * Vector3.forward;
            var lookPosition = _focusPoint - lookDirection * _distance;

            // Determine actual focus position to cast from 
            var rectOffset = lookDirection * _regularCamera.nearClipPlane;
            var rectPosition = lookPosition + rectOffset;
            var castFrom = _focus.position;
            var castLine = rectPosition - castFrom;
            var castDistance = castLine.magnitude;
            var castDirection = castLine / castDistance;
            
            // Pulling camera closer if something blocks the view
            if (Physics.BoxCast(castFrom, CameraHalfExtends, castDirection, out var hit, lookRotation, castDistance, _obstructionMask, QueryTriggerInteraction.Ignore))
            {
                rectPosition = castFrom + castDirection * hit.distance;
                lookPosition = rectPosition - rectOffset;
            }
            
            // Align to look at focus point from given distance
            _cachedTransform.SetPositionAndRotation(lookPosition, lookRotation);
        }

        private void UpdateGravityAlignment()
        {
            // Create alignment from the last aligned up direction to the current up direction (minimal rotation).
            var fromUp = _gravityAlignment * Vector3.up;
            var toUp = CustomGravity.GetUpAxis(_focusPoint);
            var dot = Vector3.Dot(fromUp, toUp);
            // Sanitize precision errors.
            var dotClamped = Mathf.Clamp(dot, -1, 1);
            var angleBetweenUpVectors = Mathf.Acos(dotClamped) * Mathf.Rad2Deg;
            var maxAngleForFrame = _upAlignmentSpeedInDegreesPerSecond * Time.deltaTime;
            var newAlignment = Quaternion.FromToRotation(fromUp, toUp) * _gravityAlignment;
            if (angleBetweenUpVectors <= maxAngleForFrame)
            {
                _gravityAlignment = newAlignment;
            }
            else
            {
                // Interpolating between rotations.
                var t = maxAngleForFrame / angleBetweenUpVectors;
                _gravityAlignment = Quaternion.SlerpUnclamped(_gravityAlignment, newAlignment, t);
            }
        }
        
        private bool ManualRotation () {
            var input = new Vector2(
                Input.GetAxis("Vertical Camera"),
                Input.GetAxis("Horizontal Camera")
            );
            const float e = 0.001f;
            var inputExceedsSensitivity = input.x is < -e or > e || input.y is < -e or > e;
            if (inputExceedsSensitivity) {
                _orbitAngles += _rotationSpeedInDegreesPerSecond * Time.unscaledDeltaTime * input;
                _lastManualRotationTime = Time.unscaledTime;
                return true;
            }
            return false;
        }

        private bool AutomaticRotation() 
        {
            if (Time.unscaledTime - _lastManualRotationTime < _alignDelay) 
            {
                return false;
            }
            // Undo the gravity alignment
            var alignedDelta = Quaternion.Inverse(_gravityAlignment) * (_focusPoint - _previousFocusPoint);
            var movement = new Vector2(alignedDelta.x, alignedDelta.z);
            var movementDeltaSqr = movement.sqrMagnitude;
            if (movementDeltaSqr < 0.0001f)
            {
                return false;
            }
            var direction = movement / Mathf.Sqrt(movementDeltaSqr);
            var headingAngle = GetAngle(direction);
            var deltaAbs = Mathf.Abs(Mathf.DeltaAngle(_orbitAngles.y, headingAngle));
            var rotationChange = _rotationSpeedInDegreesPerSecond * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
            if (deltaAbs < _alignSmoothRange)
            {
                rotationChange *= deltaAbs / _alignSmoothRange;
            } 
            // preventing the camera from rotating away at full speed.
            else if (180.0f - deltaAbs < _alignSmoothRange)
            {
                rotationChange *= (180.0f - deltaAbs) / _alignSmoothRange;
            }
            _orbitAngles.y = Mathf.MoveTowardsAngle(_orbitAngles.y, headingAngle, rotationChange);
            return true;
        }

        private static float GetAngle(Vector2 direction)
        {
            // y means here movement toward z
            var angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
            return direction.x < 0f ? 360f - angle : angle;
        }

        private void UpdateFocusPoint()
        {
            _previousFocusPoint = _focusPoint;
            var targetPoint = _focus.position;
            // Relaxed focus enabled
            if (_focusRadius > 0)
            {
                var distance = Vector3.Distance(targetPoint, _focusPoint);
                var t = 1.0f;
                // Focus centering enabled
                if (distance > 0.01f && _focusCentering > 0)
                {
                    // Smooth centering
                    // ex.: 0.5 ^ 0.016 = 0.988
                    t = Mathf.Pow(1f - _focusCentering, Time.unscaledDeltaTime);
                }
                // We are above focus range
                if (distance > _focusRadius)
                {
                    // ex.: Min(0.988, 0.5)
                    t = Mathf.Min(t, _focusRadius / distance);
                }
                // Lerp focus point from target to focus.
                _focusPoint = Vector3.Lerp(targetPoint, _focusPoint, t);
            }
            else
            {
                _focusPoint = targetPoint;
            }
        }
    }
}