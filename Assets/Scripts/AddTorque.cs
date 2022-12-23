using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AddTorque : MonoBehaviour
{
    [SerializeField] private Vector3 _torque;
    [SerializeField] private float _torqueTime;

    private Rigidbody _rigidbody;
    
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        if (_torqueTime > 0)
        {
            _torqueTime -= Time.fixedDeltaTime;
            _rigidbody.AddTorque(_torque);
        }
    }
}
