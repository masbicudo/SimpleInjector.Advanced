using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleInjector.Advanced.Core;
using SimpleInjector.Advanced.Extensions;
using SimpleInjector.Advanced.Tests.Models;
using SimpleInjector.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace SimpleInjector.Advanced.Tests
{
    [TestClass]
    public class ConventionsTests
    {
        static ConventionsTests()
        {
            var tests = new RegisterWithContextTests();
            var methods = tests.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var methodInfo in methods.Where(m => m.GetCustomAttributes(typeof(TestMethodAttribute)).Any()))
                methodInfo.Invoke(tests, null);
        }

        [TestMethod]
        public void SimpleTest()
        {
            var container = new Container();

            container.Options.RegisterParameterConvention(new AppSettingsConvention(x => x + "Cfg"));
            container.Options.RegisterParameterConvention(new ConnectionStringsConvention(x => new ConnectionStringSettings(x, x + "ConnStr", x + "Prov")));

            container.Register<ConfigDepender>();

            container.Verify();

            var depender = container.GetInstance<ConfigDepender>();

            Assert.AreEqual(depender.MasbAppSetting, "masbCfg");
            Assert.AreEqual(depender.MasbConnectionString, "masbConnStr");
            Assert.AreEqual(depender.MasbConnectionProviderName, "masbProv");
            Assert.AreEqual(depender.Masb.ConnectionString, "masbConnStr");
            Assert.AreEqual(depender.Masb.ProviderName, "masbProv");
        }
    }
}