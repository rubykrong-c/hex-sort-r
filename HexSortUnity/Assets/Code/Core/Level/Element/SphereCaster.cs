using System.Collections.Generic;
using UnityEngine;

namespace Code.Core.Level.Element
{
    public class SphereCaster : MonoBehaviour
    {
        [SerializeField] private bool _enableGizmos;
        [SerializeField] private bool _doCheckParent;
        [SerializeField] private LayerMask _layerToCast;
        [SerializeField] private float _sphereRadius = 1f;
        [SerializeField] private float _castDistance = 1f;
        [SerializeField] private Vector3 _castDirection = Vector3.down;

        private Vector3 _lastCastPosition;

        private void Start()
        {
            _lastCastPosition = transform.position;
        }

        public TComponent[] CastSphereDown<TComponent>()
        {
            return CastSphereDown<TComponent>(transform.position);
        }

        public TComponent[] CastSphereDown<TComponent>(Vector3 posToCastAt)
        {
            _lastCastPosition = posToCastAt;

            List<TComponent> components = new List<TComponent>();
            RaycastHit[] hits = new RaycastHit[7];

            int collisionsCount = Physics.SphereCastNonAlloc(
                posToCastAt,
                _sphereRadius,
                _castDirection.normalized,
                hits,
                _castDistance,
                _layerToCast
            );

            if (collisionsCount != 0)
            {
                for (int i = 0; i < collisionsCount; i++)
                {
                    RaycastHit hit = hits[i];
                    if (hit.transform.gameObject.GetInstanceID() == transform.gameObject.GetInstanceID()) continue;

                    var gameObjectToCheck = _doCheckParent
                        ? hit.collider.gameObject.transform.parent.gameObject
                        : hit.collider.gameObject;

                    if (gameObjectToCheck.TryGetComponent(out TComponent component))
                    {
                        components.Add(component);
                    }
                }
            }

            return components.ToArray();
        }

        private void OnDrawGizmosSelected()
        {
            if (!_enableGizmos) return;

            Vector3 origin = _lastCastPosition;
            Vector3 direction = _castDirection.normalized;
            float distance = _castDistance;
            float radius = _sphereRadius;

            Vector3 endPoint = origin + direction * distance;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(origin, radius);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(endPoint, radius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin, endPoint);

            DrawWireSphereCast(origin, direction, distance, radius);
        }

        private void DrawWireSphereCast(Vector3 origin, Vector3 direction, float distance, float radius)
        {
            Vector3 end = origin + direction * distance;

            Vector3 up = Vector3.up * radius;
            Vector3 right = Vector3.right * radius;
            Vector3 forward = Vector3.forward * radius;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin + up, end + up);
            Gizmos.DrawLine(origin - up, end - up);
            Gizmos.DrawLine(origin + right, end + right);
            Gizmos.DrawLine(origin - right, end - right);
            Gizmos.DrawLine(origin + forward, end + forward);
            Gizmos.DrawLine(origin - forward, end - forward);
        }
    }
}