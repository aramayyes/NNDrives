using ArtificialIntelligence.ArtificialNeuralNetwork;

namespace ArtificialIntelligence.GeneticAlgorithm
{
    /// <summary>
    /// Contains a collection of gens as a <see cref="NeuralNetwork"/>.
    /// </summary>
    public class Chromosome
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Chromosome"/> class with given <paramref name="id"/> and <paramref name="network"/>.
        /// </summary>
        /// <param name="id">The new Chromosome's id.</param>
        /// <param name="network">The new Chromosome's neural network.</param>
        public Chromosome(string id, NeuralNetwork network)
        {
            Id = id;
            Network = network;
        }

        /// <summary>
        /// Gets the chromosome's id.
        /// </summary>
        public string Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the neural network of this chromosome.
        /// </summary>
        public NeuralNetwork Network
        {
            get;
            private set;
        }
    }
}