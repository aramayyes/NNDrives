using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Helpers;

using UnityEngine;

namespace RacingEnvironment
{
    /// <summary>
    /// Adds an ability for user to create a path.
    /// </summary>
    [RequireComponent(typeof(PathCreator))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class RoadCreator : MonoBehaviour
    {
        /// <summary>
        /// Prefab to create checkpoints.
        /// </summary>
        [SerializeField]
        private GameObject checkpointPrefab = null;

        /// <summary>
        /// Distance between evenly spaced points on the curve.
        /// </summary>
        [SerializeField, Range(0.1f, 1.5f)]
        private float spacing = 1;

        /// <summary>
        /// Width of the road (mesh).
        /// </summary>
        [SerializeField]
        private float roadWidth = 2.5f;

        /// <summary>
        /// Value indicating whether this road should have edge collider.
        /// </summary>
        [SerializeField]
        private bool useEdgeCollider = true;

        /// <summary>
        /// Value indicating whether this road must be updated if the curve was modified.
        /// </summary>
        [SerializeField]
        private bool autoUpdate = false;

        /// <summary>
        /// Contains all checkpoints' game objects.
        /// </summary>
        [SerializeField, HideInInspector]
        private List<GameObject> checkpoints = new List<GameObject>();

        /// <summary>
        /// Value indicating whether there should be narrow segments on the road.
        /// </summary>
        [SerializeField, HideInInspector]
        private bool useNarrowSegments = false;

        /// <summary>
        /// Holds min width the road can have at narrow segments.
        /// </summary>
        [SerializeField, HideInInspector]
        private float minRoadWidth = 1.5f;

        /// <summary>
        /// Holds the distance between narrow segments on the road.
        /// </summary>
        [SerializeField, HideInInspector]
        private int distanceBetweenNarrowSegments = 140;

        /// <summary>
        /// Holds the length of narrow segments on the road.
        /// </summary>
        [SerializeField, HideInInspector]
        private int narrowSegmentLength = 30;

        /// <summary>
        /// Holds the left edge collider of the road.
        /// </summary>
        [SerializeField, HideInInspector]
        private GameObject leftColliderGameObject = null;

        /// <summary>
        /// Holds the right edge collider of the road.
        /// </summary>
        [SerializeField, HideInInspector]
        private GameObject rightColliderGameObject = null;

        /// <summary>
        /// Gets a value indicating whether this road must be updated if the curve was modified.
        /// </summary>
        public bool AutoUpdate
        {
            get
            {
                return autoUpdate;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether there should be narrow segments on the road.
        /// </summary>
        public bool UseNarrowSegments
        {
            get
            {
                return useNarrowSegments;
            }

            set
            {
                useNarrowSegments = value;
            }
        }

        /// <summary>
        /// Gets or sets min width the road can have at narrow segments.
        /// </summary>
        public float MinRoadWidth
        {
            get
            {
                return minRoadWidth;
            }

            set
            {
                minRoadWidth = value;
            }
        }

        /// <summary>
        /// Gets or sets the distance between narrow segments on the road.
        /// </summary>
        public int DistanceBetweenNarrowSegments
        {
            get
            {
                return distanceBetweenNarrowSegments;
            }

            set
            {
                distanceBetweenNarrowSegments = value;
            }
        }

        /// <summary>
        /// Gets or sets the length of narrow segments on the road.
        /// </summary>
        public int NarrowSegmentLength
        {
            get
            {
                return narrowSegmentLength;
            }

            set
            {
                narrowSegmentLength = value;
            }
        }

        /// <summary>
        /// Gets all checkpoints of this road.
        /// </summary>
        /// <returns>All checkpoints located on this road.</returns>
        public ReadOnlyCollection<GameObject> GetCheckpoints()
        {
            return checkpoints.AsReadOnly();
        }

        /// <summary>
        /// Updates the road.
        /// </summary>
        public void UpdateRoad()
        {
            Path path = GetComponent<PathCreator>().Path;
            Vector2[] points = path.CalculateEvenlySpacedPoints(spacing);

            Mesh mesh = CreateRoadMesh(points, path.IsClosed);
            GetComponent<MeshFilter>().mesh = mesh;

            // Create a collider.
            if (useEdgeCollider)
            {
                Vector2[] edgePoints = mesh.vertices.Select(v3 => (Vector2)v3).ToArray();
                Vector2[] leftPoints = edgePoints.Where((point, index) => index % 2 == 0).ToArray();
                Vector2[] rightsPoints = edgePoints.Skip(1).Where((point, index) => index % 2 == 0).ToArray();

                EdgeCollider2D leftCollider = null;
                EdgeCollider2D rightCollider = null;

                if (leftColliderGameObject == null)
                {
                    leftColliderGameObject = new GameObject
                    {
                        name = "LeftCollider",
                        tag = Tags.Wall,
                        layer = LayerMask.NameToLayer(Layers.Wall)
                    };
                    leftColliderGameObject.transform.parent = gameObject.transform;
                }

                leftCollider = leftColliderGameObject.GetComponent<EdgeCollider2D>();
                if (leftCollider == null)
                {
                    leftCollider = leftColliderGameObject.AddComponent<EdgeCollider2D>();
                }

                if (rightColliderGameObject == null)
                {
                    rightColliderGameObject = new GameObject
                    {
                        name = "RightCollider",
                        tag = Tags.Wall,
                        layer = LayerMask.NameToLayer(Layers.Wall)
                    };
                    rightColliderGameObject.transform.parent = gameObject.transform;
                }

                rightCollider = rightColliderGameObject.GetComponent<EdgeCollider2D>();
                if (rightCollider == null)
                {
                    rightCollider = rightColliderGameObject.AddComponent<EdgeCollider2D>();
                }

                leftCollider.points = leftPoints;
                rightCollider.points = rightsPoints;
            }
            else
            {
                DestroyImmediate(leftColliderGameObject);
                DestroyImmediate(rightColliderGameObject);
            }

            // Add checkpoints
            if (checkpointPrefab != null && !Application.isPlaying)
            {
                Vector2[] checkpoints = path.CalculateEvenlySpacedPoints(5f, 0.5f);
                CreateCheckpoints(checkpoints);
            }
        }

        /// <summary>
        /// Creates a mesh using the given points.
        /// </summary>
        /// <param name="points">Points for constructing the vertices and triangles for mesh.</param>
        /// <param name="isClosed">Value indication whether the path is closed.</param>
        /// <remarks>
        /// If <see cref="useNarrowSegments"/> option is turned on, creates narrow segments on the road having min width
        ///  equal to <see cref="minRoadWidth"/>.
        /// For each segment a fragment of the road with length of 'narrowSegmentLength'/2 narrows to the point of the road
        ///  where it's smallest in width, and the next fragment with the same length widens starting from that point.
        /// </remarks>
        /// <returns>Created instance of <see cref="Mesh"/>.</returns>
        private Mesh CreateRoadMesh(Vector2[] points, bool isClosed)
        {
            if (points.Length < 2)
            {
                throw new ArgumentException("Invalid argument {0}. The array must contain at least 2 points.");
            }

            var vertices = new Vector3[points.Length * 2];
            var uvs = new Vector2[vertices.Length];

            int trianglesCount = (2 * (points.Length - 1)) + (isClosed ? 2 : 0);
            int[] triangles = new int[trianglesCount * 3];

            int vertexIndex = 0;
            int triangleIndex = 0;

            for (int i = 0; i < points.Length; i++)
            {
                // Forward is the sum of vector going from the previous point and vector going to the next point.
                Vector2 forward = Vector2.zero;

                if (i < points.Length - 1 || isClosed)
                {
                    forward += points[(i + 1) % points.Length] - points[i];
                }

                if (i > 0 || isClosed)
                {
                    forward += points[i] - points[(i - 1 + points.Length) % points.Length];
                }

                forward.Normalize();

                // Left is a normalized vector which is perpendicular to forward vector.
                var left = new Vector2(-forward.y, forward.x);

                float coefficient = 0.5f;
                if (useNarrowSegments)
                {
                    int segmentHalfLength = narrowSegmentLength / 2;
                    int pointIndex = i % distanceBetweenNarrowSegments;

                    // Road narrows or widens in the vicinity of every 'distanceBetweenNarrowSegments'-th point.
                    if (pointIndex >= distanceBetweenNarrowSegments - segmentHalfLength)
                    {
                        int x1 = distanceBetweenNarrowSegments - segmentHalfLength;
                        int x2 = distanceBetweenNarrowSegments - 1;

                        float y1 = 0.5f;
                        float y2 = 0.5f * (minRoadWidth / roadWidth);

                        coefficient = (((y1 - y2) * (pointIndex - x1)) / (x1 - x2)) + y1;
                    }
                    else if (pointIndex <= segmentHalfLength && i >= distanceBetweenNarrowSegments)
                    {
                        int x1 = 0;
                        int x2 = segmentHalfLength - 1;

                        float y1 = 0.5f * (minRoadWidth / roadWidth);
                        float y2 = 0.5f;

                        coefficient = (((y1 - y2) * (pointIndex - x1)) / (x1 - x2)) + y1;
                    }
                }

                vertices[vertexIndex] = points[i] + (left * roadWidth * coefficient);
                vertices[vertexIndex + 1] = points[i] - (left * roadWidth * coefficient);

                float completionPercent = 0.001f * i;
                float v = 1 - Mathf.Abs((2 * completionPercent) - 1);
                uvs[vertexIndex] = new Vector2(0, v);
                uvs[vertexIndex + 1] = new Vector2(1, v);

                // Make sure closed case is also handled.
                if (i < points.Length - 1 || isClosed)
                {
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = (vertexIndex + 2) % vertices.Length;
                    triangles[triangleIndex + 2] = vertexIndex + 1;

                    triangles[triangleIndex + 3] = vertexIndex + 1;
                    triangles[triangleIndex + 4] = (vertexIndex + 2) % vertices.Length;
                    triangles[triangleIndex + 5] = (vertexIndex + 3) % vertices.Length;
                }

                vertexIndex += 2;
                triangleIndex += 6;
            }

            var mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles,
                uv = uvs
            };

            return mesh;
        }

        /// <summary>
        /// Creates game objects at given positions for checkpoints.
        /// </summary>
        /// <param name="positions">Positions of checkpoints.</param>
        private void CreateCheckpoints(Vector2[] positions)
        {
            foreach (GameObject checkpoint in checkpoints)
            {
                DestroyImmediate(checkpoint);
            }

            checkpoints.Clear();
            for (int i = 0; i < positions.Length; i++)
            {
                GameObject checkpointObject = Instantiate(checkpointPrefab);
                checkpointObject.transform.SetParent(transform);
                checkpointObject.transform.position = positions[i];
                checkpointObject.transform.localScale = new Vector3(roadWidth / 20, roadWidth, 0);

                checkpointObject.name = string.Format("Ch_{0}", i + 1);
                checkpointObject.tag = Tags.Checkpoint;

                checkpointObject.GetComponent<BoxCollider2D>().isTrigger = true;

                // Forward is the sum of vector going from the previous point and vector going to the next point.
                Vector2 forward = Vector2.zero;

                if (i < positions.Length - 1)
                {
                    forward += positions[i + 1] - positions[i];
                }

                if (i > 0)
                {
                    forward += positions[i] - positions[i - 1];
                }

                forward.Normalize();

                // Left is a normalized vector which is perpendicular to forward vector.
                var direction = new Vector2(-forward.y, forward.x);
                direction.Normalize();

                // Calculate the rotation angle
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Quaternion rotation = new Quaternion
                {
                    eulerAngles = new Vector3(0, 0, angle - 90)
                };

                // Rotate the checkpoint object
                checkpointObject.transform.rotation = rotation;

                checkpoints.Add(checkpointObject);
            }
        }
    }
}