using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class PhysicsEngine : MonoBehaviour
{
    [field:SerializeField]
    private Vector3 Velocity { get; [UsedImplicitly] set; }
    
    [SerializeField] private List<Vector3> _forces;

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

    private void FixedUpdate()
    {
        var totalForce = SumForces();
        if (totalForce != Vector3.zero)
        {
            Debug.LogError("Unbalanced force");
            return;
        }
        var deltaS = Velocity * Time.fixedDeltaTime;
        transform.position += deltaS + totalForce;
    }
}
