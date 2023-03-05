using System.Collections.Generic;
using UnityEngine;

namespace Catlike
{
    public static class CustomGravity
    {
        private static List<GravitySource> _sources = new();

        public static void Register(GravitySource source)
        {
            Debug.Assert(!_sources.Contains(source), "Duplicated registration of gravity source!", source);
            _sources.Add(source);
        }

        public static void Unregister(GravitySource source)
        {
            Debug.Assert(_sources.Contains(source), "Unregistration of unknown gravity source!", source);
            _sources.Remove(source);
        }
        
        public static Vector3 GetGravity(Vector3 position)
        {
            var summedGravity = Vector3.zero;
            foreach (var gravitySource in _sources)
            {
                summedGravity += gravitySource.GetGravity(position);
            }
            return summedGravity;
        }

        public static Vector3 GetUpAxis(Vector3 position)
        {
            var g = GetGravity(position);
            return -g.normalized;
        }

        public static Vector3 GetGravity(Vector3 position, out Vector3 upAxis)
        {
            var g = GetGravity(position);
            upAxis = -g.normalized;
            return g;
        }
    }
}
