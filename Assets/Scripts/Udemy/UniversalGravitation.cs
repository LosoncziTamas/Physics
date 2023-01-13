using System;
using System.Collections.Generic;
using UnityEngine;

public class UniversalGravitation : MonoBehaviour
{
    private const float G = 6.674e-11f;
    
    private List<PhysicsEngine> _physicsEngines;

    private void Start()
    {
        _physicsEngines = new List<PhysicsEngine>(FindObjectsOfType<PhysicsEngine>());
    }

    private void FixedUpdate()
    {
        CalculateAndUpdateGravityForFrame();
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
}
