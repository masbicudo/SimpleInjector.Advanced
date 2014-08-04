using System.Linq.Expressions;
using System.Reflection;

namespace SimpleInjector.Advanced.Core
{
    /// <summary>
    /// Represents constructor parameter injection by using conventions,
    /// base on <see cref="ParameterInfo"/>'s of the constructor.
    /// </summary>
    public interface IParameterConvention
    {
        /// <summary>
        /// Returns a value indicating whether an <see cref="C:Expression"/> can be built and injected
        /// through the given <see cref="C:ParameterInfo"/> that represents a constructor parameter.
        /// </summary>
        /// <param name="parameter">A <see cref="C:ParameterInfo"/> representing a constructor parameter to inject a value to.</param>
        /// <returns>True if an <see cref="C:Expression"/> can be built and injected through a given constructor parameter.</returns>
        bool CanResolve(ParameterInfo parameter);

        /// <summary>
        /// Returns an <see cref="C:Expression"/> from a <see cref="C:ParameterInfo"/> representing a constructor parameter.
        /// </summary>
        /// <param name="parameter">A <see cref="C:ParameterInfo"/> representing a constructor parameter to inject a value to.</param>
        /// <returns>An <see cref="C:Expression"/> that when compiled returns the value to inject into the constructor parameter.</returns>
        Expression BuildExpression(ParameterInfo parameter);
    }
}