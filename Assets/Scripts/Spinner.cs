using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Spinner : MonoBehaviour
{
    private Rigidbody _rigidbody;
    
    [SerializeField] private Vector3 _angularVelocity = new(4, 0, 0);

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Spin"))
        {
            _rigidbody.angularVelocity = _angularVelocity;
        }
    }
}
