using System;
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

        private Vector3 _focusPoint;
        private Transform _cachedTransform;
        private float _lastManualRotationTime;
        private Vector2 _orbitAngles = new(45f, 0f);

        private void OnValidate()
        {
            if (_maxVerticalAngle < _minVerticalAngle) 
            {
                _maxVerticalAngle = _minVerticalAngle;
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("_orbitAngles " + _orbitAngles);
            GUILayout.Label("_maxVerticalAngle " + _maxVerticalAngle);
            GUILayout.Label("_minVerticalAngle " + _minVerticalAngle);
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

        private void Start()
        {
            _focusPoint = _focus.position;
            _cachedTransform = transform;
            _cachedTransform.localRotation = Quaternion.Euler(_orbitAngles);
        }

        private void LateUpdate()
        {
            UpdateFocusPoint();
            Quaternion lookRotation;
            if (ManualRotation() || AutomaticRotation())
            {
                ConstrainAngles();
                lookRotation = Quaternion.Euler(_orbitAngles);
            }
            else
            {
                lookRotation = _cachedTransform.localRotation;
            }
            var lookDirection = lookRotation * Vector3.forward;
            var lookPosition = _focusPoint - lookDirection * _distance;
            // Align to look at focus point from given distance
            _cachedTransform.SetPositionAndRotation(lookPosition, lookRotation);
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
        
        private bool AutomaticRotation () 
        {
            if (Time.unscaledTime - _lastManualRotationTime < _alignDelay) 
            {
                return false;
            }
            return true;
        }

        private void UpdateFocusPoint()
        {
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