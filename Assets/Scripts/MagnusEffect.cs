using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MagnusEffect : MonoBehaviour
{
    [SerializeField] private float _magnusConstant = 1.0f;
    private Rigidbody _rigidbody;
    
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        var magnus = Vector3.Cross(_rigidbody.angularVelocity, _rigidbody.velocity);
        _rigidbody.AddForce(magnus * _magnusConstant);
    }
}
