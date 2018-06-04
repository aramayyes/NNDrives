namespace Helpers
{
    /// <summary>
    /// Contains all tag values used in the project.
    /// </summary>
    public static class Tags
    {
        /// <summary>
        /// Holds the tag value of the objects that cause the car to die on collision.
        /// </summary>
        public const string Wall = "Wall";

        /// <summary>
        /// Holds the tag value of the cars.
        /// </summary>
        public const string Car = "Car";

        /// <summary>
        /// Holds the tag value of the checkpoints on the roads. 
        /// </summary>
        public const string Checkpoint = "Checkpoint";

        /// <summary>
        /// Holds the tag value of the objects notifying the car that it's inside a crossroad.
        /// </summary>
        public const string CrossroadEnter = "CrossIn";

        /// <summary>
        /// Holds the tag value of the objects that notifying the car that it's outside the crossroad.
        /// </summary>
        public const string CrossroadExit = "CrossOut";
    }
}
