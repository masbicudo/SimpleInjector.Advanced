using System;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleInjector.Advanced.Core
{
    /// <summary>
    /// A convention to inject a value from the Application Settings configuration section,
    /// when the parameter name of the constructor end with "AppSetting" postfix.
    /// </summary>
    public class AppSettingsConvention : IParameterConvention
    {
        private const string AppSettingsPostFix = "AppSetting";

        private readonly Func<string, string> applicationSettingsReader;

        public AppSettingsConvention()
        {
            this.applicationSettingsReader = x => ConfigurationManager.AppSettings[x];
        }

        public AppSettingsConvention(Func<string, string> applicationSettingsReader)
        {
            if (applicationSettingsReader == null)
                throw new ArgumentNullException("applicationSettingsReader");

            this.applicationSettingsReader = applicationSettingsReader;
        }

        public bool CanResolve(ParameterInfo parameter)
        {
            Type type = parameter.ParameterType;

            bool resolvable =
                (type.IsValueType || type == typeof(string)) &&
                parameter.Name.EndsWith(AppSettingsPostFix) &&
                parameter.Name.LastIndexOf(AppSettingsPostFix, StringComparison.Ordinal) > 0;

            if (resolvable)
            {
                this.VerifyConfigurationFile(parameter);
            }

            return resolvable;
        }

        public Expression BuildExpression(ParameterInfo parameter)
        {
            object valueToInject = this.GetAppSettingValue(parameter);

            return Expression.Constant(
                valueToInject,
                parameter.ParameterType);
        }

        private void VerifyConfigurationFile(ParameterInfo parameter)
        {
            this.GetAppSettingValue(parameter);
        }

        private object GetAppSettingValue(ParameterInfo parameter)
        {
            string key = parameter.Name.Substring(
                0,
                parameter.Name.LastIndexOf(AppSettingsPostFix, StringComparison.Ordinal));

            string configurationValue =
                this.applicationSettingsReader(key);

            if (configurationValue == null)
            {
                throw new ActivationException(
                    "No app setting with key '" + key + "' " +
                    "could be found in the application's " +
                    "configuration file.");
            }

            TypeConverter converter = TypeDescriptor.GetConverter(
                parameter.ParameterType);

            return converter.ConvertFromString(
                // ReSharper disable once AssignNullToNotNullAttribute
                null,
                CultureInfo.InvariantCulture,
                configurationValue);
        }
    }
}