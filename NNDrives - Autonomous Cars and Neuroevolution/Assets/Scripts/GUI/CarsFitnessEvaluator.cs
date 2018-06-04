using System;
using System.Collections.Generic;
using System.Linq;

using ArtificialIntelligence.GeneticAlgorithm;

using GUI.Controllers;

using RacingEnvironment;

using UnityEngine;

namespace GUI
{
    /// <summary>
    /// Represents a population fitness evaluator using cars that have to move themselves.
    /// </summary>
    public class CarsFitnessEvaluator : MonoBehaviour, IFitnessEvaluator
    {
        /// <summary>
        /// Contains all checkpoint objects.
        /// </summary>
        [SerializeField]
        private List<GameObject> checkpoints = new List<GameObject>();

        /// <summary>
        /// Contains all checkpoints for the evaluation grouped by roads.
        /// </summary>
        [SerializeField]
        private List<CheckpointsContainer> checkpointContainers = new List<CheckpointsContainer>();

        /// <summary>
        /// Contains all car objects.
        /// </summary>
        private List<GameObject> cars = new List<GameObject>();

        /// <summary>
        /// Key-value collection containing the cars ids and fitness values.
        /// </summary>
        private Dictionary<string, double> fitnessValues = new Dictionary<string, double>();

        /// <summary>
        /// Id of the car which has reached the most checkpoints and is alive.
        /// </summary>
        private string bestCarId = string.Empty;

        /// <summary>
        /// Number of the checkpoints the best alive car has reached so far.
        /// </summary>
        private int bestCarCheckpointsCount = 0;

        /// <inheritdoc/>
        /// <see cref="IFitnessEvaluator.EvaluationFinished"/>
        public event EventHandler<FitnessEvaluationEventArgs> EvaluationFinished = delegate { };

        /// <summary>
        /// Gets number of all checkpoints.
        /// </summary>
        public int CheckpointsCount
        {
            get
            {
                return checkpoints.Count;
            }
        }

        /// <summary>
        /// Gets or sets the car prefab, which is used to create a new car object.
        /// </summary>
        public GameObject CarPrefab { get; set; }

        /// <summary>
        /// Gets or sets the camera which will follow the leader car.
        /// </summary>
        public Camera Camera { get; set; }

        /// <summary>
        /// Gets or sets the number of the sensors of a car used in the evaluation.
        /// </summary>
        public int CarSensorsCount { get; set; }

        /// <summary>
        /// Gets or sets number of values that a car used in this evaluation will get from its navigator.
        /// </summary>
        public int CarNavigatorValuesCount { get; set; }

        /// <inheritdoc />
        /// <see cref="IFitnessEvaluator.EvaluationFinished"/>
        public void StartEvaluation(Population population)
        {
            if (population == null || !population.Any())
            {
                throw new ArgumentException("The given population doesn't contain any chromosomes.", "population");
            }

            int i = 0;
            fitnessValues.Clear();
            bool isFirstPopulation = cars.Count == 0;

            foreach (var chromosome in population)
            {
                Vector3 position = CarPrefab.transform.position;

                GameObject car = Instantiate(CarPrefab, position, Quaternion.identity);
                car.name = "Car -- " + chromosome.Id.ToString();
                CarController carContoller = car.GetComponent<CarController>();

                carContoller.transform.rotation = CarPrefab.transform.rotation;
                carContoller.Id = chromosome.Id;

                carContoller.ReachedCheckpoint += CarContoller_ReachedCheckpoint;
                carContoller.Died += CarContoller_Died;

                carContoller.MakeActive(CarSensorsCount, CarNavigatorValuesCount, chromosome.Network);

                if (isFirstPopulation)
                {
                    cars.Add(car);
                }
                else
                {
                    Destroy(cars[i]);
                    cars[i++] = car;
                }
            }

            bestCarId = string.Empty;
            bestCarCheckpointsCount = 0;
            Camera.GetComponent<CameraController>().LeadObject = cars[0];
        }

        /// <summary>
        /// Raises the <see cref="EvaluationFinished"/> event with given arguments.
        /// </summary>
        /// <param name="e">Contains the data which will be passed to the subscribers.</param>
        protected virtual void OnEvaluationFinished(FitnessEvaluationEventArgs e)
        {
            EventHandler<FitnessEvaluationEventArgs> handler = EvaluationFinished;
            handler(this, e);
        }

        /// <summary>
        /// Used for initialization.
        /// </summary>
        private void Start()
        {
            if (checkpoints == null || !checkpoints.Any())
            {
                checkpoints = new List<GameObject>();
                foreach (var container in checkpointContainers)
                {
                    checkpoints.AddRange(container.GetCheckpoints());
                }
            }
        }

        /// <summary>
        /// This method is executed when one of the <see cref="cars"/> reaches a checkpoint.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">Object containing number of reached checkpoints.</param>
        private void CarContoller_ReachedCheckpoint(object sender, CheckpointReachedEventArgs e)
        {
            var carController = sender as CarController;
            if (carController != null)
            {
                if (e.ReachedCheckpointsCount > bestCarCheckpointsCount)
                {
                    bestCarCheckpointsCount = e.ReachedCheckpointsCount;
                    bestCarId = carController.Id;

                    Camera.GetComponent<CameraController>().LeadObject = carController.gameObject;
                }
            }
        }

        /// <summary>
        /// This method is executed when one of the <see cref="cars"/> dies, i.e. touches an object(wall) or is killed by user.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">Object containing the car id and the touched object's tag.</param>
        private void CarContoller_Died(object sender, CarDiedEventArgs e)
        {
            if (bestCarId == e.CarId)
            {
                bestCarCheckpointsCount = 0;
            }

            float distanceToPrevCheckpoint;
            if (e.ReachedCheckpointsCount > 0)
            {
                distanceToPrevCheckpoint = Vector3.Distance(checkpoints[e.ReachedCheckpointsCount - 1].transform.position, e.CarPosition);
            }
            else
            {
                distanceToPrevCheckpoint = Vector3.Distance(CarPrefab.transform.position, e.CarPosition);
            }

            float distanceToNextCheckpoint = e.ReachedCheckpointsCount < checkpoints.Count
                ? Vector3.Distance(checkpoints[e.ReachedCheckpointsCount].transform.position, e.CarPosition)
                : 0;

            float fitness = e.ReachedCheckpointsCount + (distanceToPrevCheckpoint / (distanceToPrevCheckpoint + distanceToNextCheckpoint));

            fitnessValues[e.CarId] = fitness;

            if (fitnessValues.Count == cars.Count)
            {
                OnEvaluationFinished(new FitnessEvaluationEventArgs(fitnessValues));
            }
        }
    }
}