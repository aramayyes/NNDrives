using System;
using System.Linq;

namespace ArtificialIntelligence.GeneticAlgorithm
{
    /// <summary>
    /// Manager class which implements the Genetic Algorithm.
    /// </summary>
    public class GeneticAlgorithmManager
    {
        /// <summary>
        /// An object which has to evaluate the fitness values for a population.
        /// </summary>
        private readonly IFitnessEvaluator fitnessEvaluator;

        /// <summary>
        /// An object which has to generate populations for each generation.
        /// </summary>
        private readonly IPopulationGenerator populationGenerator;

        /// <summary>
        /// Tells if the algorithm is already started.
        /// </summary>
        private bool isStarted = false;

        /// <summary>
        /// Contains the initial chromosome.
        /// </summary>
        private string initialChromosomeBrain = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneticAlgorithmManager"/> class 
        /// with given <paramref name="fitnessEvaluator"/> and <paramref name="populationSize"/>.
        /// </summary>
        /// <param name="fitnessEvaluator">Object which has to evaluate fitness values for chromosomes in each population.</param>
        /// <param name="populationGenerator">Object which has to generate populations for each generation.</param>
        /// <param name="populationSize">The number of chromosomes of population used in the algorithm.</param>
        /// <param name="initialChromosomeBrain">The string representation of the initial chromosome.</param>
        public GeneticAlgorithmManager(IFitnessEvaluator fitnessEvaluator, IPopulationGenerator populationGenerator = null, int populationSize = 4, string initialChromosomeBrain = null)
        {
            if (fitnessEvaluator == null)
            {
                throw new ArgumentNullException("fitnessEvaluator");
            }

            PopulationSize = populationSize;
            Generation = 0;

            this.fitnessEvaluator = fitnessEvaluator;
            this.fitnessEvaluator.EvaluationFinished += FitnessEvaluator_EvaluationFinished;

            this.populationGenerator = populationGenerator ?? new SRMPopulationGenerator(25);

            this.initialChromosomeBrain = initialChromosomeBrain;
        }

        /// <summary>
        /// This event is raised when the fitness values are evaluated for each chromosome in the current population,
        /// and can be handled to finish the algorithm.
        /// </summary>
        public event EventHandler<GenerationEventArgs> GenerationChanging = delegate { };

        /// <summary>
        /// This event is raised when algorithm is finished.
        /// </summary>
        public event EventHandler<AlgorithmFinishedEventArgs> AlgorithmFinished = delegate { };

        /// <summary>
        /// Gets the number of chromosomes in the population.
        /// </summary>
        public int PopulationSize { get; private set; }

        /// <summary>
        /// Gets the 1-based number of the current generation.
        /// </summary>
        public int Generation { get; private set; }

        /// <summary>
        /// Gets the current population.
        /// </summary>
        public Population CurrentPopulation { get; private set; }

        /// <summary>
        /// Starts the algorithm if it hasn't been done yet.
        /// </summary>
        /// <returns>True if algorithm is just started, false if it has been already started.</returns>
        public bool Start()
        {
            if (isStarted)
            {
                return false;
            }

            StartIteration();
            return true;
        }

        /// <summary>
        /// Raises the <see cref="GenerationChanging"/> event with given arguments.
        /// </summary>
        /// <param name="e">Contains the data which will be passed to the subscribers.</param>
        protected virtual void OnGenerationChanging(GenerationEventArgs e)
        {
            EventHandler<GenerationEventArgs> handler = GenerationChanging;
            handler(this, e);
        }

        /// <summary>
        /// Raises the <see cref="AlgorithmFinished"/> event with given arguments.
        /// </summary>
        /// <param name="e">Contains the data which will be passed to the subscribers.</param>
        protected virtual void OnAlgorithmFinished(AlgorithmFinishedEventArgs e)
        {
            EventHandler<AlgorithmFinishedEventArgs> handler = AlgorithmFinished;
            handler(this, e);
        }

        /// <summary>
        /// This function is executed when <see cref="fitnessEvaluator"/> finishes evaluating.
        /// </summary>
        /// <param name="sender">The sender objet.</param>
        /// <param name="e">Object containing the fitness values for chromosomes in the <see cref="CurrentPopulation"/>.</param>
        private void FitnessEvaluator_EvaluationFinished(object sender, FitnessEvaluationEventArgs e)
        {
            CurrentPopulation.SortByFitness(e.FitnessValues);

            double bestFitness = e.FitnessValues[CurrentPopulation.First().Id];

            var generationEventArgs = new GenerationEventArgs(Generation, bestFitness, e.FitnessValues.Sum(kvPair => kvPair.Value));
            OnGenerationChanging(generationEventArgs);

            if (!generationEventArgs.IsTheLast)
            {
                StartIteration();
            }
            else
            {
                OnAlgorithmFinished(new AlgorithmFinishedEventArgs(CurrentPopulation.First()));
            }
        }

        /// <summary>
        /// Starts the algorithm's number (<see cref="Generation"/> + 1) iteration.
        /// </summary>
        private void StartIteration()
        {
            ++Generation;

            if (Generation == 1)
            {
                if (initialChromosomeBrain != null)
                {
                    CurrentPopulation = populationGenerator.GenerateFirstPopulation(PopulationSize, initialChromosomeBrain);
                }
                else
                {
                    CurrentPopulation = populationGenerator.GenerateFirstPopulation(PopulationSize);
                }
            }
            else
            {
                CurrentPopulation = populationGenerator.GeneratePopulation(CurrentPopulation, Generation);
            }

            fitnessEvaluator.StartEvaluation(CurrentPopulation);
        }
    }
}