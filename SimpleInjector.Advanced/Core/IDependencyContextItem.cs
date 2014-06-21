namespace SimpleInjector.Advanced.Core
{
    internal interface IDependencyContextItem<out TService>
    {
        TService Service { get; }
    }
}