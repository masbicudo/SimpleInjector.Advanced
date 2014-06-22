namespace SimpleInjector.Advanced.Tests.Models
{
    public class DependerA : Depender
    {
        public DependerA(IDependable dependable)
            : base(dependable)
        {
        }
    }
}