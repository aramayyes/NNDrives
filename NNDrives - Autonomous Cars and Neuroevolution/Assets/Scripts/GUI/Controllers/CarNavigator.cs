using Helpers;

using UnityEngine;

namespace GUI.Controllers
{
    /// <summary>
    /// Simulates a (map) navigator for a car.
    /// </summary>
    public class CarNavigator : MonoBehaviour
    {
        /// <summary>
        /// Value indicating whether the car is in a crossroad.
        /// </summary>
        private bool isInCrossroad;

        /// <summary>
        /// A point in the crossroad to which the car should move.
        /// </summary>
        private Vector3 crossroadPoint;

        /// <summary>
        /// Tells the navigator that the car entered a trigger collider attached to another object.
        /// </summary>
        /// <param name="collider">The other object's collider involved in this collision.</param>
        public void CarEnteredATrigger(Collider2D collider)
        {
            string colliderTag = collider.gameObject.tag;
            switch (colliderTag)
            {
                case Tags.CrossroadEnter:
                    isInCrossroad = true;
                    crossroadPoint = collider.gameObject.GetComponentInParent<CrossroadController>().PointPosition;
                    Debug.Log("Inside crossroad");
                    break;

                case Tags.CrossroadExit:
                    isInCrossroad = false;
                    Debug.Log("Outside crossroad");
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Gets data as a number containing information about car's position, direction and crossroads of the map.
        /// </summary>
        /// <param name="carPosition">A vector containing car's position.</param>
        /// <param name="carDirection">A normalized vector indicating the car's direction.</param>
        /// <returns>Number representing information about the car and the map.</returns>
        public float GetData(Vector3 carPosition, Vector3 carDirection)
        {
            if (!isInCrossroad)
            {
                return 0.01f;
            }

            return GetAngleToTarget(carPosition, carDirection);
        }

        /// <summary>
        /// Gets the angle between the car's direction and target vectors.
        /// </summary>
        /// <param name="carPosition">A vector containing car's position.</param>
        /// <param name="carDirection">A normalized vector indicating the car's direction.</param>
        /// <returns>The angle in degrees.</returns>
        private float GetAngleToTarget(Vector3 carPosition, Vector3 carDirection)
        {
            Vector3 directionVector = carDirection;
            Vector3 targetVector = (crossroadPoint - carPosition).normalized;

            float angle = Vector3.Angle(directionVector, targetVector);

            Vector3 crossProduct = Vector3.Cross(directionVector, targetVector);
            float z = crossProduct.z;

            return z >= 0 ? angle : -angle;
        }
    }
}