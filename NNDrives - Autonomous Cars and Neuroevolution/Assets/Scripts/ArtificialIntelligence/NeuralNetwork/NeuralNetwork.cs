using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArtificialIntelligence.ArtificialNeuralNetwork
{
    /// <summary>
    /// Represents an artificial neural network.
    /// </summary>
    public class NeuralNetwork
    {
        /// <summary>
        /// Holds all dense layers of this network.
        /// </summary>
        private DenseLayer[] layers;

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuralNetwork"/> class with given dense layers.
        /// </summary>
        /// <param name="layers">The new NeuralNetwork's layers.</param>
        public NeuralNetwork(IEnumerable<DenseLayer> layers)
        {
            this.layers = layers.ToArray();
        }

        /// <summary>
        /// Constructs a new instance of <see cref="NeuralNetwork"/> using topology and weights specified in 
        /// the <paramref name="networkInString"/> argument.
        /// </summary>
        /// <remarks>
        /// The given string must have 2 lines, the first containing layers' sizes
        /// separated by ',' sign, and the second containing weights also separated by ','.
        /// </remarks>
        /// <param name="networkInString">An instance of string containing the topology and weights of the network.
        /// </param>
        /// <returns>The new constructed neural network.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if input parameter <paramref name="networkInString"/> contains invalid network representation.
        /// </exception>
        public static NeuralNetwork ConstructFromString(string networkInString)
        {
            string[] lines = networkInString.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            if (lines.Length < 2)
            {
                throw new ArgumentException(string.Format("Invalid input parameter {0}. The string must contain 2 lines.", "networkInString"));
            }

            string[] sizes = lines[0].Split(',');
            if (sizes.Length < 4)
            {
                throw new ArgumentException(string.Format("Invalid input parameter {0}. The string must contain sizes for at least 2 layers.", "networkInString"));
            }

            string[] weightsInString = lines[1].Split(',');

            DenseLayer[] denseLayers = new DenseLayer[sizes.Length / 2];

            int k = 0;
            for (int c = 0; c < sizes.Length; c += 2)
            {
                int selfNeuronsCount;
                if (!int.TryParse(sizes[c], out selfNeuronsCount) || selfNeuronsCount < 1)
                {
                    throw new ArgumentException(string.Format("Invalid input parameter {0}. The neural layer must contain at least 1 neuron.", "networkInString"));
                }

                int outputNeuronsCount;
                if (!int.TryParse(sizes[c + 1], out outputNeuronsCount) || outputNeuronsCount < 1)
                {
                    throw new ArgumentException(string.Format("Invalid input parameter {0}. Output vector of neural layer must contain at least 1 neuron.", "networkInString"));
                }

                double[,] weights = new double[selfNeuronsCount + 1, outputNeuronsCount];
                for (int i = 0; i <= selfNeuronsCount; i++)
                {
                    for (int j = 0; j < outputNeuronsCount; j++)
                    {
                        double weight;
                        if (k >= weightsInString.Length || !double.TryParse(weightsInString[k++], out weight))
                        {
                            throw new ArgumentException(string.Format("Invalid input parameter {0}. Weights count must match to typology.", "networkInString"));
                        }

                        weights[i, j] = weight;
                    }
                }

                denseLayers[c / 2] = new DenseLayer(weights, LayerActivationFunctions.Sigmoid);
            }

            return new NeuralNetwork(denseLayers);
        }

        /// <summary>
        /// Gets the dense layers of this network.
        /// </summary>
        /// <returns>An array containing all dense layers of this network.</returns>
        public DenseLayer[] GetLayers()
        {
            return (DenseLayer[])layers.Clone();
        }

        /// <summary>
        /// Gets all weights of this network.
        /// </summary>
        /// <returns>The weights of all layers of this network.</returns>
        public IEnumerable<double> GetAllWeights()
        {
            foreach (DenseLayer layer in layers)
            {
                for (int i = 0; i <= layer.InputNeuronsCount; i++)
                {
                    for (int j = 0; j < layer.OutputNeuronsCount; j++)
                    {
                        yield return layer[i, j];
                    }
                }
            }
        }

        /// <summary>
        /// Processes the input data: applies all layers on it.
        /// </summary>
        /// <param name="inputs">Input tensor (in this case it's just a vector).</param>
        /// <returns>Output tensor (in this case it's just a vector).</returns>
        public double[] Process(double[] inputs)
        {
            double[] outputs = inputs;
            foreach (DenseLayer layer in layers)
            {
                outputs = layer.Apply(outputs);
            }

            return outputs;
        }

        /// <summary>
        /// Creates a deep copy of this network.
        /// </summary>
        /// <returns>A new instance of <see cref="NeuralNetwork"/> which has the same layers as this network.</returns>
        public NeuralNetwork Clone()
        {
            DenseLayer[] clonnedLayers = new DenseLayer[layers.Length];
            for (int k = 0; k < layers.Length; k++)
            {
                clonnedLayers[k] = layers[k].Clone();
            }

            return new NeuralNetwork(clonnedLayers);
        }

        /// <summary>
        /// Serializes this network to string.
        /// </summary>
        /// <returns>Instance of <see cref="String"/> containing this network's layers' sizes and weights.</returns>
        public string SerializeToString()
        {
            StringBuilder networkInTextBuilder = new StringBuilder();

            networkInTextBuilder.Append(layers[0].InputNeuronsCount);
            networkInTextBuilder.Append(",");
            networkInTextBuilder.Append(layers[0].OutputNeuronsCount);

            for (int i = 1; i < layers.Length; i++)
            {
                networkInTextBuilder.Append(",");
                networkInTextBuilder.Append(layers[i].InputNeuronsCount);
                networkInTextBuilder.Append(",");
                networkInTextBuilder.Append(layers[i].OutputNeuronsCount);
            }

            networkInTextBuilder.AppendLine();
            networkInTextBuilder.Append(string.Join(",", GetAllWeights().Select(w => w.ToString()).ToArray()));

            return networkInTextBuilder.ToString();
        }
    }
}