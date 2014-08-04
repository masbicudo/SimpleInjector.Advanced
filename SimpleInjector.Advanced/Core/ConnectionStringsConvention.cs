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

        public bool CanResolve(ParameterInfo parameter)
        {
            bool resolvable =
                parameter.ParameterType == typeof(string) &&
                parameter.Name.EndsWith(ConnectionStringPostFix) &&
                parameter.Name.LastIndexOf(ConnectionStringPostFix, StringComparison.Ordinal) > 0;

            if (resolvable)
            {
                this.VerifyConfigurationFile(parameter);
            }

            return resolvable;
        }

        public Expression BuildExpression(ParameterInfo parameter)
        {
            var constr = this.GetConnectionString(parameter);

            return Expression.Constant(constr, typeof(string));
        }

        private void VerifyConfigurationFile(ParameterInfo parameter)
        {
            this.GetConnectionString(parameter);
        }

        private string GetConnectionString(ParameterInfo parameter)
        {
            string name = parameter.Name.Substring(
                0,
                parameter.Name.LastIndexOf(ConnectionStringPostFix, StringComparison.Ordinal));

            ConnectionStringSettings settings =
                ConfigurationManager.ConnectionStrings[name];

            if (settings == null)
            {
                throw new ActivationException(
                    "No connection string with name '" + name +
                    "' could be found in the application's " +
                    "configuration file.");
            }

            return settings.ConnectionString;
        }
    }
}