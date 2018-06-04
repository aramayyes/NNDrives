using System;

namespace ArtificialIntelligence.GeneticAlgorithm
{
    /// <summary>
    /// Used to pass the best chromosome to subscribers when the <see cref="GeneticAlgorithmManager.AlgorithmFinished"/> 
    /// event is raised.
    /// </summary>
    public class AlgorithmFinishedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlgorithmFinishedEventArgs"/> class with given chromosome.
        /// </summary>
        /// <param name="bestChromosome">Best chromosome in the last generation.</param>
        public AlgorithmFinishedEventArgs(Chromosome bestChromosome)
        {
            BestChromosome = bestChromosome;
        }

        /// <summary>
        /// Gets the best fitness value of the population in the changing generation.
        /// </summary>
        public Chromosome BestChromosome
        {
            get;
            private set;
        }
    }
}