using UnityEngine;

namespace GUI.Controllers
{
    /// <summary>
    /// This class is responsible for crossroad's behavior and lifecycle.
    /// </summary>
    [ExecuteInEditMode]
    public class CrossroadController : MonoBehaviour
    {
        /// <summary>
        /// The active side of the crossroad.
        /// </summary>
        [SerializeField]
        private GameObject sideToPass = null;

        /// <summary>
        /// Gets the position of the active side of the crossroad. (To which the cars should move)
        /// </summary>
        public Vector3 PointPosition
        {
            get
            {
                return sideToPass.transform.position;
            }
        }
    }
}