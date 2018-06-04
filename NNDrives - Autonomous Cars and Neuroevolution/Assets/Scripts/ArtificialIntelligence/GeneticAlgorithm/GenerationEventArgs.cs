using System;

namespace ArtificialIntelligence.GeneticAlgorithm
{
    /// <summary>
    /// Used to pass an information about the changing generation to subscribers 
    /// when the the <see cref="GeneticAlgorithmManager.GenerationChanging"/> event is raised.
    /// </summary>
    public class GenerationEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenerationEventArgs"/> class with given fitness value and generation number.
        /// </summary>
        /// <param name="generation">The number of changing generation.</param>
        /// <param name="bestFitnessValue">Best fitness value of the population of the changing generation.</param>
        /// <param name="sumFitnessValue">The sum of all fitness values of the population in the changing generation.</param>
        public GenerationEventArgs(int generation, double bestFitnessValue, double sumFitnessValue)
        {
            Generation = generation;
            BestFitnessValue = bestFitnessValue;
            SumFitnessValue = sumFitnessValue;
        }

        /// <summary>
        /// Gets the 1-based number of the generation.
        /// </summary>
        public int Generation { get; private set; }

        /// <summary>
        /// Gets the best fitness value of the population of the changing generation.
        /// </summary>
        public double BestFitnessValue { get; private set; }

        /// <summary>
        /// Gets the sum of all fitness values of the population of the changing generation.
        /// </summary>
        public double SumFitnessValue { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this is the last generation or should the generational process be repeated.
        /// </summary>
        public bool IsTheLast { get; private set; }

        /// <summary>
        /// Sets a bit indicating that this changing generation is the last.
        /// </summary>
        public void SetTheLast()
        {
            IsTheLast = true;
        }
    }
}