﻿using System.Configuration;

namespace ASB_ManageAzureServicesGetFees
{
    /// <summary>
    /// Helper Methods to make tasks easier.
    /// </summary>
    public static class ExtensionMethods
    {
        #region Local Members

        private static string TemplatePieceKeyName = "[KeyName]";
        private static string TemplatePieceKey = "[Key]";
        private static string TemplatePieceServiceName = "[ServiceName]";
        private static string BaseServiceBusEndpoint = "sb://{0}.servicebus.windows.net/";

        #endregion


        #region Public Members

        /// <summary>
        /// A fully qualified Azure Service Bus connection string.
        /// </summary>
        /// <remarks>
        /// This value will contain all the security keys and values.
        /// </remarks>
        public static string AzureServiceConnectionString(this EnvironmentType environment)
        {
            string newValue = environment.AzureServiceConnectionTemplate()
                .Replace(TemplatePieceServiceName, environment.AzureServiceName())
                .Replace(TemplatePieceKeyName, environment.AzureServiceKeyName())
                .Replace(TemplatePieceKey, environment.AzureServiceKeyValue());

            return newValue;
        }

        /// <summary>
        /// The name of the Topic for the specified Environment Type.
        /// </summary>
        public static string AzureTopicName(this EnvironmentType environment) => $"GetFees.{environment.ShortName()}";

        /// <summary>
        /// The name of the Message Queue for the specified Environment Type.
        /// </summary>
        public static string MessageQueueName(this EnvironmentType environment) => $"GetFeesRequests.{environment.ShortName()}";

        /// <summary>
        /// The abbreviation for the specified Environment Type.
        /// </summary>
        public static string ShortName(this EnvironmentType environment)
        {
            string shortName = "NONE";

            switch (environment)
            {
                case EnvironmentType.Development:
                    shortName = "DEV";
                    break;
                case EnvironmentType.UserAcceptanceTesting:
                    shortName = "UAT";
                    break;
                case EnvironmentType.Staging:
                    shortName = "STG";
                    break;
                case EnvironmentType.Production:
                    shortName = "PRD";
                    break;
            }

            return shortName;
        }

        /// <summary>
        /// The basic piece of the ConnectionString that can identify the Azure Service but not enough to connect nor expose the security elements.
        /// </summary>
        public static string AzureServiceBaseUri(this EnvironmentType environment) => string.Format(BaseServiceBusEndpoint, environment.AzureServiceName());

        /// <summary>
        /// Get the name of the Azure Service Bus service to which we should connect.
        /// </summary>
        /// <remarks>
        /// This is a required parameter for creating the ConnectionString.
        /// </remarks>
        public static string AzureServiceName(this EnvironmentType environment)
        {
            string configKeyName = $"ASB_ServiceName_{environment.ShortName()}";

            return ConfigurationManager.AppSettings[configKeyName];
        }

        /// <summary>
        /// Get the name of the Key, from the app.config file, required for connecting to an Azure Service Bus.
        /// </summary><remarks>
        /// This is a required parameter for creating the ConnectionString.
        /// </remarks>
        public static string AzureServiceKeyName(this EnvironmentType environment)
        {
            string configKeyName = $"ASB_KeyName_{environment.ShortName()}";

            return ConfigurationManager.AppSettings[configKeyName];
        }

        /// <summary>
        /// Get the Key, from the app.config file, required for connecting to an Azure Service Bus.
        /// </summary>
        public static string AzureServiceKeyValue(this EnvironmentType environment)
        {
            string configKeyName = $"ASB_KeyValue_{environment.ShortName()}";

            return ConfigurationManager.AppSettings[configKeyName];
        }

        /// <summary>
        /// Get the Connection String Template, from the app.config file, required for connecting to an Azure Service Bus.
        /// </summary>
        public static string AzureServiceConnectionTemplate(this EnvironmentType environment)
        {
            string configKeyName = $"ASB_ConnectionString_{environment.ShortName()}";

            return ConfigurationManager.AppSettings[configKeyName];
        }

        #endregion
    }
}
