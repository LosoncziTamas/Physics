using UnityEngine;

public class FluidDrag : MonoBehaviour
{
    [Range(1.0f, 2.0f)]
    [SerializeField] private float _velocityExponent;
    [SerializeField] private float _dragConstant;

    private PhysicsEngine _physicsEngine;

    private void Start()
    {
        _physicsEngine = GetComponent<PhysicsEngine>();
    }

    private float CalculateDrag(float velocity)
    {
        var drag = Mathf.Pow(velocity, _velocityExponent) * _dragConstant;
        return drag;
    }

    private void FixedUpdate()
    {
        var velocity = _physicsEngine.Velocity;
        var speed = velocity.magnitude;
        var dragSize = CalculateDrag(speed);
        var dragVector = dragSize * -velocity.normalized;
        _physicsEngine.AddForce(dragVector);
    }
}
