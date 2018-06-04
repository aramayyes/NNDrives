using UnityEngine;

namespace RacingEnvironment
{
    /// <summary>
    /// Adds an ability for user to create a path. 
    /// </summary>
    public class PathCreator : MonoBehaviour
    {
        /// <summary>
        /// Instance of <see cref="Path"/> that contains all the details of path.
        /// </summary>
        [SerializeField, HideInInspector]
        private Path path;

        /// <summary>
        /// Gets a value that contains all the details of path.
        /// </summary>
        [HideInInspector]
        public Path Path
        {
            get
            {
                return path;
            }
        }

        /// <summary>
        /// Creates a new path (curve) which center points is the same as one of this object.
        /// </summary>
        public void CreatePath()
        {
            path = new Path(transform.position);
        }

        /// <summary>
        /// Is called when the user hits the Reset button in the Inspector's context menu.
        /// Creates a new path.
        /// </summary>
        private void Reset()
        {
            CreatePath();
        }
    }
}