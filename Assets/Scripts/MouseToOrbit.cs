using UnityEngine;

public class MouseToOrbit : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _distance = 10.0f;
    [SerializeField] private float _xSpeed = 250.0f;
    [SerializeField] private float _ySpeed = 120.0f;
    [SerializeField] private float _yMinLimit = -20;
    [SerializeField] private float _yMaxLimit = 80;
    
    private float _x;
    private float _y;

    private void Start()
    {
        var angles = transform.eulerAngles;
        _x = angles.y;
        _y = angles.x;

        // Make the rigid body not change rotation
        if (GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().freezeRotation = true;
        }
    }

    private void LateUpdate()
    {
        if (!_target)
        {
            return;
        }
        _x += Input.GetAxis("Mouse X") * _xSpeed * 0.02f;
        _y -= Input.GetAxis("Mouse Y") * _ySpeed * 0.02f;
        _distance += Input.mouseScrollDelta.y * 0.2f;
        _y = ClampAngle(_y, _yMinLimit, _yMaxLimit);
 		       
        var rotation = Quaternion.Euler(_y, _x, 0);
        var position = rotation * new Vector3(0.0f, 0.0f, -_distance) + _target.position;
        
        transform.rotation = rotation;
        transform.position = position;
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
        {
            angle += 360;
        }
        if (angle > 360)
        {
            angle -= 360;
        }
        return Mathf.Clamp(angle, min, max);
    }
}
