namespace ArtificialIntelligence.GeneticAlgorithm
{
    /// <summary>
    /// Supports population generation for genetic algorithm.
    /// </summary>
    public interface IPopulationGenerator
    {
        /// <summary>
        /// Generates the initial population. 
        /// </summary>
        /// <param name="size">The new population's size.</param>
        /// <returns>An instance of <see cref="Population"/> with <paramref name="size"/> chromosomes.</returns>
        Population GenerateFirstPopulation(int size);

        /// <summary>
        /// Generates the initial population using the given chromosome.
        /// </summary>
        /// <param name="size">The new population's size.</param>
        /// <param name="chromosomeBrain">A chromosome's string representation.</param>
        /// <returns>An instance of <see cref="Population"/> with <paramref name="size"/> chromosomes.</returns>
        Population GenerateFirstPopulation(int size, string chromosomeBrain);

        /// <summary>
        /// Generates a new population from the given one.
        /// </summary>
        /// <remarks>
        /// This function assumes that the given population is already sorted and the first chromosome has the best 
        /// fitness value.
        /// </remarks>
        /// <param name="currentPopulation">An instance of <see cref="Population"/> containing current chromosomes.</param>
        /// <param name="generation">1-based number of the generation</param>
        /// <returns>An instance of <see cref="Population"/> containing new chromosomes.</returns>
        Population GeneratePopulation(Population currentPopulation, int generation);
    }
}