using UnityEngine;

namespace Catlike
{
    public class Resettable : MonoBehaviour
    {
        private Vector3 _startPosition;
        private Rigidbody _rigidbody;

        private void Start()
        {
            _startPosition = transform.position;
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void ResetPhysics()
        {
            transform.position = _startPosition;
            if (_rigidbody)
            {
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
            }
        }
    }
}