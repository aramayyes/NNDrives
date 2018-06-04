using System;

namespace ArtificialIntelligence.ArtificialNeuralNetwork
{
    /// <summary>
    /// Represents a collection of functions used as an activation function in neural layers.
    /// </summary>
    public static class LayerActivationFunctions
    {
        /// <summary>
        /// Represents the "Sigmoid" mathematical function.
        /// </summary>
        /// <remarks>
        /// This function is nonlinear and its values are from (0,1) range.
        /// </remarks>
        /// <param name="value">A double-precision floating-point number specifying the power of 1/e.</param>
        /// <returns>The sigmoid value at the given <paramref name="value"/> point.</returns>
        public static double Sigmoid(double value)
        {
            return 1 / (1 + Math.Exp(-value));
        }

        /// <summary>
        /// This is an activation function defined as the positive part of its argument.
        /// </summary>
        /// <param name="value">The number whose positive part is to be found.</param>
        /// <returns>The positive part of <paramref name="value"/>.</returns>
        public static double Relu(double value)
        {
            return value > 0 ? value : 0;
        }

        /// <summary>
        /// Represents the "Hyperbolic tangent" mathematical function.
        /// </summary>
        /// <remarks>
        /// This function is nonlinear and its values are from (-1,1) range.
        /// </remarks>
        /// <param name="value">An angle, measured in radians.</param>
        /// <returns>The hyperbolic tangent of the specified angle.</returns>
        public static double TanH(double value)
        {
            return Math.Tanh(value);
        }
    }
}