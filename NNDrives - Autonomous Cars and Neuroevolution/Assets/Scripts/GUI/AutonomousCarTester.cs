using System;

using ArtificialIntelligence.ArtificialNeuralNetwork;

using GUI.Controllers;

using UnityEngine;

namespace GUI
{
    /// <summary>
    /// This class is used to construct an autonomous car and test it on the road.
    /// </summary>
    public class AutonomousCarTester : MonoBehaviour
    {
        /// <summary>
        /// Text which contains the string representation of an instance of <see cref="NeuralNetwork"/>.
        /// </summary>
        [SerializeField]
        private TextAsset trainedNetwork = null;

        /// <summary>
        /// A prefab which is used to instantiate the car.
        /// </summary>
        [SerializeField]
        private GameObject carPrefab = null;

        /// <summary>
        /// The camera which will follow the lead car.
        /// </summary>
        [SerializeField]
        private Camera cameraForLeadObject = null;

        /// <summary>
        /// Value indicating whether the car is user driven.
        /// </summary>
        [SerializeField]
        private bool isDrivenByUser = false;

        /// <summary>
        /// Used for initialization.
        /// </summary>
        private void Start()
        {
            // The first line of the file contains number of values the car gets from navigator.
            // The rest is the network's representation.
            string text = this.trainedNetwork.text;
            string[] firstLineAndRest = text.Split(new[] { "\r\n", "\r", "\n" }, 2, StringSplitOptions.None);
            string firstLine = firstLineAndRest[0];

            int navigatorValuesCount;
            if (!int.TryParse(firstLine, out navigatorValuesCount))
            {
                navigatorValuesCount = 0;
            }

            string trainedNetwork = firstLineAndRest[1];

            NeuralNetwork network = NeuralNetwork.ConstructFromString(trainedNetwork);

            Vector3 position = carPrefab.transform.position;
            GameObject car = Instantiate(carPrefab, position, Quaternion.identity);
            car.name = "Car_testee";

            CarController carContoller = car.GetComponent<CarController>();
            carContoller.transform.rotation = carPrefab.transform.rotation;
            carContoller.Id = "Test_Chromosome";
            carContoller.MakeActive(network.GetLayers()[0].InputNeuronsCount - navigatorValuesCount, navigatorValuesCount, network);

            if (isDrivenByUser)
            {
                carContoller.IsDrivenByUser = true;
            }

            cameraForLeadObject.GetComponent<CameraController>().LeadObject = car;
        }
    }
}