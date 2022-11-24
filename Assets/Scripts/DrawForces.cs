using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
[RequireComponent(typeof(PhysicsEngine))]
public class DrawForces : MonoBehaviour
{
    [SerializeField] public bool _showTrails = true;

    private List<Vector3> _forceVectorList = new();
    private LineRenderer _lineRenderer;
    private int _numberOfForces;

    private void Start()
    {
        _forceVectorList = GetComponent<PhysicsEngine>().Forces;
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.startColor = _lineRenderer.endColor = Color.yellow;
        _lineRenderer.startWidth = _lineRenderer.endWidth = 0.2f;
        _lineRenderer.useWorldSpace = false;
    }

    private void Update()
    {
        if (_showTrails)
        {
            _lineRenderer.enabled = true;
            _numberOfForces = _forceVectorList.Count;
            _lineRenderer.positionCount = _numberOfForces * 2;
            var i = 0;
            foreach (var forceVector in _forceVectorList)
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