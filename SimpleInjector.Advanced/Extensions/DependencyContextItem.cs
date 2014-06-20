namespace SimpleInjector.Advanced.Extensions
{
    internal class DependencyContextItem<TImplementation, TService> : IDependencyContextItem<TService>
    {
        public DependencyContextItem(TService service)
        {
            this.Service = service;
        }

        public TService Service { get; private set; }
    }
}