using UnityEngine;

[RequireComponent(typeof(PhysicsEngine))]
public class RocketEngine : MonoBehaviour
{
    private const float KiloNewtonToNewton = 1000;
    
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

    private float CalculateFuelForFrame()
    {
        var effectiveExhaustVelocityInMetersPerSecond = 4462f;
        var exhaustMassFlow = _currentThrustInNewton / effectiveExhaustVelocityInMetersPerSecond;
        return exhaustMassFlow * Time.fixedDeltaTime;
    }
    
    private void FixedUpdate()
    {
        var fuelForFrame = CalculateFuelForFrame();
        if (_fuelMassInKilogram > fuelForFrame)
        {
            _fuelMassInKilogram -= fuelForFrame;
            _physicsEngine.MassInKilogram -= fuelForFrame;
            ExertForce();
        }
    }

    private void ExertForce()
    {
        var thrust = _maxThrustInKiloNewton * _thrustPercent * KiloNewtonToNewton;
        _currentThrustInNewton = thrust;
        var thrustVectorInNewton = _thrustUnitVector.normalized * thrust;
        _physicsEngine.ForcesInNewton.Add(thrustVectorInNewton);
    }
}
