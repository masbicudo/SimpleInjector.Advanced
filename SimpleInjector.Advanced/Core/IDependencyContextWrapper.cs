using System;
namespace SimpleInjector.Advanced.Core
{
    internal interface IDependencyContextWrapper<TService>
    {
        TService GetService(Func<DependencyContext, TService> f, DependencyContext c);
    }
}