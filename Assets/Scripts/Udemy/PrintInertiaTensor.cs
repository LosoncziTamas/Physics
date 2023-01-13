using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[ExecuteInEditMode]
public class PrintInertiaTensor : MonoBehaviour
{
    private Rigidbody _rigidbody;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        var inertiaTensor = _rigidbody.inertiaTensor;
        var centerOfMass = _rigidbody.centerOfMass;
        Debug.Log($"{name} inertia {inertiaTensor} center of mass {centerOfMass}");
    }
}
