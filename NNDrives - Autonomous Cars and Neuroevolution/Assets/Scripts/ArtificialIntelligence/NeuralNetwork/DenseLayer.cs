using System;

namespace ArtificialIntelligence.ArtificialNeuralNetwork
{
    /// <summary>
    /// Represents a fully connected layer.
    /// Each neuron of this layer is connected to every other neuron in the next layer.
    /// </summary>
    public class DenseLayer
    {
        /// <summary>
        /// Activation function of the layer.
        /// By default this function is the "Identity function".
        /// </summary>
        private readonly Func<double, double> activationFunction;

        /// <summary>
        /// Matrix containing all weights and biases between this and the previous layer.
        /// </summary>
        private double[,] weights;

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseLayer"/> class with <paramref name="inputNeuronsCount"/> neurons,
        /// with output vector of length <paramref name="outputNeuronsCount"/> and <paramref name="activationFunction"/>.
        /// </summary>
        /// <param name="inputNeuronsCount">The new DenseLayer's input neurons count.</param>
        /// <param name="outputNeuronsCount">The new DenseLayer's output neurons count.</param>
        /// <param name="activationFunction">The new DenseLayer's activation function.</param>
        /// <param name="randomInitializer">Function for generating random numbers between 0 and 1.</param>
        public DenseLayer(int inputNeuronsCount, int outputNeuronsCount, Func<double, double> activationFunction = null, Func<double> randomInitializer = null)
        {
            InputNeuronsCount = inputNeuronsCount;
            OutputNeuronsCount = outputNeuronsCount;
            this.activationFunction = activationFunction ?? (x => x);

            /*
             * Matrix containing all weights between this and the next layer:
             *  the last row in this matrix is for biases.
             */
            weights = new double[InputNeuronsCount + 1, OutputNeuronsCount];

            if (randomInitializer != null)
            {
                InitializeWeightsRandomly(randomInitializer);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseLayer"/> class with given matrix of <paramref name="weights"/>,
        /// and <paramref name="activationFunction"/>.
        /// </summary>
        /// <param name="weights">Matrix containing all weights and biases between the previous and this layers.</param>
        /// <param name="activationFunction">The new DenseLayer's activation function.</param>
        public DenseLayer(double[,] weights, Func<double, double> activationFunction = null)
        {
            int length0 = weights.GetLength(0);
            int length1 = weights.GetLength(1);
            if (weights == null || length0 < 2 || length1 < 1)
            {
                throw new ArgumentException("The given array must contain at least 2 values.", "weights");
            }

            InputNeuronsCount = length0 - 1;
            OutputNeuronsCount = length1;
            this.activationFunction = activationFunction ?? (x => x);

            this.weights = weights;
        }

        /// <summary>
        /// Gets the number of input neurons of this layer.
        /// </summary>
        public int InputNeuronsCount { get; private set; }

        /// <summary>
        /// Gets the number of output neurons of this layer.
        /// </summary>
        public int OutputNeuronsCount { get; private set; }

        /// <summary>
        /// Gets the connection weight's value between the previous layer's neuron at index <see cref="i"/>
        /// and this layer's neuron at index <see cref="j"/>.
        /// </summary>
        /// <param name="i">A number specifying previous layer's neuron's index.</param>
        /// <param name="j">A number specifying this layer's neuron's index.</param>
        /// <returns>A double-precision floating-point value of the weight.</returns>
        public double this[int i, int j]
        {
            get
            {
                return weights[i, j];
            }
        }

        /// <summary>
        /// Applies this layer on the input.
        /// </summary>
        /// <param name="inputs">Input tensor (in this case it's just a vector).</param>
        /// <returns>Output tensor (in this case it's just a vector).</returns>
        public double[] Apply(double[] inputs)
        {
            if (inputs == null)
            {
                throw new ArgumentNullException("inputs");
            }

            if (inputs.Length != InputNeuronsCount)
            {
                throw new ArgumentException(string.Format("Given array must contain {0} elements.", InputNeuronsCount.ToString()), "inputs");
            }

            var outputs = new double[OutputNeuronsCount];

            for (int col = 0; col < OutputNeuronsCount; col++)
            {
                for (int row = 0; row < InputNeuronsCount; row++)
                {
                    outputs[col] += inputs[row] * weights[row, col];
                }

                outputs[col] += weights[InputNeuronsCount, col]; // Also add the bias
                outputs[col] = activationFunction(outputs[col]);
            }

            return outputs;
        }

        /// <summary>
        /// Creates a deep copy of this layer.
        /// </summary>
        /// <returns>A new instance of <see cref="DenseLayer"/> which has the same weights and activation function as this layer.</returns>
        public DenseLayer Clone()
        {
            var clonnedWeights = weights.Clone() as double[,];
            return new DenseLayer(clonnedWeights, activationFunction);
        }

        /// <summary>
        /// Gets the activation function of the layer.
        /// </summary>
        /// <returns>The activation function of this layer.</returns>
        public Func<double, double> GetActivationFunction()
        {
            return (Func<double, double>)activationFunction.Clone();
        }

        /// <summary>
        /// Initializes the tensor (matrix) of weights with random values.
        /// </summary>
        /// <param name="randomInitializer">A function which returns random values between 0 and 1.</param>
        private void InitializeWeightsRandomly(Func<double> randomInitializer)
        {   
            int minValue = -1;

            for (int i = 0; i <= InputNeuronsCount; ++i)
            {
                for (int j = 0; j < OutputNeuronsCount; ++j)
                {
                    weights[i, j] = minValue + (randomInitializer() * (-2 * minValue));
                }
            }
        }
    }
}