namespace SimpleInjector.Advanced.Extensions
{
    internal interface IDependencyContextItem<out TService>
    {
        TService Service { get; }
    }
}