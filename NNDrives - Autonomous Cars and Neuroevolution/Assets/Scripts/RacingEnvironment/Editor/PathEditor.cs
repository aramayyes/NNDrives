using UnityEditor;

using UnityEngine;

namespace RacingEnvironment.Editors
{
    /// <summary>
    /// Editor for <see cref="PathCreator"/>.
    /// </summary>
    [CustomEditor(typeof(PathCreator))]
    public class PathEditor : Editor
    {
        /// <summary>
        /// Distance between the mouse cursor and a segment must be less than this value if the segment is the selected one.
        /// </summary>
        private const float SegmentSelectMinDistance = 0.1f;

        /// <summary>
        /// Index of the segment which is the nearest to mouse cursor position.
        /// </summary>
        private int selectedSegmentIndex = -1;

        /// <summary>
        /// Is responsible for creating the path.
        /// </summary>
        private PathCreator creator;

        /// <summary>
        /// Gets the object containing all details and information about the curve.
        /// </summary>
        private Path Path
        {
            get
            {
                return creator.Path;
            }
        }

        /// <summary>
        /// Makes custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();
            if (GUILayout.Button(InspectorResources.CreateNew))
            {
                Undo.RecordObject(creator, UndoResources.CreateNew);
                creator.CreatePath();
            }

            bool isClosed = GUILayout.Toggle(Path.IsClosed, InspectorResources.Closed);
            if (isClosed != Path.IsClosed)
            {
                Undo.RecordObject(creator, UndoResources.TogleClosed);
                Path.IsClosed = isClosed;
            }

            bool autoSetControlPoints = GUILayout.Toggle(Path.AutoSetControlPoints, InspectorResources.AutoSetControlPoints);
            if (autoSetControlPoints != Path.AutoSetControlPoints)
            {
                Undo.RecordObject(creator, UndoResources.TogleAutoSetControls);
                Path.AutoSetControlPoints = autoSetControlPoints;
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
            UserInteraction();
            Draw();
        }

        /// <summary>
        /// Allows user to create, modify and delete segments of curve.
        /// </summary>
        private void UserInteraction()
        {
            Event guiEvent = Event.current;
            Vector2 mousePosition = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

            // Split the segment if user clicks on it or add a new segment if user clicks somewhere else. 
            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
            {
                if (selectedSegmentIndex != -1)
                {
                    Undo.RecordObject(creator, UndoResources.SplitSegment);
                    Path.SplitSegment(mousePosition, selectedSegmentIndex);
                }
                else if (!Path.IsClosed)
                {
                    Undo.RecordObject(creator, UndoResources.AddSegment);
                    Path.AddSegment(mousePosition);
                }
            }

            // Move the path to the clicked position.
            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.control)
            {
                Undo.RecordObject(creator, UndoResources.MovePath);
                Path.Move(mousePosition);
            }

            // Remove the segment if user right-clicks on it.
            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
            {
                float minDistanceToAnchor = 0.1f;
                int closestAnchorIndex = -1;

                for (int i = 0; i < Path.PointsCount; i += 3)
                {
                    float distance = Vector2.Distance(mousePosition, Path[i]);
                    if (distance < minDistanceToAnchor)
                    {
                        minDistanceToAnchor = distance;
                        closestAnchorIndex = i;
                    }
                }

                if (closestAnchorIndex != -1)
                {
                    Undo.RecordObject(creator, UndoResources.DeleteSegment);
                    Path.DeleteSegment(closestAnchorIndex / 3);
                }
            }

            // Find the closest segment on mouse move to use it later when user left-clicks.
            if (guiEvent.type == EventType.MouseMove)
            {
                float minDistanceToSegment = SegmentSelectMinDistance;
                int newSelectedSegmentIndex = -1;

                for (int i = 0; i < Path.SegmentsCount; i++)
                {
                    Vector2[] points = Path.GetPointsInSegment(i);
                    float distance = HandleUtility.DistancePointBezier(mousePosition, points[0], points[3], points[1], points[2]);

                    if (distance < minDistanceToSegment)
                    {
                        minDistanceToSegment = distance;
                        newSelectedSegmentIndex = i;
                    }
                }

                if (newSelectedSegmentIndex != selectedSegmentIndex)
                {
                    selectedSegmentIndex = newSelectedSegmentIndex;
                    HandleUtility.Repaint();
                }
            }

            HandleUtility.AddDefaultControl(0);
        }

        /// <summary>
        /// Draws the curve and control points.
        /// </summary>
        private void Draw()
        {
            for (int i = 0; i < Path.SegmentsCount; i++)
            {
                Vector2[] pointsInSegment = Path.GetPointsInSegment(i);

                // Draw lines between control points.
                Handles.color = Color.black;
                Handles.DrawLine(pointsInSegment[0], pointsInSegment[1]);
                Handles.DrawLine(pointsInSegment[2], pointsInSegment[3]);

                // Draw current segment of the curve.
                Color segmentColor = (i == selectedSegmentIndex && Event.current.shift) ? Color.red : Color.green;
                Handles.DrawBezier(pointsInSegment[0], pointsInSegment[3], pointsInSegment[1], pointsInSegment[2], segmentColor, null, 2);
            }

            // Draw control points and add ability to move them.
            Handles.color = Color.red;
            for (int i = 0; i < Path.PointsCount; i++)
            {
                Vector2 newPosition = Handles.FreeMoveHandle(Path[i], Quaternion.identity, 0.2f, Vector2.zero, Handles.CylinderHandleCap);
                if (Path[i] != newPosition)
                {
                    Undo.RecordObject(creator, UndoResources.MovePoint);
                    Path.MovePoint(i, newPosition);
                }
            }
        }

        /// <summary>
        /// This function is called when the object becomes enabled and active. Create a new path (curve).
        /// </summary>
        private void OnEnable()
        {
            creator = target as PathCreator;
            if (creator.Path == null)
            {
                creator.CreatePath();
            }
        }

        /// <summary>
        /// Holds string resources for inspector.
        /// </summary>
        private static class InspectorResources
        {
            /// <summary>
            /// Text of the button to create new path.
            /// </summary>
            public const string CreateNew = "Create New";

            /// <summary>
            /// Label of the checkbox to make the path close/open.
            /// </summary>
            public const string Closed = "Closed";

            /// <summary>
            /// Label of the checkbox to set the control points of the path automatically.
            /// </summary>
            public const string AutoSetControlPoints = "Auto Set Control Points";
        }

        /// <summary>
        /// Holds string resources for Undo/Redo management.
        /// </summary>
        private static class UndoResources
        {
            /// <summary>
            /// New path was created.
            /// </summary>
            public const string CreateNew = "Create New";

            /// <summary>
            /// Path was made closed/opened.
            /// </summary>
            public const string TogleClosed = "Toggle closed";

            /// <summary>
            /// All control points of the path were automatically set.
            /// </summary>
            public const string TogleAutoSetControls = "Toggle auto set controls";

            /// <summary>
            /// A segment was split.
            /// </summary>
            public const string SplitSegment = "Split segment";

            /// <summary>
            /// A new segment was added.
            /// </summary>
            public const string AddSegment = "Add segment";

            /// <summary>
            /// A segment was deleted.
            /// </summary>
            public const string DeleteSegment = "DeleteSegment";

            /// <summary>
            /// The path was moved.
            /// </summary>
            public const string MovePath = "Move path";

            /// <summary>
            /// A point was moved. 
            /// </summary>
            public const string MovePoint = "Move point";
        }
    }
}