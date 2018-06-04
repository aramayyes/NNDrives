using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ArtificialIntelligence.ArtificialNeuralNetwork;

using UnityEngine;

namespace ArtificialIntelligence.GeneticAlgorithm
{
    /// <summary>
    /// Represents a collection of <see cref="Chromosome"/> objects.
    /// </summary>
    public class Population : IEnumerable<Chromosome>
    {
        /// <summary>
        /// All chromosomes of the population.
        /// </summary>
        private List<Chromosome> chromosomes;

        /// <summary>
        /// Initializes a new instance of the <see cref="Population"/> class with the given chromosomes.
        /// </summary>
        /// <param name="chromosomes">The new Population's chromosomes in a list.</param>
        public Population(IEnumerable<Chromosome> chromosomes)
        {
            this.chromosomes = new List<Chromosome>(chromosomes);
        }

        /// <summary>
        /// Gets the number of chromosomes of the population.
        /// </summary>
        public int Count
        {
            get
            {
                return chromosomes.Count;
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Population"/> with random chromosomes.
        /// </summary>
        /// <param name="count">The number of chromosomes in the new Population.</param>
        /// <param name="inputsLength">The length of the input vector of neural network.</param>
        /// <param name="idGenerator">A function which takes the chromosome's order number in population
        /// and returns an id for that chromosome.</param>
        /// <returns>An instance of <see cref="Population"/> with <paramref name="count"/> random chromosomes.</returns>
        public static Population GenerateRandomPopulation(int count, int inputsLength, Func<int, string> idGenerator)
        {
            // HiddenLayer = (InputLayer + OutputLayer) / 2
            int hiddenLayerOutputsLength = (inputsLength + 2) / 2;

            var chromosomes = new List<Chromosome>();
            System.Random random = new System.Random();
            for (int i = 0; i < count; i++)
            {
                var layers = new DenseLayer[]
                {
                    new DenseLayer(inputsLength, hiddenLayerOutputsLength, LayerActivationFunctions.Sigmoid, random.NextDouble),
                    new DenseLayer(hiddenLayerOutputsLength, 2, LayerActivationFunctions.Sigmoid, random.NextDouble)
                };
                var network = new NeuralNetwork(layers);

                string chromosomeId = idGenerator != null ? idGenerator(i + 1) : (i + 1).ToString();
                var chromosome = new Chromosome(chromosomeId, network);

                chromosomes.Add(chromosome);
            }

            return new Population(chromosomes);
        }

        /// <summary>
        /// Gets the chromosomes of the population.
        /// </summary>
        /// <returns>A readonly collection containing all chromosomes in the population.</returns>
        public ReadOnlyCollection<Chromosome> GetChromosomes()
        {
            return chromosomes.AsReadOnly();
        }

        /// <summary>
        /// Sorts the chromosomes of the population by their fitness.
        /// </summary>
        /// <param name="fitnesses">A key-value collection containing the id and fitness for each chromosome of the population.</param>
        public void SortByFitness(IDictionary<string, double> fitnesses)
        {
            try
            {
                chromosomes.Sort((chromosome1, chromosome2) =>
                {
                    // TODO: fix the bug
                    double fitness1;
                    if (!fitnesses.TryGetValue(chromosome1.Id, out fitness1))
                    {
                        fitness1 = 0;
                    }

                    double fitness2;
                    if (!fitnesses.TryGetValue(chromosome2.Id, out fitness2))
                    {
                        fitness2 = 0;
                    }

                    return fitness2.CompareTo(fitness1);
                });
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
                throw;
            }
        }

        /// <summary>
        /// Implements the <see cref="IEnumerable{Chromosome}"/> interface.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="IEnumerator{Chromosome}"/> which allows 
        /// to iterate over the population's chromosomes.
        /// </returns>
        public IEnumerator<Chromosome> GetEnumerator()
        {
            return chromosomes.GetEnumerator();
        }

        /// <summary>
        /// Implements the <see cref="IEnumerable{Chromosome}"/> interface explicitly.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="IEnumerator{Chromosome}"/> which allows 
        /// to iterate over the population.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}