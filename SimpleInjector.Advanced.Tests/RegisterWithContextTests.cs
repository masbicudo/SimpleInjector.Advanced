using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleInjector.Advanced.Extensions;
using SimpleInjector.Advanced.Tests.Models;
using SimpleInjector.Extensions;
using SimpleInjector.Integration.Web;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace SimpleInjector.Advanced.Tests
{
    [TestClass]
    public class RegisterWithContextTests
    {
        static RegisterWithContextTests()
        {
            var tests = new RegisterWithContextTests();
            var methods = tests.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var methodInfo in methods.Where(m => m.GetCustomAttributes(typeof(TestMethodAttribute), true).Any()))
                methodInfo.Invoke(tests, null);
        }

        [TestMethod]
        public void RegisterWithContext_TestWithoutLifestyle()
        {
            var container = new Container();

            container.RegisterOpenGeneric(typeof(IDependable<>), typeof(Dependable<>));

            container.RegisterWithContext(
                context =>
                {
                    if (context.ImplementationType == null)
                        return new Dependable<string>();

                    var type = typeof(IDependable<>).MakeGenericType(
                        context.ImplementationType);

                    return (IDependable)container.GetInstance(type);
                });

            container.Verify();

            var depender = container.GetInstance<DependerA>();

            Assert.IsInstanceOfType(depender.Dependable, typeof(IDependable<DependerA>));
        }

        [TestMethod]
        public void RegisterWithContext_TestWithSingletonLifestyle()
        {
            var container = new Container();

            container.RegisterOpenGeneric(typeof(IDependable<>), typeof(Dependable<>));

            container.RegisterWithContext(
                context =>
                {
                    if (context.ImplementationType == null)
                        return new Dependable<string>();

                    var type = typeof(IDependable<>).MakeGenericType(
                        context.ImplementationType);

                    return (IDependable)container.GetInstance(type);
                }, Lifestyle.Singleton);

            container.Verify();

            var dependerA1 = container.GetInstance<DependerA>();
            var dependerA2 = container.GetInstance<DependerA>();
            var dependerB1 = container.GetInstance<DependerB>();

            Assert.AreNotEqual(dependerA1, dependerA2);
            Assert.IsInstanceOfType(dependerA1.Dependable, typeof(IDependable<DependerA>));
            Assert.IsInstanceOfType(dependerA2.Dependable, typeof(IDependable<DependerA>));
            Assert.AreEqual(dependerA1.Dependable, dependerA2.Dependable);

            Assert.IsInstanceOfType(dependerB1.Dependable, typeof(IDependable<DependerB>));
            Assert.AreNotEqual(dependerA1.Dependable, dependerB1.Dependable);
        }

        [TestMethod]
        public void RegisterWithContext_TestWithWebLifestyle()
        {
            var container = new Container();

            container.RegisterOpenGeneric(typeof(IDependable<>), typeof(Dependable<>));

            var webRequestLifestyle = new WebRequestLifestyle();
            container.RegisterWithContext(
                context =>
                {
                    if (context.ImplementationType == null)
                        return new Dependable<string>();

                    var type = typeof(IDependable<>).MakeGenericType(
                        context.ImplementationType);

                    return (IDependable)container.GetInstance(type);
                }, webRequestLifestyle);

            container.Verify();

            HttpContext.Current = new HttpContext(new HttpRequest("fileName", "http://www.xpto.com", ""), new HttpResponse(new StringWriter()));

            var dependerCtx11 = container.GetInstance<DependerA>();
            var dependerCtx12 = container.GetInstance<DependerA>();

            HttpContext.Current = new HttpContext(new HttpRequest("fileName", "http://www.xpto.com", ""), new HttpResponse(new StringWriter()));

            var dependerCtx2 = container.GetInstance<DependerA>();

            Assert.AreEqual(dependerCtx11.Dependable, dependerCtx12.Dependable);
            Assert.AreNotEqual(dependerCtx11.Dependable, dependerCtx2.Dependable);
        }
    }
}