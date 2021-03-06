using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleInjector.Advanced.Core;
using SimpleInjector.Advanced.Extensions;
using SimpleInjector.Advanced.Tests.Models;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace SimpleInjector.Advanced.Tests
{
    [TestClass]
    public class ConventionsTests
    {
        static ConventionsTests()
        {
            var tests = new ConventionsTests();
            var methods = tests.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (
                var methodInfo in methods.Where(m => m.GetCustomAttributes(typeof(TestMethodAttribute), true).Any()))
                methodInfo.Invoke(tests, null);
        }

        [TestMethod]
        public void Conventions_SimpleTest()
        {
            var container = CreateContainerWithConventions();

            container.Register<ConfigDepender>();

            container.Verify();

            var depender = container.GetInstance<ConfigDepender>();

            Assert.AreEqual(depender.MasbAppSetting, "masbCfg");
            Assert.AreEqual(depender.MasbConnectionString, "masbConnStr");
            Assert.AreEqual(depender.MasbConnectionProviderName, "masbProv");
            Assert.AreEqual(depender.Masb.ConnectionString, "masbConnStr");
            Assert.AreEqual(depender.Masb.ProviderName, "masbProv");
            Assert.AreEqual(depender.MasbConnection.ConnectionString, "masbConnStr1");
            Assert.AreEqual(depender.MasbConnectionSettings.ConnectionString, "masbConnStr2");
            Assert.AreEqual(depender.MasbConnectionStringSetting.ConnectionString, "masbConnStr");
        }

        [TestMethod]
        public void Conventions_NotInjectableParams()
        {
            var container = CreateContainerWithConventions();

            container.Register<NotConnStrDepender<char[]>>();

            Exception exception = null;
            try
            {
                container.Verify();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Assert.IsInstanceOfType(exception, typeof(InvalidOperationException));
        }

        [TestMethod]
        public void Conventions_NotInjectableParams_2()
        {
            var container = CreateContainerWithConventions();

            Exception exception = null;
            try
            {
                container.Register<NotConnStrDepender<string>>();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Assert.IsInstanceOfType(exception, typeof(ArgumentException));
        }

        private static Container CreateContainerWithConventions()
        {
            var container = new Container();

            container.Options.RegisterParameterConvention(
                new AppSettingsConvention(x => x + "Cfg"));

            container.Options.RegisterParameterConvention(
                new ConnectionStringsConvention(
                    x =>
                    {
                        switch (x)
                        {
                            case "masb":
                                return new ConnectionStringSettings(
                                    "masb",
                                    "masbConnStr",
                                    "masbProv");

                            case "masbConnection":
                                return new ConnectionStringSettings(
                                    "masbConnection",
                                    "masbConnStr1",
                                    "masbProv");

                            case "masbConnectionSetting":
                                return new ConnectionStringSettings(
                                    "masbConnectionSettings",
                                    "masbConnStr2",
                                    "masbProv");

                            default:
                                return null;
                        }
                    }));

            return container;
        }
    }
}