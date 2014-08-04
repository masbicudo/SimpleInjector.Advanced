using System.Linq.Expressions;
using System.Reflection;

namespace SimpleInjector.Advanced.Core
{
    internal class ConventionConstructorInjectionBehavior
        : IConstructorInjectionBehavior
    {
        private IConstructorInjectionBehavior decorated;
        private IParameterConvention convention;

        public ConventionConstructorInjectionBehavior(
            IConstructorInjectionBehavior decorated,
            IParameterConvention convention)
        {
            this.decorated = decorated;
            this.convention = convention;
        }

        public Expression BuildParameterExpression(
            ParameterInfo parameter)
        {
            if (!this.convention.CanResolve(parameter))
            {
                return this.decorated.BuildParameterExpression(parameter);
            }

            return this.convention.BuildExpression(parameter);
        }
    }
}