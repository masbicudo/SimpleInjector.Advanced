using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleInjector.Advanced.Extensions;
using SimpleInjector.Advanced.Tests.Models;
using SimpleInjector.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace SimpleInjector.Advanced.Tests
{
    [TestClass]
    public class RegisterLazyTests
    {
        static RegisterLazyTests()
        {
            var tests = new RegisterWithContextTests();
            var methods = tests.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var methodInfo in methods.Where(m => m.GetCustomAttributes(typeof(TestMethodAttribute), true).Any()))
                methodInfo.Invoke(tests, null);
        }

        [TestMethod]
        public void SimpleTest()
        {
            var container = new Container();

            container.RegisterOpenGeneric(typeof(IDependable<>), typeof(Dependable<>));
            container.Register<Depender<Lazy<IDependable<int>>>>();
            container.RegisterLazy();

            container.Verify();

            var depender = container.GetInstance<Depender<Lazy<IDependable<int>>>>();

            Assert.IsInstanceOfType(depender.Dependable, typeof(Lazy<IDependable<int>>));
            Assert.IsInstanceOfType(depender.Dependable.Value, typeof(IDependable<int>));
        }

        [TestMethod]
        public void NestedTest()
        {
            var container = new Container();

            container.RegisterOpenGeneric(typeof(IDependable<>), typeof(Dependable<>));
            container.Register<Depender<Lazy<IDependable<int>>>>();
            container.RegisterLazy();

            container.Verify();

            var depender = container.GetInstance<Depender<Lazy<Lazy<IDependable<int>>>>>();

            Assert.IsInstanceOfType(depender.Dependable, typeof(Lazy<Lazy<IDependable<int>>>));
            Assert.IsInstanceOfType(depender.Dependable.Value, typeof(Lazy<IDependable<int>>));
            Assert.IsInstanceOfType(depender.Dependable.Value.Value, typeof(IDependable<int>));
        }
    }
}