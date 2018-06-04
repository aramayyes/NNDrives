using UnityEngine;

namespace GUI.Controllers
{
    /// <summary>
    /// Main camera controller.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        /// <summary>
        /// The lead object that camera has to follow.
        /// </summary>
        private GameObject leadObject;

        /// <summary>
        /// Offset between the camera and the lead object.
        /// </summary>
        private Vector3 offset;

        /// <summary>
        /// Gets or sets the lead object that camera has to follow.
        /// </summary>
        public GameObject LeadObject
        {
            get
            {
                return leadObject;
            }

            set
            {
                if (leadObject == null && value != null)
                {
                    /* 
                     * Calculate the initial offset between the camera and the lead object, 
                     * then save it for using later 
                     * to make the distance between the camera and the lead object always be the same.
                     */
                    offset = transform.position - value.transform.position;
                }

                leadObject = value;
            }
        }

        /// <summary>
        /// LateUpdate is called after Update each frame.
        /// </summary>
        private void LateUpdate()
        {
            // Make the camera follow the given object
            if (leadObject != null)
            {
                transform.position = leadObject.transform.position + offset;
            }
        }
    }
}