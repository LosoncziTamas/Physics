using UnityEngine;

namespace Catlike
{
    [RequireComponent(typeof(Collider))]
    public class AccelerationZone : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _speed = 10.0f;

        private void OnTriggerEnter(Collider other)
        {
            var rb = other.attachedRigidbody;
            if (rb)
            {
                Accelerate(rb);
            }
        }

        private void Accelerate(Rigidbody rb)
        {
            var velocity = rb.velocity;
            if (velocity.y > _speed)
            {
                return;
            }
            velocity.y = _speed;
            rb.velocity = velocity;
            if (rb.TryGetComponent(out MovingSphereRigidbody sphere))
            {
                sphere.PreventSnapToGround();
            }
        }
    }
}