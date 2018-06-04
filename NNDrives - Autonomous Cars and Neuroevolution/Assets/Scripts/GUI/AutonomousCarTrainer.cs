using System;
using System.IO;

using ArtificialIntelligence.ArtificialNeuralNetwork;
using ArtificialIntelligence.GeneticAlgorithm;

using UnityEditor;

using UnityEngine;
using UnityEngine.UI;

namespace GUI
{
    /// <summary>
    /// This class is used to train the autonomous cars using genetic algorithm.
    /// </summary>
    [RequireComponent(typeof(CarsFitnessEvaluator))]
    public class AutonomousCarTrainer : MonoBehaviour
    {
        /// <summary>
        /// A prefab which is used to instantiate the cars.
        /// </summary>
        [SerializeField]
        private GameObject carPrefab = null;

        /// <summary>
        /// The main camera.
        /// </summary>
        [SerializeField]
        private Camera mainCamera = null;

        /// <summary>
        /// A text displaying the current generation's number.
        /// </summary>
        [SerializeField]
        private Text generationText = null;

        /// <summary>
        /// Number of the cars to be used in each iteration of the genetic algorithm.
        /// </summary>
        [Header("Algorithm props")]
        [SerializeField]
        private uint carsPerIteration = 3;

        /// <summary>
        /// Number of the sensors of the cars.
        /// </summary>
        [SerializeField]
        [Range(5, 35)]
        private int carSensorsCount = 25;

        /// <summary>
        /// Number of checkpoints, that cars must pass to finish algorithm.
        /// </summary>
        [SerializeField, HideInInspector]
        private int checkpointsCount;

        /// <summary>
        /// Contains string representation of a trained network.
        /// </summary>
        [SerializeField]
        private TextAsset savedNetwork = null;

        /// <summary>
        /// Number of values cars will get from theirs navigators.
        /// </summary>
        private int carNavigatorValuesCount = 3;

        /// <summary>
        /// Used for initialization.
        /// </summary>
        private void Start()
        {
            int carSensorsCount = this.carSensorsCount;
            if (carSensorsCount % 2 == 0)
            {
                carSensorsCount += 1;
            }

            string initialChromosomeBrain = null;
            int navigatorValuesCount = carNavigatorValuesCount;
            if (savedNetwork != null)
            {
                // First line of the file contains number of values the car gets from navigator.
                // The rest is the networksrepresentation.
                string text = savedNetwork.text;
                string[] firstLineAndRest = text.Split(new[] { "\r\n", "\r", "\n" }, 2, StringSplitOptions.None);
                string firstLine = firstLineAndRest[0];

                if (!int.TryParse(firstLine, out navigatorValuesCount))
                {
                    navigatorValuesCount = 0;
                }

                initialChromosomeBrain = firstLineAndRest[1];
            }

            CarsFitnessEvaluator fitnessEvaluator = GetComponent<CarsFitnessEvaluator>();
            fitnessEvaluator.CarPrefab = carPrefab;
            fitnessEvaluator.Camera = mainCamera;
            fitnessEvaluator.CarSensorsCount = carSensorsCount;
            fitnessEvaluator.CarNavigatorValuesCount = navigatorValuesCount;

            checkpointsCount = fitnessEvaluator.CheckpointsCount - 1;

            GeneticAlgorithmManager geneticAlgorithm = new GeneticAlgorithmManager(fitnessEvaluator, new SRMPopulationGenerator(carSensorsCount + carNavigatorValuesCount), (int)carsPerIteration, initialChromosomeBrain);
            geneticAlgorithm.GenerationChanging += GeneticAlgorithm_GenerationChanging;
            geneticAlgorithm.AlgorithmFinished += GeneticAlgorithm_AlgorithmFinished;

            if (!geneticAlgorithm.Start())
            {
                Debug.LogError("Algorithm is already started.");
            }

            SetGenerationText(1);
        }

        /// <summary>
        /// This function is executed when the generation in <see cref="GeneticAlgorithmManager"/> is going to be changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">Object containing information about changing generation.</param>
        private void GeneticAlgorithm_GenerationChanging(object sender, GenerationEventArgs e)
        {
            // Termination.
            if (e.BestFitnessValue >= checkpointsCount)
            {
                e.SetTheLast();
            }

            if (!e.IsTheLast)
            {
                SetGenerationText(e.Generation + 1);
            }
        }

        /// <summary>
        /// This function is executed when the algorithm in <see cref="GeneticAlgorithmManager"/> is finished.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">Object containing the network trained by the algorithm.</param>
        private void GeneticAlgorithm_AlgorithmFinished(object sender, AlgorithmFinishedEventArgs e)
        {
            SaveNeuralNetworkToFile(e.BestChromosome);
            Debug.Log("Algorithm is finished.");
        }

        /// <summary>
        /// Constructs the generation text.
        /// </summary>
        /// <param name="generation">The current generation number to be shown.</param>
        private void SetGenerationText(int generation)
        {
            generationText.text = string.Format("Generation: {0}", generation);
        }

        /// <summary>
        /// Saves the neural network of the given chromosome to a txt file.
        /// </summary>
        /// <param name="chromosome">Chromosome which neural network should be saved to a file.</param>
        private void SaveNeuralNetworkToFile(Chromosome chromosome)
        {
            string path = string.Format("Assets/Resources/TrainedNetworks/ch___{0}.txt", chromosome.Id);

            NeuralNetwork network = chromosome.Network;
            string serializedNetwork = network.SerializeToString();

            // Save the serialized network to a file specified above
            using (StreamWriter writer = new StreamWriter(File.Open(path, FileMode.Create, FileAccess.ReadWrite)))
            {
                writer.WriteLine(carNavigatorValuesCount.ToString()); // Also save the number of values cars get from the navigators
                writer.WriteLine(serializedNetwork);
            }

            AssetDatabase.ImportAsset(path);
        }
    }
}