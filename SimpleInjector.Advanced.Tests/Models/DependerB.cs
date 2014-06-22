namespace SimpleInjector.Advanced.Tests.Models
{
    public class DependerB : Depender
    {
        public DependerB(IDependable dependable)
            : base(dependable)
        {
        }
    }
}