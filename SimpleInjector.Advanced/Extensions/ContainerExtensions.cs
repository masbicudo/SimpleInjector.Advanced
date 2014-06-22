using System;
using SimpleInjector.Advanced.Core;

namespace SimpleInjector.Advanced.Extensions
{
    /// <summary>
    /// Advanced Simple Injector container extension methods that allow advanced registration scenarios.
    /// </summary>
    public static class ContainerExtensions
    {
        /// <summary>
        ///     Registers the specified delegate <paramref name="contextBasedFactory"/> that will produce instances of
        /// type <typeparamref name="TService"/> and will be returned when an instance of type
        /// <typeparamref name="TService"/> is requested.
        ///     Be aware that by using this registration method,
        /// you are probably not following the best practices, like SOLID and DRY.
        ///     Read remarks for more information.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="container">The container in which the service will be registered.</param>
        /// <param name="contextBasedFactory">The delegate that allows building or creating new instances.</param>
        /// <exception cref="T:System.InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, 
        /// or when the <typeparamref name="TService"/> has already been registered.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// Thrown when one of the supplied arguments is a null reference (Nothing in VB).
        /// </exception>
        /// <remarks>
        /// <para>
        /// This code is based on a code snippet from SimpleInjector:
        /// <a href="https://simpleinjector.codeplex.com/wikipage?title=ContextDependentExtensions&amp;referringTitle=Advanced-scenarios">
        /// ContextDependentExtensions
        /// </a>
        /// </para>
        /// <para>
        /// Note that the use of this method is not encouraged.
        /// When you use this method, it is most probably an indication that you are not following
        /// design principles like SOLID and DRY.
        /// </para>
        /// <para>
        /// <h2>Information about these principles</h2>
        /// <ul>
        /// <li> <a href="http://stackoverflow.com/questions/9892137/windsor-pulling-transient-objects-from-the-container/9915056#9915056">
        /// Do I log too much? Do I violate the SOLID principles?
        /// </a> </li>
        /// <li> <a href="https://simpleinjector.codeplex.com/wikipage?title=Advanced-scenarios&amp;referringTitle=Advanced-scenarios&amp;ANCHOR#Context-Based-Injection">
        /// Context Based Injection
        /// </a> </li>
        /// <li> <a href="http://en.wikipedia.org/wiki/SOLID">
        /// SOLID (object-oriented design)
        /// </a> </li>
        /// </ul>
        /// </para>
        /// </remarks>
        public static void RegisterWithContext<TService>(
            this Container container,
            Func<DependencyContext, TService> contextBasedFactory)
            where TService : class
        {
            if (contextBasedFactory == null)
            {
                throw new ArgumentNullException("contextBasedFactory");
            }

            Func<TService> rootFactory =
                () => contextBasedFactory(DependencyContext.Root);

            container.Register(rootFactory, Lifestyle.Transient);

            // Allow the Func<DependencyContext, TService> to be 
            // injected into parent types.
            container.ExpressionBuilding += (sender, e) =>
            {
                if (e.RegisteredServiceType != typeof(TService))
                {
                    var rewriter = new DependencyContextRewriter
                    {
                        DependencyServiceType = typeof(TService),
                        ServiceType = e.RegisteredServiceType,
                        ContextBasedFactory = contextBasedFactory,
                        RootFactory = rootFactory,
                        Expression = e.Expression
                    };

                    e.Expression = rewriter.Visit(e.Expression);
                }
            };
        }

        /// <summary>
        ///     Registers the specified delegate <paramref name="contextBasedFactory"/> that will produce instances of
        /// type <typeparamref name="TService"/> and will be returned when an instance of type
        /// <typeparamref name="TService"/> is requested. The delegate is expected to produce new instances on
        /// each call. The instances are cached according to the supplied <paramref name="lifestyle"/>,
        /// and also cached by context `ImplementationType` property.
        ///     Be aware that by using this registration method,
        /// you are probably not following the best practices, like SOLID and DRY.
        ///     Read remarks for more information.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="container">The container in which the service will be registered.</param>
        /// <param name="contextBasedFactory">The delegate that allows building or creating new instances.</param>
        /// <param name="lifestyle">The lifestyle that specifies how the returned instance will be cached.</param>
        /// <exception cref="T:System.InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, 
        /// or when the <typeparamref name="TService"/> has already been registered.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// Thrown when one of the supplied arguments is a null reference (Nothing in VB).
        /// </exception>
        /// <remarks>
        /// <para>
        /// Note that the use of this method is not encouraged.
        /// When you use this method, it is most probably an indication that you are not following
        /// design principles like SOLID and DRY.
        /// </para>
        /// <para>
        /// <h2>Information about these principles</h2>
        /// <ul>
        /// <li> <a href="http://stackoverflow.com/questions/9892137/windsor-pulling-transient-objects-from-the-container/9915056#9915056">
        /// Do I log too much? Do I violate the SOLID principles?
        /// </a> </li>
        /// <li> <a href="https://simpleinjector.codeplex.com/wikipage?title=Advanced-scenarios&amp;referringTitle=Advanced-scenarios&amp;ANCHOR#Context-Based-Injection">
        /// Context Based Injection
        /// </a> </li>
        /// <li> <a href="http://en.wikipedia.org/wiki/SOLID">
        /// SOLID (object-oriented design)
        /// </a> </li>
        /// </ul>
        /// </para>
        /// </remarks>
        public static void RegisterWithContext<TService>(
            this Container container,
            Func<DependencyContext, TService> contextBasedFactory,
            Lifestyle lifestyle)
            where TService : class
        {
            if (contextBasedFactory == null)
            {
                throw new ArgumentNullException("contextBasedFactory");
            }

            container.ResolveUnregisteredType += (sender, args) =>
            {
                if (!args.Handled && typeof(IDependencyContextWrapper<TService>).IsAssignableFrom(args.UnregisteredServiceType))
                    args.Register(lifestyle.CreateRegistration(args.UnregisteredServiceType, (Container)sender));
            };

            Func<DependencyContext, TService> depFactory =
                c =>
                {
                    if (c.GetServiceWrapperType() == null)
                        return contextBasedFactory(c);

                    var obj = container.GetInstance(c.GetServiceWrapperType());
                    var wrapper = (IDependencyContextWrapper<TService>)obj;
                    return wrapper.GetService(contextBasedFactory, c);
                };

            Func<TService> rootFactory =
                () => depFactory(DependencyContext.Root);

            container.Register(rootFactory, Lifestyle.Transient);

            // Allow the Func<DependencyContext, TService> to be 
            // injected into parent types.
            container.ExpressionBuilding += (sender, e) =>
            {
                if (e.RegisteredServiceType != typeof(TService))
                {
                    var rewriter = new DependencyContextRewriter
                    {
                        DependencyServiceType = typeof(TService),
                        ServiceType = e.RegisteredServiceType,
                        ContextBasedFactory = depFactory,
                        RootFactory = rootFactory,
                        Expression = e.Expression
                    };

                    e.Expression = rewriter.Visit(e.Expression);
                }
            };
        }

        /// <summary>
        ///     Registers the specified delegate <paramref name="contextBasedFactory"/> that will produce instances of
        /// type <typeparamref name="TService"/> and will be returned when an instance of type
        /// <typeparamref name="TService"/> is requested. 
        /// The delegate is expected to produce new instances on each call.
        /// This delegate will be called at most once per context `ImplementationType` during the lifetime of the application.
        /// The returned instance must be thread-safe when working in a multi-threaded environment.
        ///     Be aware that by using this registration method,
        /// you are probably not following the best practices, like SOLID and DRY.
        ///     Read remarks for more information.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="container">The container in which the service will be registered.</param>
        /// <param name="contextBasedFactory">The delegate that allows building or creating new instances.</param>
        /// <exception cref="T:System.InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, 
        /// or when the <typeparamref name="TService"/> has already been registered.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// Thrown when one of the supplied arguments is a null reference (Nothing in VB).
        /// </exception>
        /// <remarks>
        /// <para>
        /// Note that the use of this method is not encouraged.
        /// When you use this method, it is most probably an indication that you are not following
        /// design principles like SOLID and DRY.
        /// </para>
        /// <para>
        /// <h2>Information about these principles</h2>
        /// <ul>
        /// <li> <a href="http://stackoverflow.com/questions/9892137/windsor-pulling-transient-objects-from-the-container/9915056#9915056">
        /// Do I log too much? Do I violate the SOLID principles?
        /// </a> </li>
        /// <li> <a href="https://simpleinjector.codeplex.com/wikipage?title=Advanced-scenarios&amp;referringTitle=Advanced-scenarios&amp;ANCHOR#Context-Based-Injection">
        /// Context Based Injection
        /// </a> </li>
        /// <li> <a href="http://en.wikipedia.org/wiki/SOLID">
        /// SOLID (object-oriented design)
        /// </a> </li>
        /// </ul>
        /// </para>
        /// </remarks>
        public static void RegisterSingleWithContext<TService>(
            this Container container,
            Func<DependencyContext, TService> contextBasedFactory)
            where TService : class
        {
            container.RegisterWithContext(contextBasedFactory, Lifestyle.Singleton);
        }
    }
}