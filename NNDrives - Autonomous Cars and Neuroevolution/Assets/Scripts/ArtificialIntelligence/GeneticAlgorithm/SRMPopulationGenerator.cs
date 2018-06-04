using System;
using System.Collections.Generic;
using System.Linq;

using ArtificialIntelligence.ArtificialNeuralNetwork;

using Helpers;

namespace ArtificialIntelligence.GeneticAlgorithm
{
    /// <summary>
    /// Generates a new population from the given one using 3 genetic operators, in order mentioned below:
    /// 1. Selection            
    ///     The best chromosomes are selected for creating new ones and the worst chromosomes are removed,
    /// 2. Recombination        
    ///     Chromosomes are recombinated using crossover operation,
    /// 3. Mutation             
    ///     All new chromosomes got after recombination are mutated with mutation operator.
    /// </summary>
    public class SRMPopulationGenerator : IPopulationGenerator
    {
        /// <summary>
        /// A pseudo-random number generator.
        /// </summary>
        private Random random = new Random();

        /// <summary>
        /// Length of the input vectors of neural networks used in chromosomes.
        /// </summary>
        private int networkInputsLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="SRMPopulationGenerator"/> class, which generated chromosomes' 
        /// networks have input vectors of length <paramref name="networkInputsLength"/>.
        /// </summary>
        /// <param name="networkInputsLength">The length of the input layer of a network used in a chromosome.</param>
        public SRMPopulationGenerator(int networkInputsLength)
        {
            this.networkInputsLength = networkInputsLength;
        }

        /// <inheritdoc />
        /// <see cref="IPopulationGenerator"/>
        /// <returns>An instance of <see cref="Population"/> with random chromosomes.</returns>
        public Population GenerateFirstPopulation(int size)
        {
            return Population.GenerateRandomPopulation(
                size,
                networkInputsLength,
                (orderInPopulation) => string.Format("{0}_{1}", 1, orderInPopulation));
        }

        /// <inheritdoc />
        /// <see cref="IPopulationGenerator"/>
        /// <returns>An instance of <see cref="Population"/> containing the given chromosome 
        /// and other chromosomes got from that one by mutation.</returns>
        public Population GenerateFirstPopulation(int size, string chromosomeBrain)
        {
            NeuralNetwork firstNetwork = NeuralNetwork.ConstructFromString(chromosomeBrain);

            var newChromosomes = new List<Chromosome>(size);
            int id = 1;
            newChromosomes.Add(new Chromosome(string.Format("{0}_{1}", 1, id++), firstNetwork));

            for (int i = 1; i < size; i++)
            {
                newChromosomes.Add(new Chromosome(string.Format("{0}_{1}", 1, id++), GaussianMutation(firstNetwork, 0.3f, 1)));
            }

            return new Population(newChromosomes);
        }

        /// <inheritdoc />
        /// <see cref="IPopulationGenerator"/>
        /// <returns>An instance of <see cref="Population"/> containing new chromosomes got after genetic operators.</returns>
        public Population GeneratePopulation(Population currentPopulation, int generation)
        {
            if (currentPopulation == null)
            {
                throw new ArgumentNullException("currentPopulation");
            }

            if (currentPopulation.Count < 2)
            {
                throw new ArgumentException("The given population must contain at least 2 chromosomes.", "currentPopulation");
            }

            // Get networks of the chromosomes from the given population.
            List<NeuralNetwork> currentNetworks = currentPopulation.GetChromosomes().Select(chromosome => chromosome.Network).ToList();

            // Get the best cromosomes for creating new ones and get rid of worst networks.
            List<NeuralNetwork> selectedNetworks = MakeSelection(currentNetworks);

            // Make new networks from selected ones using crossover operator.
            List<NeuralNetwork> newNetworks = DoRecombination(selectedNetworks, currentPopulation.Count);

            // Mutate all networks got after crossover.
            List<NeuralNetwork> mutatedNetworks = DoMutation(newNetworks);

            // Construct new chromosomes.
            List<Chromosome> newChromosomes = new List<Chromosome>();
            int id = 1;
            foreach (NeuralNetwork network in mutatedNetworks)
            {
                newChromosomes.Add(new Chromosome(string.Format("{0}_{1}", generation, id++), network));
            }

            // Construct and return a new population with constructed chromosomes.
            return new Population(newChromosomes);
        }

        /// <summary>
        /// Selects(chooses) the best networks from the given ones.
        /// </summary>
        /// <param name="currentNetworks">A collection containing the networks of the chromosomes.</param>
        /// <param name="selectionPercentage">What percentage of networks must be selected.</param>
        /// <returns>A collection containing networks survived after selection.</returns>
        private List<NeuralNetwork> MakeSelection(List<NeuralNetwork> currentNetworks, int selectionPercentage = 10)
        {
            if (selectionPercentage > 100)
            {
                throw new ArgumentException("The percentage value must be lower than 100", "selectionPercentage");
            }

            int percentage = selectionPercentage;
            int count = (currentNetworks.Count * percentage) / 100;
            if (count < 2)
            {
                count = 2; // At least 2 networks must be selected
            }

            return currentNetworks.GetRange(0, count);
        }

        /// <summary>
        /// Constructs new networks from the given ones using crossover operation.
        /// </summary>
        /// <param name="selectedNetworks">Networks got from selection operator.</param>
        /// <param name="populationSize">How many networks to construct.</param>
        /// <returns>A collection containing new networks got with crossover operation.</returns>
        private List<NeuralNetwork> DoRecombination(List<NeuralNetwork> selectedNetworks, int populationSize)
        {
            // Construct new brains (networks) for the new population.
            var newNetworks = new List<NeuralNetwork>(populationSize);

            int firstParentIndex = 0;
            int secondParentIndex = 1;
            while (newNetworks.Count < populationSize)
            {
                /*
                 * If there is no more networks left for crossover and the new population isn't full yet, 
                 * then restart the networks creating algorithm.
                 */
                if (firstParentIndex >= selectedNetworks.Count - 1)
                {
                    firstParentIndex = 0;
                    secondParentIndex = 1;
                }
                else
                {
                    if (secondParentIndex < selectedNetworks.Count)
                    {
                        NeuralNetwork newNetwork1;
                        NeuralNetwork newNetwork2;
                        UniformCrossover(selectedNetworks[firstParentIndex], selectedNetworks[secondParentIndex], 0.7f, out newNetwork1, out newNetwork2);

                        newNetworks.Add(newNetwork1);
                        if (newNetworks.Count < populationSize)
                        {
                            newNetworks.Add(newNetwork2);
                        }

                        secondParentIndex++;
                    }
                    else
                    {
                        firstParentIndex++;
                        secondParentIndex = firstParentIndex + 1;
                    }
                }
            }

            return newNetworks;
        }

        /// <summary>
        /// Constructs a new network using a fixed mixing ration between two parents.
        /// </summary>
        /// <param name="parent1">The fist parent network.</param>
        /// <param name="parent2">The second parent network.</param>
        /// <param name="mixingRatio">The probability with which the first(second) child will have genes from the first(second) parent.</param>
        /// <param name="ch1">When this method returns contains the first child network.</param>
        /// <param name="ch2">When this method returns contains the second child network.</param>
        private void UniformCrossover(NeuralNetwork parent1, NeuralNetwork parent2, float mixingRatio, out NeuralNetwork ch1, out NeuralNetwork ch2)
        {
            DenseLayer[] parent1Layers = parent1.GetLayers();
            DenseLayer[] parent2Layers = parent2.GetLayers();

            var child1Layers = new DenseLayer[parent1Layers.Length];
            var child2Layers = new DenseLayer[parent1Layers.Length];

            for (int k = 0; k < child1Layers.Length; k++)
            {
                DenseLayer parent1Layer = parent1Layers[k];
                DenseLayer parent2Layer = parent2Layers[k];

                var child1Weights = new double[parent1Layer.InputNeuronsCount + 1, parent1Layer.OutputNeuronsCount];
                var child2Weights = new double[parent1Layer.InputNeuronsCount + 1, parent1Layer.OutputNeuronsCount];

                for (int i = 0; i < child1Weights.GetLength(0); i++)
                {
                    for (int j = 0; j < child1Weights.GetLength(1); j++)
                    {
                        if (random.NextDouble() < mixingRatio)
                        {
                            child1Weights[i, j] = parent1Layer[i, j];
                            child2Weights[i, j] = parent2Layer[i, j];
                        }
                        else
                        {
                            child1Weights[i, j] = parent2Layer[i, j];
                            child2Weights[i, j] = parent1Layer[i, j];
                        }
                    }
                }

                child1Layers[k] = new DenseLayer(child1Weights, parent1Layer.GetActivationFunction());
                child2Layers[k] = new DenseLayer(child2Weights, parent1Layer.GetActivationFunction());
            }

            ch1 = new NeuralNetwork(child1Layers);
            ch2 = new NeuralNetwork(child2Layers);
        }

        /// <summary>
        /// Creates new networks by mutating the given ones.
        /// </summary>
        /// <param name="networks">Networks to be mutated.</param>
        /// <returns>A new collection containing the mutated networks.</returns>
        private List<NeuralNetwork> DoMutation(List<NeuralNetwork> networks)
        {
            // Keep the best network
            var mutatedNetworks = new List<NeuralNetwork>(networks.Count)
            {
                networks[0].Clone(),
            };

            for (int i = 1; i < networks.Count; i++)
            {
                NeuralNetwork mutatedNetwork = GaussianMutation(networks[i], 0.3f, 1);
                mutatedNetworks.Add(mutatedNetwork);
            }

            return mutatedNetworks;
        }

        /// <summary>
        /// Adds a random number taken from a Gaussian distribution to all parameters of the given network 
        /// with <paramref name="mutationProbability"/> probability.
        /// </summary>
        /// <param name="network">Network to be mutated.</param>
        /// <param name="mutationProbability">Probability for gene mutation.</param>
        /// <param name="variance">Variance for the Gaussian distribution.</param>
        /// <returns>The mutated network.</returns>
        private NeuralNetwork GaussianMutation(NeuralNetwork network, float mutationProbability, float variance)
        {
            DenseLayer[] layers = network.GetLayers();
            var newLayers = new DenseLayer[layers.Length];
            for (int k = 0; k < newLayers.Length; k++)
            {
                DenseLayer layer = layers[k];
                var childWeights = new double[layers[k].InputNeuronsCount + 1, layers[k].OutputNeuronsCount];

                for (int i = 0; i < childWeights.GetLength(0); i++)
                {
                    for (int j = 0; j < childWeights.GetLength(1); j++)
                    {
                        if (random.NextDouble() < mutationProbability)
                        {
                            childWeights[i, j] = layer[i, j] + random.NextGaussian(0, variance);
                        }
                        else
                        {
                            childWeights[i, j] = layer[i, j];
                        }
                    }
                }

                newLayers[k] = new DenseLayer(childWeights, layers[k].GetActivationFunction());
            }

            return new NeuralNetwork(newLayers);
        }
    }
}