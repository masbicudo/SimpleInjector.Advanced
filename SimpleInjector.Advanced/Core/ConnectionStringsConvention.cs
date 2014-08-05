using System;
using System.Configuration;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleInjector.Advanced.Core
{
    /// <summary>
    /// A convention to inject a value from the Connection Strings configuration section,
    /// when the parameter name of the constructor end with "ConnectionString" postfix.
    /// </summary>
    public class ConnectionStringsConvention : IParameterConvention
    {
        private const string ConnectionStringPostFix = "ConnectionString";
        private const string ConnectionProviderNamePostFix = "ConnectionProviderName";

        private readonly Func<string, ConnectionStringSettings> connectionStringReader;

        public ConnectionStringsConvention()
        {
            this.connectionStringReader = x => ConfigurationManager.ConnectionStrings[x];
        }

        public ConnectionStringsConvention(Func<string, ConnectionStringSettings> connectionStringReader)
        {
            if (connectionStringReader == null)
                throw new ArgumentNullException("connectionStringReader");

            this.connectionStringReader = connectionStringReader;
        }

        public bool CanResolve(ParameterInfo parameter)
        {
            var cn = ConnectionStringPostFix;
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

        private static bool CheckParamName(ParameterInfo parameter, string cn)
        {
            return parameter.ParameterType == typeof(string)
                    && parameter.Name.EndsWith(cn)
                    && parameter.Name.Length > cn.Length;
        }

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
            string name;
            if (parameter.ParameterType == typeof(ConnectionStringSettings))
            {
                name = parameter.Name;
            }
            else if (parameter.Name.EndsWith(ConnectionStringPostFix))
            {
                name = parameter.Name.Substring(
                    0,
                    parameter.Name.LastIndexOf(ConnectionStringPostFix, StringComparison.Ordinal));
            }
            else if (parameter.Name.EndsWith(ConnectionProviderNamePostFix))
            {
                name = parameter.Name.Substring(
                    0,
                    parameter.Name.LastIndexOf(ConnectionProviderNamePostFix, StringComparison.Ordinal));
            }
            else
            {
                throw new ArgumentException("Could not get ConnectionStringSettings from the given ParameterInfo", "parameter");
            }

            var settings = this.connectionStringReader(name);

            if (settings == null)
            {
                throw new ActivationException(
                    "No connection string with name '" + name +
                    "' could be found in the application's " +
                    "configuration file.");
            }

            return settings;
        }
    }
}