using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class PhysicsEngine : MonoBehaviour
{
    [field:SerializeField]
    private Vector3 Velocity { get; [UsedImplicitly] set; }
    
    [SerializeField] private List<Vector3> _forces;
    [SerializeField] private float _mass;

    private Vector3 _totalForce;

    private Vector3 SumForces()
    {
        if (_forces == null)
        {
            return Vector3.zero;
        }
        var sum = Vector3.zero;
        foreach (var force in _forces)
        {
            sum += force;
        }
        return sum;
    }

    private void UpdateTotalForce()
    {
        _totalForce = SumForces();
    }

    private void UpdateVelocity()
    {
        var acceleration = _totalForce / _mass;
        Velocity += acceleration * Time.fixedDeltaTime;
    }
    
    private void FixedUpdate()
    {
        UpdateTotalForce();
        UpdateVelocity();
        var deltaS = Velocity * Time.fixedDeltaTime;
        transform.position += deltaS + _totalForce;
    }
}
