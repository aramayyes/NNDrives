using System;

namespace ArtificialIntelligence.GeneticAlgorithm
{
    /// <summary>
    /// Supports fitness evaluation for a population.
    /// </summary>
    public interface IFitnessEvaluator
    {
        /// <summary>
        /// This event is raised when the fitness evaluation is finished.
        /// </summary>
        event EventHandler<FitnessEvaluationEventArgs> EvaluationFinished;

        /// <summary>
        /// Starts the fitness evaluation for each chromosome in the given population.
        /// </summary>
        /// <param name="population">Population for which fitness has to be evaluated.</param>
        void StartEvaluation(Population population);
    }
}