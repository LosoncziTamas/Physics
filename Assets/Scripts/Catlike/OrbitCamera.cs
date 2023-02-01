using UnityEngine;

namespace Catlike
{
    [RequireComponent(typeof(Camera))]
    public class OrbitCamera : MonoBehaviour
    {
        [SerializeField] private Transform _focus;
        [SerializeField, Range(1f, 20f)] private float _distance = 5f;
        [SerializeField, Min(0f)] private float _focusRadius = 1f;

        private Transform _cachedTransform;

        private void Start()
        {
            _cachedTransform = transform;
        }

        private void LateUpdate()
        {
            var focusPoint = _focus.position;
            var lookDirection = _cachedTransform.forward;
            _cachedTransform.localPosition = focusPoint - lookDirection * _distance;
        }
    }
}