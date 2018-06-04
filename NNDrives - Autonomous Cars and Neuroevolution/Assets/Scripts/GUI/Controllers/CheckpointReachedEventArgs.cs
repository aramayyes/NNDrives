using System;

namespace GUI.Controllers
{
    /// <summary>
    /// Used to pass a data to subscribers 
    /// when the <see cref="CarController.ReachedCheckpoint"/> event is raised.
    /// </summary>
    public class CheckpointReachedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckpointReachedEventArgs"/> class with given number of reached checkpoints.
        /// </summary>
        /// <param name="reachedCheckpointsCount">THe number of checkpoints the car has reached.</param>
        public CheckpointReachedEventArgs(int reachedCheckpointsCount)
        {
            ReachedCheckpointsCount = reachedCheckpointsCount;
        }

        /// <summary>
        /// Gets the number of checkpoints the car has reached so far.
        /// </summary>
        public int ReachedCheckpointsCount { get; private set; }
    }
}