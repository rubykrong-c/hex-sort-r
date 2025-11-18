using System.Collections.Generic;
using UnityEngine;

namespace Code.Tools
{

    /// <summary>
    /// Standalone helper that only spawns circular hexagon grids.
    /// Inspired by GridGenerator.GenerateHexagonGrid_Circular but kept self contained.
    /// </summary>
    [ExecuteInEditMode]
    public class GridLevelEditor : MonoBehaviour
    {
         private static readonly Vector2Int[] axialDirections = new[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(1, -1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(-1, 1),
            new Vector2Int(0, 1)
        };

        [Tooltip("Toggle to rebuild the grid in edit / play mode.")]
        public bool spawnGrid = false;
        public bool spawnAtStart = true;
        public bool rotateHexagon = false;
        public bool centerPivot = true;
        [Tooltip("If true hexes share corners (vertex based). If false they align edge-to-edge (flat top).")]
        public bool useVertexLayout = true;

        [Space]
        [Tooltip("Prefab that will be used for every hexagon tile.")]
        public GameObject hexagonPrefab;
        public Material gridMaterial;

        [Space]
        [Range(0.5f, 4f)]
        public float hexWidth = 1f;
        [Range(0, 10)]
        public int outerRings = 1;

        private readonly List<Transform> spawnedHexagons = new List<Transform>();

        private void Start()
        {
            if (spawnAtStart)
                spawnGrid = true;
        }

        private void Update()
        {
            if (!spawnGrid)
                return;

            spawnGrid = false;
            SpawnCircularGrid();
        }

        public void SpawnCircularGrid()
        {
            ClearChildren();
            spawnedHexagons.Clear();

            if (hexagonPrefab == null)
                return;

            HashSet<Vector2Int> spawnedCoords = new HashSet<Vector2Int>();

            void SpawnHexagon(Vector2Int axialCoord)
            {
                if (spawnedCoords.Contains(axialCoord))
                    return;

                spawnedCoords.Add(axialCoord);

                Vector3 position = AxialToWorld(axialCoord);
                GameObject hex = Instantiate(hexagonPrefab, transform);
                spawnedHexagons.Add(hex.transform);
                hex.transform.localPosition = position;
                hex.transform.localRotation = Quaternion.identity;

                if (rotateHexagon)
                {
                    hex.transform.Rotate(Vector3.right, -90f, Space.Self);
                    hex.transform.Rotate(Vector3.forward, 30f, Space.Self);
                }

                if (gridMaterial != null)
                {
                    MeshRenderer renderer = hex.GetComponent<MeshRenderer>();
                    if (renderer != null) renderer.material = gridMaterial;
                }
            }

            SpawnHexagon(Vector2Int.zero);

            for (int radius = 1; radius <= outerRings; radius++)
            {
                Vector2Int axialPos = Multiply(axialDirections[4], radius);
                for (int side = 0; side < 6; side++)
                {
                    for (int step = 0; step < radius; step++)
                    {
                        SpawnHexagon(axialPos);
                        axialPos += axialDirections[side];
                    }
                }
            }

            if (centerPivot)
                CenterPivot();
        }

        private void ClearChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);
        }

        private void CenterPivot()
        {
            if (transform.childCount == 0)
                return;

            Vector3 centroid = Vector3.zero;
            foreach (Transform child in transform)
                centroid += child.localPosition;

            centroid /= transform.childCount;

            foreach (Transform child in transform)
                child.localPosition -= centroid;
        }

        private Vector3 AxialToWorld(Vector2Int axial)
        {
            float radius = hexWidth * 0.5f;
            if (useVertexLayout)
            {
                float x = Mathf.Sqrt(3f) * radius * (axial.x + axial.y * 0.5f);
                float z = 1.5f * radius * axial.y;
                return new Vector3(x, 0f, z);
            }
            else
            {
                float x = 1.5f * radius * axial.x;
                float z = Mathf.Sqrt(3f) * radius * (axial.y + axial.x * 0.5f);
                return new Vector3(x, 0f, z);
            }
        }

        private Vector2Int Multiply(Vector2Int input, int value)
        {
            return new Vector2Int(input.x * value, input.y * value);
        }
    }
}