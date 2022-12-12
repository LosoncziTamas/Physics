using System;
using System.Collections;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    private static readonly Vector3 LaunchDirection = new Vector3(-1, 1, 0);
    
    [SerializeField] private PhysicsEngine _ballPrefab;
    [SerializeField] private float _maxLaunchSpeed;
    [SerializeField] private AudioSource _windUp;
    [SerializeField] private AudioSource _launch;

    private bool _beingPushed;
    private float _extraSpeedPerFrame;
    private readonly YieldInstruction _waitForPhysicsUpdate = new WaitForFixedUpdate();

    private void Awake()
    {
        _extraSpeedPerFrame = _maxLaunchSpeed * Time.fixedDeltaTime / _windUp.clip.length;
    }

    private IEnumerator OnMouseDown()
    {
        _beingPushed = true;
        var launchSpeed = 0f;
        _windUp.Play();
        while (_beingPushed)
        {
            var newSpeed = launchSpeed + _extraSpeedPerFrame;
            launchSpeed = Math.Min(newSpeed, _maxLaunchSpeed);
            if (launchSpeed >= _maxLaunchSpeed)
            {
                _beingPushed = false;
            }
            yield return _waitForPhysicsUpdate;
        }
        _windUp.Stop();
        Launch(launchSpeed);
    }
    
    private void OnMouseUp()
    {
        _beingPushed = false;
    }

    private void Launch(float launchSpeed)
    {
        _launch.Play();
        var ball = Instantiate(_ballPrefab);
        ball.Velocity = LaunchDirection.normalized * launchSpeed;
    }
}
