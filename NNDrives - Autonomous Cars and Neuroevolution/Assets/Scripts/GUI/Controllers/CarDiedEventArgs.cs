using System;

using UnityEngine;

namespace GUI.Controllers
{
    /// <summary>
    /// Used to pass the car details to subscribers 
    /// when the <see cref="CarController.Died"/> event is raised.
    /// </summary>
    public class CarDiedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CarDiedEventArgs"/> class with given tag.
        /// </summary>
        /// <param name="carId">The id of the car that touched an object.</param>
        /// <param name="carPosition">The position of the car at the time of collision.</param>
        /// <param name="touchedObjectTag">String representation of the touched object's tag (null if the car is killed by the user).</param>
        /// <param name="reachedCheckpointsCount">An integer value indicating how many checkpoints has the car reached.</param>
        public CarDiedEventArgs(string carId, Vector3 carPosition, string touchedObjectTag, int reachedCheckpointsCount)
        {
            CarId = carId;
            CarPosition = carPosition;
            TouchedObjectTag = touchedObjectTag;
            ReachedCheckpointsCount = reachedCheckpointsCount;
        }

        /// <summary>
        /// Gets the id of the car that touched an object.
        /// </summary>
        public string CarId { get; private set; }

        /// <summary>
        /// Gets the position of the car at the time of collision.
        /// </summary>
        public Vector3 CarPosition { get; private set; }

        /// <summary>
        /// Gets the touched object's tag.
        /// If the car was killed by the user, then this value must be null.
        /// </summary>
        public string TouchedObjectTag { get; private set; }

        /// <summary>
        /// Gets the number of checkpoints the car reached.
        /// </summary>
        public int ReachedCheckpointsCount { get; private set; }
    }
}