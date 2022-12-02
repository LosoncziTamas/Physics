using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

public class PhysicsEngine : MonoBehaviour
{
    private const float G = 6.674e-11f;
    
    [field:SerializeField] private Vector3 Velocity { get; [UsedImplicitly] set; }
    [field:SerializeField] public List<Vector3> ForcesInNewton { get; [UsedImplicitly] set; } = new();
    [field:SerializeField] public float MassInKilogram { get; [UsedImplicitly] set; }
    
    [SerializeField] public bool _showTrails = true; 
    
    private LineRenderer _lineRenderer;
    private List<PhysicsEngine> _physicsEngines;

    private void Start()
    {
        _physicsEngines = new List<PhysicsEngine>(FindObjectsOfType<PhysicsEngine>());
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.startColor = _lineRenderer.endColor = Color.yellow;
        _lineRenderer.startWidth = _lineRenderer.endWidth = 0.2f;
        _lineRenderer.useWorldSpace = false;
    }

    private Vector3 SumForces()
    {
        if (ForcesInNewton == null)
        {
            return Vector3.zero;
        }
        var sum = Vector3.zero;
        foreach (var force in ForcesInNewton)
        {
            sum += force;
        }
        return sum;
    }

    private Vector3 CalculateForceForFrame()
    {
        var result = SumForces();
        ForcesInNewton.Clear();
        return result;
    }

    private Vector3 CalculateAccelerationForFrame(Vector3 totalForce)
    {
        var acceleration = totalForce / MassInKilogram;
        return acceleration * Time.fixedDeltaTime;
    }

    private void AddForce(Vector3 force)
    {
        ForcesInNewton.Add(force);
    }

    private void CalculateAndUpdateGravityForFrame()
    {
        foreach (var engineA in _physicsEngines)
        {
            foreach (var engineB in _physicsEngines)
            {
                if (engineA == engineB)
                {
                    continue;
                }
                var massProduct = engineA.MassInKilogram * engineB.MassInKilogram;
                var offset = engineA.transform.position - engineB.transform.position;
                var distanceSquare = offset.magnitude * offset.magnitude;
                var gravityMagnitude = G * massProduct / distanceSquare;
                var gravityVector = gravityMagnitude * offset.normalized;
                engineA.AddForce(-gravityVector);
            }
        }
    }
    
    private void FixedUpdate()
    {
        var force = CalculateForceForFrame();
        Velocity += CalculateAccelerationForFrame(force);
        CalculateAndUpdateGravityForFrame();
        var delta = Velocity * Time.fixedDeltaTime;
        transform.position += delta;
    }

    private void OnDrawGizmos()
    {
        Handles.color = Color.red;
        var from = transform.position;
        foreach (var force in ForcesInNewton)
        {
            Handles.DrawLine(from, from + force, 10);
        }
    }
    
    private void Update()
    {
        if (_showTrails)
        {
            _lineRenderer.enabled = true;
            var numberOfForces = ForcesInNewton.Count;
            _lineRenderer.positionCount = numberOfForces * 2;
            var i = 0;
            foreach (var forceVector in ForcesInNewton)
            {
                _lineRenderer.SetPosition(i, Vector3.zero);
                _lineRenderer.SetPosition(i + 1, -forceVector);
                i += 2;
            }
        }
        else
        {
            _lineRenderer.enabled = false;
        }
    }
}
