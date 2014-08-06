using System;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using SimpleInjector.Advanced.Helpers;

namespace SimpleInjector.Advanced.Core
{
    /// <summary>
    /// A convention to inject a value from the Connection Strings configuration section,
    /// when the parameter name of the constructor end with "ConnectionString" postfix.
    /// </summary>
    public class ConnectionStringsConvention : IParameterConvention
    {
        //// based on: http://www.cuttingedge.it/blogs/steven/pivot/entry.php?id=94

        private const string ConnectionStringPostFix = "ConnectionString";
        private const string ConnectionProviderNamePostFix = "ConnectionProviderName";

        private readonly Func<string, ConnectionStringSettings> connectionStringReader;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionStringsConvention"/> class.
        /// </summary>
        public ConnectionStringsConvention()
        {
            this.connectionStringReader = x => ConfigurationManager.ConnectionStrings[x];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionStringsConvention"/> class.
        /// </summary>
        /// <param name="connectionStringReader">
        /// A connection string reader delegate, that can change the default behavior.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// When <paramref name="connectionStringReader"/> is null.
        /// </exception>
        public ConnectionStringsConvention([NotNull] Func<string, ConnectionStringSettings> connectionStringReader)
        {
            if (connectionStringReader == null)
                throw new ArgumentNullException("connectionStringReader");

            this.connectionStringReader = connectionStringReader;
        }

        /// <summary>
        /// Returns a value indicating whether an <see cref="C:Expression"/> can be built and injected
        /// through the given <see cref="C:ParameterInfo"/> that represents a constructor parameter.
        /// </summary>
        /// <param name="parameter">A <see cref="C:ParameterInfo"/> representing a constructor parameter to inject a value to.</param>
        /// <returns>True if an <see cref="C:Expression"/> can be built and injected through a given constructor parameter.</returns>
        public bool CanResolve(ParameterInfo parameter)
        {
            bool resolvable =
                parameter.ParameterType == typeof(ConnectionStringSettings)
                || CheckParamName(parameter, ConnectionStringPostFix)
                || CheckParamName(parameter, ConnectionProviderNamePostFix);

            if (resolvable)
            {
                this.VerifyConfigurationFile(parameter);
            }

            return resolvable;
        }

        private static bool CheckParamName(ParameterInfo parameter, string postfix)
        {
            return parameter.ParameterType == typeof(string)
                   && parameter.Name.EndsWith(postfix)
                   && parameter.Name.Length > postfix.Length;
        }

        /// <summary>
        /// Returns an <see cref="C:Expression"/> from a <see cref="C:ParameterInfo"/> representing a constructor parameter.
        /// </summary>
        /// <param name="parameter">A <see cref="C:ParameterInfo"/> representing a constructor parameter to inject a value to.</param>
        /// <returns>An <see cref="C:Expression"/> that when compiled returns the value to inject into the constructor parameter.</returns>
        public Expression BuildExpression(ParameterInfo parameter)
        {
            if (parameter.ParameterType == typeof(ConnectionStringSettings))
            {
                var constr = this.GetConnectionString(parameter);
                return Expression.Constant(constr, typeof(ConnectionStringSettings));
            }

            if (parameter.ParameterType == typeof(string))
            {
                if (parameter.Name.EndsWith(ConnectionStringPostFix))
                {
                    var constr = this.GetConnectionString(parameter);
                    return Expression.Constant(constr.ConnectionString, typeof(string));
                }

                if (parameter.Name.EndsWith(ConnectionProviderNamePostFix))
                {
                    var constr = this.GetConnectionString(parameter);
                    return Expression.Constant(constr.ProviderName, typeof(string));
                }
            }

            throw new ArgumentException("Could not build expression from the given ParameterInfo", "parameter");
        }

        private void VerifyConfigurationFile(ParameterInfo parameter)
        {
            this.GetConnectionString(parameter);
        }

        private ConnectionStringSettings GetConnectionString(ParameterInfo parameter)
        {
            string[] namesToTry;
            if (parameter.ParameterType == typeof(ConnectionStringSettings))
            {
                var pureName =
                    parameter.Name.RemoveEnd(
                        new[] { "ConnectionStringSetting", "ConnectionString", "ConnectionSetting", "Connection" },
                        StringComparison.OrdinalIgnoreCase);

                namesToTry = new[]
                {
                    parameter.Name,
                    pureName,
                    pureName + "Connection",
                    pureName + "ConnectionString",
                    pureName + "ConnectionSetting",
                    pureName + "ConnectionStringSetting",
                };
            }
            else if (parameter.Name.EndsWith(ConnectionStringPostFix))
            {
                namesToTry = new[]
                {
                    parameter.Name.Substring(
                        0,
                        parameter.Name.LastIndexOf(ConnectionStringPostFix, StringComparison.Ordinal)),
                };
            }
            else if (parameter.Name.EndsWith(ConnectionProviderNamePostFix))
            {
                namesToTry = new[]
                {
                    parameter.Name.Substring(
                        0,
                        parameter.Name.LastIndexOf(ConnectionProviderNamePostFix, StringComparison.Ordinal)),
                };
            }
            else
            {
                throw new ArgumentException(
                    string.Format("Cannot get a ConnectionStringSettings for the given ParameterInfo: {0}", parameter.Name),
                    "parameter");
            }

            var settings = namesToTry
                .Where(name => name != null)
                .Select(name => this.connectionStringReader(name))
                .FirstOrDefault(s => s != null);

            if (settings == null)
            {
                throw new ActivationException(
                    string.Format(
                        "No connection string with name '{0}' could be found in the application's configuration file.",
                        string.Join("' or '", namesToTry.Distinct())));
            }

            return settings;
        }
    }
}