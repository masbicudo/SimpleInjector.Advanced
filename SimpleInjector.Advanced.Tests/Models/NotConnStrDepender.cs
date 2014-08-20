namespace SimpleInjector.Advanced.Tests.Models
{
    public class NotConnStrDepender<T>
    {
        public NotConnStrDepender(T notConnectionString)
        {
            this.NotConnectionString = notConnectionString;
        }

        public T NotConnectionString { get; set; }
    }
}