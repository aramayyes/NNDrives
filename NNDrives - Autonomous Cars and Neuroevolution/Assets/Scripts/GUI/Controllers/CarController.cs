using System;
using System.Collections.Generic;

using ArtificialIntelligence.ArtificialNeuralNetwork;

using Helpers;

using UnityEngine;

namespace GUI.Controllers
{
    /// <summary>
    /// This class is responsible for car's movement, behavior and lifecycle.
    /// </summary>
    public class CarController : MonoBehaviour
    {
        /// <summary>
        /// Speed of the car.
        /// </summary>
        [SerializeField]
        private float speed = 10f;

        /// <summary>
        /// Speed of the car for turning left or right.
        /// </summary>
        [SerializeField]
        private float turnSpeed = 80f;

        /// <summary>
        /// Length of the lights emitted by laser sensors. 
        /// </summary>
        [SerializeField, Range(2, 5)]
        private float sensorLength = 4f;

        /// <summary>
        /// Value indicating whether the lights emitted from laser sensors must be shown.
        /// </summary>
        [SerializeField]
        private bool showSensors = true;

        /// <summary>
        /// Value indicating whether the car can move.
        /// </summary>
        private bool? isActive = null;

        /// <summary>
        /// Navigator for this car: gives an information about the map and crossroads.
        /// </summary>
        private CarNavigator navigator;

        /// <summary>
        /// Direction in which the car is moving.
        /// </summary>
        private Vector3 currentDirection;

        /// <summary>
        /// Array containing input values of the neural network.
        /// </summary>
        /// <remarks>
        /// Contains distances from the car to the nearest objects in each sensor direction and values got from the navigator.
        /// If the nearest object in a specified direction is far than the sensor length
        /// then the sensor length is taken as the sensor value.
        /// </remarks>
        private double[] valuesForNetwork;

        /// <summary>
        /// Contains all reached checkpoints' positions.
        /// </summary>
        private HashSet<Vector3> reachedCheckpoints = new HashSet<Vector3>();

        /// <summary>
        /// This event is raised when the car has crushed into another object and is no longer active.
        /// </summary>
        public event EventHandler<CarDiedEventArgs> Died = delegate { };

        /// <summary>
        /// This event is raised when the car reaches a checkpoint.
        /// </summary>
        public event EventHandler<CheckpointReachedEventArgs> ReachedCheckpoint = delegate { };

        /// <summary>
        /// Gets or sets the car's id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the car is driven by user or moves itself (using NN).
        /// </summary>
        public bool IsDrivenByUser { get; set; }

        /// <summary>
        /// Gets the number of all front sensors, which cover 90 degrees.
        /// </summary>
        public int SensorsCount { get; private set; }

        /// <summary>
        /// Gets the number of values in <see cref="valuesForNetwork"/> that are got from the navigator.
        /// </summary>
        public int NavigatorValuesCount { get; private set; }

        /// <summary>
        /// Gets the artificial neural network which drives this car.
        /// </summary>
        public NeuralNetwork Network { get; private set; }

        /// <summary>
        /// Gets the number of sensors, which are installed on the left side of the car's front part.   
        /// </summary>
        /// <remarks> 
        /// *Note: as many sensors are installed on the right side, and only one in the center.
        /// </remarks>
        private int LeftSideSensorsCount
        {
            get
            {
                return SensorsCount / 2;
            }
        }

        /// <summary>
        /// Makes this car active, so it can start the movement.
        /// </summary>
        /// <remarks>
        /// This operation is done only once. All the subsequent calls will be ignored.
        /// </remarks>
        /// <param name="sensorsCount">The number of all sensors.</param>
        /// <param name="navigatorValuesCount">The number of values that are got from the navigator.</param>
        /// <param name="network">The neural network that will drive this car.</param>
        public void MakeActive(int sensorsCount, int navigatorValuesCount, NeuralNetwork network)
        {
            if (!isActive.HasValue)
            {
                isActive = true;

                SensorsCount = sensorsCount;
                NavigatorValuesCount = navigatorValuesCount;
                Network = network;

                valuesForNetwork = new double[SensorsCount + NavigatorValuesCount];
                navigator = gameObject.AddComponent<CarNavigator>();
            }
        }

        /// <summary>
        /// Raises the <see cref="Died"/> event with given arguments.
        /// </summary>
        /// <param name="e">Contains the data which will be passed to the subscribers.</param>
        protected virtual void OnDied(CarDiedEventArgs e)
        {
            EventHandler<CarDiedEventArgs> handler = Died;
            handler(this, e);
        }

        /// <summary>
        /// Raises the <see cref="ReachedCheckpoint"/> event with given arguments.
        /// </summary>
        /// <param name="e">Contains the data which will be passed to the subscribers.</param>
        protected virtual void OnReachedCheckpoint(CheckpointReachedEventArgs e)
        {
            EventHandler<CheckpointReachedEventArgs> handler = ReachedCheckpoint;
            handler(this, e);
        }

        /// <summary>
        /// This function is called every fixed framerate frame.
        /// </summary>
        private void FixedUpdate()
        {
            if (isActive != true)
            {
                return;
            }

            float verticalInput;
            float horizontalInput;

            if (IsDrivenByUser)
            {
                GetCommandFromUser(out verticalInput, out horizontalInput);
                Drive(verticalInput, horizontalInput);
            }
            else if (Network != null)
            {
                GetCommandFromNetwork(out verticalInput, out horizontalInput);
                Drive(verticalInput, horizontalInput);
            }

            // Setup sensors, get the values from those sensors as well as from the navigator.
            SetUpSensors();
            GetValuesFromNavigator();

            // User can kill the car by pressing 'K' on the keyboard.
            if (Input.GetKeyDown(KeyCode.K))
            {
                isActive = false;
                OnDied(new CarDiedEventArgs(Id, transform.position, null, reachedCheckpoints.Count));
            }
        }

        /// <summary>
        /// This function is called when another collider makes contact with this object's collider.
        /// </summary>
        /// <param name="collision">An instance of <see cref="Collision2D"/> containing the data associated with this collision.</param>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            string collisionTag = collision.gameObject.tag;
            switch (collisionTag)
            {
                case Tags.Wall:
                    Debug.Log("Wall");
                    isActive = false;
                    OnDied(new CarDiedEventArgs(Id, transform.position, collisionTag, reachedCheckpoints.Count));
                    break;

                case Tags.Car:
                    // Ignore collisions beetween cars
                    Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.collider);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// This function is called when another object enters a trigger collider attached to this object.
        /// </summary>
        /// <param name="collider">The other object's collider involved in this collision.</param>
        private void OnTriggerEnter2D(Collider2D collider)
        {
            string colliderTag = collider.gameObject.tag;
            switch (colliderTag)
            {
                case Tags.Checkpoint:
                    if (reachedCheckpoints.Add(collider.gameObject.transform.position))
                    {
                        Debug.Log("Checkpoint");
                        ReachedCheckpoint(this, new CheckpointReachedEventArgs(reachedCheckpoints.Count));
                    }

                    break;

                case Tags.CrossroadEnter:
                case Tags.CrossroadExit:
                    navigator.CarEnteredATrigger(collider);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Constructs the frontside sensors and adds the values of those sensors to the <see cref="valuesForNetwork"/>. 
        /// Each sensor measures the distance between the car and the nearest object in the specified direction.
        /// </summary>
        private void SetUpSensors()
        {
            Vector3 centralSensorPos = transform.position;
            centralSensorPos += currentDirection * 0.55f;

            for (int i = -LeftSideSensorsCount; i <= LeftSideSensorsCount; i++)
            {
                // Sensor position and direction
                Vector3 sensorPos = centralSensorPos;
                Vector3 carFrontVector = (Quaternion.AngleAxis(90 * Math.Sign(i), new Vector3(0, 0, 1)) * currentDirection).normalized;
                sensorPos += carFrontVector * (0.2f / LeftSideSensorsCount) * Math.Abs(i);
                Vector3 direction = (Quaternion.AngleAxis(i * (90f / LeftSideSensorsCount), new Vector3(0, 0, 1)) * currentDirection).normalized;

                RaycastHit2D raycastHit = Physics2D.Raycast(sensorPos, direction, sensorLength, LayerMask.GetMask(Layers.Wall));
                if (raycastHit.collider != null)
                {
                    valuesForNetwork[LeftSideSensorsCount + i] = raycastHit.distance;
                    if (showSensors)
                    {
                        DrawLine(sensorPos, raycastHit.point, Color.red);
                    }
                }
                else
                {
                    valuesForNetwork[LeftSideSensorsCount + i] = sensorLength;
                    if (showSensors)
                    {
                        DrawLine(sensorPos, sensorPos + (direction * sensorLength), Color.red);
                    }
                }
            }
        }

        /// <summary>
        /// Gets value(s) from the navigator for the specified position and direction 
        /// and adds them to the <see cref="valuesForNetwork"/>.
        /// </summary>
        private void GetValuesFromNavigator()
        {
            float valueFromNavigator = navigator.GetData(transform.position, currentDirection);

            int length = valuesForNetwork.Length;
            for (int i = 1; i <= NavigatorValuesCount; i++)
            {
                valuesForNetwork[length - i] = valueFromNavigator;
            }
        }

        /// <summary>
        /// Gets the vertical and horizontal input values from the user.
        /// </summary>
        /// <param name="verticalInput">When this method returns contains a floating-point number 
        /// between -1 and 1 to move car up or down.
        /// </param>
        /// <param name="horizontalInput">When this method returns contains a floating-point number 
        /// between -1 and 1 to move car left or right.
        /// </param>
        private void GetCommandFromUser(out float verticalInput, out float horizontalInput)
        {
            // Vertical axis is used for acceleration
            verticalInput = Input.GetAxis("Vertical");

            // Horizontal axis is used for rotation
            horizontalInput = Input.GetAxis("Horizontal");
        }

        /// <summary>
        /// Gets the vertical and horizontal input values from the network.
        /// </summary>
        /// <param name="verticalInput">When this method returns contains a floating-point number 
        /// between 0 and 1 to move car up or down.
        /// </param>
        /// <param name="horizontalInput">When this method returns contains a floating-point number 
        /// between -1 and 1 to move car left or right.
        /// </param>
        private void GetCommandFromNetwork(out float verticalInput, out float horizontalInput)
        {
            double[] outputs = Network.Process(valuesForNetwork);

            /*
             * A. outputs[0]: a floating-point number between 0 and 1.
             * B. outputs[1]: a floating-point number between 0 and 1.
             * 
             * The 'A' simulates the vertical input, and the 'B' simulates the horizontal input.
             * In case of the horizontal input the car expects a floating-point number between -1 and 1, 
             *  positive ones for moving right and negative ones for moving left.
             * So for achieving this a mapping (function) between [0,1] and [-1,1] is needed.
             */

            float minOutputRangeValue = -1f;
            float outputRangeLength = 2f;

            // Horizontal axis is used for rotation
            horizontalInput = minOutputRangeValue + (outputRangeLength * (float)outputs[1]);

            // Vertical axis is used for acceleration
            // Cars can only move forward.
            verticalInput = (float)outputs[0];
        }

        /// <summary>
        /// Updates the car's position.
        /// </summary>
        /// <param name="verticalInput">A floating-point number between -1 and 1 indicating the car's acceleration.</param>
        /// <param name="horizontalInput">A floating-point number between -1 and 1 indicating the car's angular acceleration.</param>
        private void Drive(float verticalInput, float horizontalInput)
        {
            float velocity = verticalInput * speed * Time.deltaTime;
            if (velocity < 0.005f && !IsDrivenByUser)
            {
                velocity += 0.005f;
            }

            // Rotate the car around Z axis
            transform.rotation *= Quaternion.AngleAxis((-1) * horizontalInput * turnSpeed * Time.deltaTime, new Vector3(0, 0, 1));
            Vector3 direction = transform.rotation * (new Vector3(0, 1, 0));
            currentDirection = direction.normalized;

            transform.position += direction * velocity;
        }

        /// <summary>
        /// Draws a line between the specified start and end points.
        /// </summary>
        /// <param name="start">Point where the line should start.</param>
        /// <param name="end">Point where the line should end.</param>
        /// <param name="color">Color of the line.</param>
        /// <param name="duration">How long the line should be visible for.</param>
        private void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.02f)
        {
            LineRenderer sensorLine = new GameObject().AddComponent<LineRenderer>();
            sensorLine.transform.position = start;

            LineRenderer lineRenderer = sensorLine.GetComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.startWidth = 0.02f;
            lineRenderer.endWidth = 0.02f;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);

            Destroy(sensorLine.gameObject, duration);
        }
    }
}