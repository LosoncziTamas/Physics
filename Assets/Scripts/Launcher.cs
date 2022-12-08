using System;
using System.Collections;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    [SerializeField] private PhysicsEngine _ballPrefab;
    [SerializeField] private float _maxLaunchSpeed;
    [SerializeField] private float _speedOfIncrease;
    [SerializeField] private AudioSource _windUp;
    [SerializeField] private AudioSource _launch;

    private bool _beingPushed;

    private IEnumerator OnMouseDown()
    {
        _beingPushed = true;
        var launchSpeed = 0f;
        _windUp.Play();
        while (_beingPushed)
        {
            yield return null;
            var newSpeed = launchSpeed + Time.deltaTime * _speedOfIncrease;
            launchSpeed = Math.Min(newSpeed, _maxLaunchSpeed);
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
        ball.Velocity = new Vector3(-1, 1, 0).normalized * launchSpeed;
    }
}
