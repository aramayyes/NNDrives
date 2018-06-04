using System;

namespace Helpers
{
    /// <summary>
    /// Contains all extension methods for <see cref="Random"/>.
    /// </summary>
    public static class RandomExtensions
    {
        /// <summary>
        /// Returns a random floating-point number from a Gaussian distribution using Box–Muller transform.
        /// </summary>
        /// <param name="random">The instance of <see cref="Random"/>.</param>
        /// <param name="mean">The mean or expectation of the distribution.</param>
        /// <param name="variance">Measures how far a set of (random) numbers are spread out from <paramref name="mean"/>.</param>
        /// <returns>A double-precision floating point number.</returns>
        public static double NextGaussian(this Random random, double mean, double variance)
        {
            // Uniform distribution:
            // map [0,1) -> (0,1].
            double un1 = 1.0 - random.NextDouble();
            double un2 = 1.0 - random.NextDouble();

            /*
             * Thanks to Box–Muller transform a random variable with a standard normal distribution
             * can be construted. (mean: 0, variance: 1) 
             */
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(un1)) * Math.Sin(2.0 * Math.PI * un2);

            return mean + (variance * randStdNormal);
        }
    }
}