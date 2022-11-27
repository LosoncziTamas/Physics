using UnityEngine;

[RequireComponent(typeof(PhysicsEngine))]
public class RocketEngine : MonoBehaviour
{
    [SerializeField] private Vector3 _thrustUnitVector;
    [SerializeField] private float _fuelMassInKilogram;
    [Range(0, 1)]
    [SerializeField] private float _thrustPercent;
    [SerializeField] private float _maxThrustInKiloNewton;

    private PhysicsEngine _physicsEngine;
    private float _currentThrustInNewton;
    
    private void Start()
    {
        _physicsEngine = GetComponent<PhysicsEngine>();
        _physicsEngine.MassInKilogram += _fuelMassInKilogram;
    }

    private void FixedUpdate()
    {
        _physicsEngine.ForcesInNewton.Add(_thrustUnitVector);
    }
}
