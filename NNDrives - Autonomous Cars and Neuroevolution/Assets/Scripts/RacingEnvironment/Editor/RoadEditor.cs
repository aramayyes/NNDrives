using System;

using UnityEditor;

using UnityEngine;

namespace RacingEnvironment.Editors
{
    /// <summary>
    /// Editor for <see cref="RoadCreator"/>.
    /// </summary>
    [CustomEditor(typeof(RoadCreator))]
    public class RoadEditor : Editor
    {
        /// <summary>
        /// Object which is responsible for creating the road.
        /// </summary>
        private RoadCreator creator;

        /// <summary>
        /// Makes custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();
            creator.UseNarrowSegments = EditorGUILayout.Toggle("Use narrow segments", creator.UseNarrowSegments);
            using (var group = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(creator.UseNarrowSegments)))
            {
                // Show the group only if 'creator.UseNarrowSegments' is checked.
                if (group.visible)
                {
                    EditorGUI.indentLevel++;

                    creator.MinRoadWidth = EditorGUILayout.Slider("Min road width", creator.MinRoadWidth, 1.0f, 5.0f);
                    creator.DistanceBetweenNarrowSegments = EditorGUILayout.IntField("Distance between narrow segments", creator.DistanceBetweenNarrowSegments);
                    if (creator.DistanceBetweenNarrowSegments <= 0)
                    {
                        creator.DistanceBetweenNarrowSegments = 10;
                    }

                    creator.NarrowSegmentLength = EditorGUILayout.IntSlider("Narrow segment length", creator.NarrowSegmentLength, 5, 800);

                    EditorGUI.indentLevel--;
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }
        }

        /// <summary>
        /// Handles events in the scene view.
        /// </summary>
        private void OnSceneGUI()
        {
            if (creator.AutoUpdate && Event.current.type == EventType.Repaint)
            {
                creator.UpdateRoad();
            }
        }

        /// <summary>
        /// This function is called when the object becomes enabled and active. Create a new path (curve).
        /// </summary>
        private void OnEnable()
        {
            creator = target as RoadCreator;

            MeshRenderer renderer = creator.GetComponent<MeshRenderer>();
            if (renderer.sharedMaterial == null)
            {
                renderer.sharedMaterial = Resources.Load<Material>("Materials/Road");
            }
        }

        /// <summary>
        /// Holds string resources for inspector.
        /// </summary>
        private static class InspectorResources
        {
            /// <summary>
            /// Text of the checkbox to make the path use narrow segments.
            /// </summary>
            public const string UseNarrowSegments = "Use Narrow Segments";

            /// <summary>
            /// Label of the slider to set min width the road can have.
            /// </summary>
            public const string MinRoadWidth = "Min Road Width";

            /// <summary>
            /// Label of the textfield to set the distance between narrow segments.
            /// </summary>
            public const string DistanceBetweenNarrowSegments = "Distance Between Narrow Segments";

            /// <summary>
            /// Label of the slider to set length of the narrow segments.
            /// </summary>
            public const string NarrowSegmentsLength = "Narrow Segment Length";
        }
    }
}