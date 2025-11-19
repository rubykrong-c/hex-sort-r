using UnityEngine;

namespace Code.Utils
{
    public static class RaycastHelper
    {
        private static readonly RaycastHit[] _raycastBuffer = new RaycastHit[16];

        public static bool RaycastThroughObstacles(Ray ray, float maxDistance, LayerMask targetMask,
            out RaycastHit validHit)
        {
            validHit = default;

            int hitCount = Physics.RaycastNonAlloc(ray, _raycastBuffer, maxDistance);
            if (hitCount == 0)
                return false;

            int closestValidIndex = -1;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                var hit = _raycastBuffer[i];
                bool isInTargetMask = ((1 << hit.collider.gameObject.layer) & targetMask) != 0;

                if (isInTargetMask && hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    closestValidIndex = i;
                }
            }

            if (closestValidIndex >= 0)
            {
                validHit = _raycastBuffer[closestValidIndex];
                return true;
            }

            return false;
        }
    }
}