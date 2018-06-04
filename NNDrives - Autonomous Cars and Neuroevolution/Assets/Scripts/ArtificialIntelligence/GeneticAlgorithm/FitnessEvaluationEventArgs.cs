using System;
using System.Collections.Generic;

namespace ArtificialIntelligence.GeneticAlgorithm
{
    /// <summary>
    /// Used to pass a key-value collection containing chromosomes' ids and fitness values to subscribers 
    /// when the <see cref="IFitnessEvaluator.EvaluationFinished"/> event is raised.
    /// </summary>
    public class FitnessEvaluationEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FitnessEvaluationEventArgs"/> class with given chromosomes' ids and fitness values.
        /// </summary>
        /// <param name="fitValues">Key-value collection containing the new FitnessEventArgs' chromosomes' ids and fitness values.</param>
        public FitnessEvaluationEventArgs(IDictionary<string, double> fitValues)
        {
            FitnessValues = fitValues;
        }

        /// <summary>
        /// Gets the key-value collection, containing the chromosomes' ids and fitness values respectively.
        /// </summary>
        public IDictionary<string, double> FitnessValues { get; private set; }
    }
}