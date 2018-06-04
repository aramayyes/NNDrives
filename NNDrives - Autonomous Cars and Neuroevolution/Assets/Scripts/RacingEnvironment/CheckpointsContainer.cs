using System.Collections.Generic;

using UnityEngine;

namespace RacingEnvironment
{
    /// <summary>
    /// Helper class, that supports checkpoints storing for a road.
    /// </summary>
    [RequireComponent(typeof(RoadCreator))]
    public class CheckpointsContainer : MonoBehaviour
    {
        /// <summary>
        /// Gets the checkpoints of road attached to this gameobject.
        /// </summary>
        /// <returns>All checkpoints of the attached road.</returns>
        public IList<GameObject> GetCheckpoints()
        {
            return GetComponent<RoadCreator>().GetCheckpoints();
        }
    }
}