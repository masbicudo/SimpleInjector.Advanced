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
        //// based on: http://www.cuttingedge.it/blogs/steven/pivot/entry.php?id=94

        private const string AppSettingsPostFix = "AppSetting";

        private readonly Func<string, string> applicationSettingsReader;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSettingsConvention"/> class.
        /// </summary>
        public AppSettingsConvention()
        {
            this.applicationSettingsReader = x => ConfigurationManager.AppSettings[x];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSettingsConvention"/> class.
        /// </summary>
        /// <param name="applicationSettingsReader">
        /// An application settings reader delegate, that can change the default behavior.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// When <paramref name="applicationSettingsReader"/> is null.
        /// </exception>
        public AppSettingsConvention(Func<string, string> applicationSettingsReader)
        {
            if (applicationSettingsReader == null)
                throw new ArgumentNullException("applicationSettingsReader");

            this.applicationSettingsReader = applicationSettingsReader;
        }

        /// <summary>
        /// Returns a value indicating whether an <see cref="C:Expression"/> can be built and injected
        /// through the given <see cref="C:ParameterInfo"/> that represents a constructor parameter.
        /// </summary>
        /// <param name="parameter">A <see cref="C:ParameterInfo"/> representing a constructor parameter to inject a value to.</param>
        /// <returns>True if an <see cref="C:Expression"/> can be built and injected through a given constructor parameter.</returns>
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

        /// <summary>
        /// Returns an <see cref="C:Expression"/> from a <see cref="C:ParameterInfo"/> representing a constructor parameter.
        /// </summary>
        /// <param name="parameter">A <see cref="C:ParameterInfo"/> representing a constructor parameter to inject a value to.</param>
        /// <returns>An <see cref="C:Expression"/> that when compiled returns the value to inject into the constructor parameter.</returns>
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