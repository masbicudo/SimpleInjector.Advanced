using System;
using System.Diagnostics;

namespace SimpleInjector.Advanced.Core
{
    /// <summary>
    /// Describes the context in which a service request must be fulfilled.
    /// </summary>
    [DebuggerDisplay(
        "DependencyContext (ServiceType: {ServiceType}, " +
        "ImplementationType: {ImplementationType})")]
    public class DependencyContext
    {
        internal static readonly DependencyContext Root =
            new DependencyContext();

        private readonly Lazy<Type> wrapperType;

        internal DependencyContext(Type serviceType, Type implementationType, Type dependencyServiceType)
        {
            this.ServiceType = serviceType;
            this.ImplementationType = implementationType;
            this.wrapperType = new Lazy<Type>(
                () => typeof(DependencyContextWrapper<,>).MakeGenericType(
                    dependencyServiceType,
                    implementationType));
        }

        private DependencyContext()
        {
            this.wrapperType = new Lazy<Type>(() => null);
        }

        /// <summary>
        /// Gets the type of service that is being requested.
        /// </summary>
        public Type ServiceType { get; private set; }

        /// <summary>
        /// Gets the type to which the created service will be handed over.
        /// </summary>
        public Type ImplementationType { get; private set; }

        internal Type GetServiceWrapperType()
        {
            return this.wrapperType.Value;
        }
    }
}