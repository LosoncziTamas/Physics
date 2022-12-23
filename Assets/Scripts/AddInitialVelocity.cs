using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AddInitialVelocity : MonoBehaviour
{
    [SerializeField] private Vector3 _velocity;
    [SerializeField] private Vector3 _angularVelocity;

    private Rigidbody _rigidbody;
    
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.velocity = _velocity;
        _rigidbody.angularVelocity = _angularVelocity;
    }
}
