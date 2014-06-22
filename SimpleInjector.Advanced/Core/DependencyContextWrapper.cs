using System;

namespace SimpleInjector.Advanced.Core
{
    internal class DependencyContextWrapper<TService, TImplementation> : IDependencyContextWrapper<TService>
    {
        private bool hasValue;
        private TService service;
        private readonly object locker = new object();

        public TService GetService(Func<DependencyContext, TService> f, DependencyContext c)
        {
            if (this.hasValue)
                return service;

            lock (locker)
            {
                if (this.hasValue)
                    return service;

                this.hasValue = true;
                this.service = f(c);

                return this.service;
            }
        }
    }
}