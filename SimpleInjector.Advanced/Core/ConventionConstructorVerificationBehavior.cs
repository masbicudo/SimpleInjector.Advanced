using System.Reflection;

namespace SimpleInjector.Advanced.Core
{
    internal class ConventionConstructorVerificationBehavior
        : IConstructorVerificationBehavior
    {
        private IConstructorVerificationBehavior decorated;
        private IParameterConvention convention;

        public ConventionConstructorVerificationBehavior(
            IConstructorVerificationBehavior decorated,
            IParameterConvention convention)
        {
            this.decorated = decorated;
            this.convention = convention;
        }

        public void Verify(ParameterInfo parameter)
        {
            if (!this.convention.CanResolve(parameter))
            {
                this.decorated.Verify(parameter);
            }
        }
    }
}