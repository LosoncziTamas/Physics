using UnityEngine;

[RequireComponent(typeof(PhysicsEngine))]
public class RocketEngine : MonoBehaviour
{
    [SerializeField] private Vector3 _thrustUnitVector;
    [SerializeField] private float _fuelMassInKilogram;
    [SerializeField] private float _thrustPercent;
    [SerializeField] private float _maxThrustInKiloNewton;

    private PhysicsEngine _physicsEngine;
    
    private void Start()
    {
        _physicsEngine = GetComponent<PhysicsEngine>();
    }

    private void FixedUpdate()
    {
        _physicsEngine.ForcesInNewton.Add(_thrustUnitVector);
    }
}
