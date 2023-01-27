using System.Collections.Generic;
using UnityEngine;

namespace Catlike
{
    public class Reset : MonoBehaviour
    {
        private List<Resettable> _resettables;
        private void Start()
        {
            _resettables = new List<Resettable>(FindObjectsOfType<Resettable>());
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Reset"))
            {
                foreach (var resettable in _resettables)
                {
                    resettable.ResetPhysics();
                }
            }
        }
    }
}