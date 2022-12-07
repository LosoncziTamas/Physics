using UnityEngine;

public class Launcher : MonoBehaviour
{
    [SerializeField] private PhysicsEngine _ballPrefab;
    
    private void Launch()
    {
        
    }

    private void OnMouseDown()
    {
        Launch();
    }
}
