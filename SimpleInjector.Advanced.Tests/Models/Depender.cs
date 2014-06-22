namespace SimpleInjector.Advanced.Tests.Models
{
    public abstract class Depender : IDepender
    {
        protected Depender(IDependable dependable)
        {
            this.Dependable = dependable;
        }

        public IDependable Dependable { get; private set; }
    }
}