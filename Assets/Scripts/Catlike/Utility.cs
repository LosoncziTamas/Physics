using UnityEngine;

namespace Catlike
{
    public static class Utility
    {
        public static bool MaskIsSet(LayerMask layerMask, int layerIndex)
        {
            return (layerMask & (1 << layerIndex)) != 0;
        }
    }
}