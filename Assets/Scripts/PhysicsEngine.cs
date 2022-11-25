using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

public class PhysicsEngine : MonoBehaviour
{
    [field:SerializeField]
    private Vector3 Velocity { get; [UsedImplicitly] set; }

    public List<Vector3> Forces { get; } = new();
    
    [SerializeField] private float _mass;
    [SerializeField] public bool _showTrails = true;
    
    private Vector3 _totalForce;
    private LineRenderer _lineRenderer;
    private int _numberOfForces;

    private void Start()
    {
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.startColor = _lineRenderer.endColor = Color.yellow;
        _lineRenderer.startWidth = _lineRenderer.endWidth = 0.2f;
        _lineRenderer.useWorldSpace = false;
    }

    private Vector3 SumForces()
    {
        if (Forces == null)
        {
            return Vector3.zero;
        }
        var sum = Vector3.zero;
        foreach (var force in Forces)
        {
            sum += force;
        }
        return sum;
    }

    private void UpdateTotalForce()
    {
        _totalForce = SumForces();
        Forces.Clear();
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

    private void OnDrawGizmos()
    {
        Handles.color = Color.red;
        var from = transform.position;
        foreach (var force in Forces)
        {
            Handles.DrawLine(from, from + force, 10);
        }
    }
    
    private void Update()
    {
        if (_showTrails)
        {
            _lineRenderer.enabled = true;
            _numberOfForces = Forces.Count;
            _lineRenderer.positionCount = _numberOfForces * 2;
            var i = 0;
            foreach (var forceVector in Forces)
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
