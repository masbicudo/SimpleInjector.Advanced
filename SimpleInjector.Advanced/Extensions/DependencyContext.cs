using System;
using System.Diagnostics;

namespace SimpleInjector.Advanced.Extensions
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

        internal DependencyContext(Type serviceType, Type implementationType)
        {
            this.ServiceType = serviceType;
            this.ImplementationType = implementationType;
            this.ServiceItemType = typeof(DependencyContextItem<,>).MakeGenericType(implementationType, serviceType);
        }

        private DependencyContext()
        {
        }

        /// <summary>
        /// Gets the type of service that is being requested.
        /// </summary>
        public Type ServiceType { get; private set; }

        /// <summary>
        /// Gets the type to which the created service will be handed over.
        /// </summary>
        public Type ImplementationType { get; private set; }

        internal Type ServiceItemType { get; private set; }
    }
}