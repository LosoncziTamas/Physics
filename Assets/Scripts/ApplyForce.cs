using System;
using UnityEngine;

[RequireComponent(typeof(PhysicsEngine))]
public class ApplyForce : MonoBehaviour
{
    [SerializeField] private Vector3 _force;

    private PhysicsEngine _physicsEngine;
    
    private void Start()
    {
        _physicsEngine = GetComponent<PhysicsEngine>();
    }

    private void FixedUpdate()
    {
        _physicsEngine.Forces.Add(_force);
    }
}
