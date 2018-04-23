using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.ServiceBus;

namespace ASB_ManageAzureServicesGetFees
{
    /// <summary>
    /// All the functionality and features needed for interacting with a single Azure Service Bus
    /// </summary>
    internal sealed class AzureServicesHelper
    {
        #region Properties

        private static NamespaceManager _namespaceManager;
        private EnvironmentType EnvironmentType { get; }

        /// <summary>
        /// Indicate if the Get Fees Topic exists for this Azure Service
        /// </summary>
        public bool TopicExists => _namespaceManager.TopicExistsAsync(EnvironmentType.AzureTopicName()).Result;

        /// <summary>
        /// Indicate if the Get Fees Message Queue exists for this Azure Service
        /// </summary>
        public bool QueueExists => _namespaceManager.QueueExistsAsync(EnvironmentType.MessageQueueName()).Result;

        /// <summary>
        /// A list of every Subscription for the Get Fees Topic
        /// </summary>
        public List<string> SubscriptionNames
        {
            get
            {
                if (_subscriptionNames == null || _subscriptionNames.Count < 255)
                {
                    _subscriptionNames = new List<string>();

                    // 0-9 and A-F
                    // ASCII 48 (0) to 57 (9)
                    // ASCII 65 (A) to 70 (F)

                    byte[] firstByte;
                    byte[] secondByte;
                    string firstCharacter;
                    string secondCharacter;

                    // Get the first character:
                    for (int a = 48; a < 58; a++)
                    {
                        firstByte = new[] { (byte)a };
                        firstCharacter = System.Text.Encoding.ASCII.GetString(firstByte);

                        // Get the second character:
                        for (int b = 65; b < 71; b++)
                        {
                            secondByte = new[] { (byte)b };
                            secondCharacter = System.Text.Encoding.ASCII.GetString(secondByte);
                            _subscriptionNames.Add($"{firstCharacter}{secondCharacter}");
                        }

                        // Get the next possible second character:
                        for (int c = 48; c < 58; c++)
                        {
                            secondByte = new[] { (byte)c };
                            secondCharacter = System.Text.Encoding.ASCII.GetString(secondByte);
                            _subscriptionNames.Add($"{firstCharacter}{secondCharacter}");
                        }
                    }

                    // Get the next possible first character:
                    for (int d = 65; d < 71; d++)
                    {
                        firstByte = new[] { (byte)d };
                        firstCharacter = System.Text.Encoding.ASCII.GetString(firstByte);

                        // Get the second character:
                        for (int e = 48; e < 58; e++)
                        {
                            secondByte = new[] { (byte)e };
                            secondCharacter = System.Text.Encoding.ASCII.GetString(secondByte);
                            _subscriptionNames.Add($"{firstCharacter}{secondCharacter}");
                        }

                        // Get the next possible second character:
                        for (int f = 65; f < 71; f++)
                        {
                            secondByte = new[] { (byte)f };
                            secondCharacter = System.Text.Encoding.ASCII.GetString(secondByte);
                            _subscriptionNames.Add($"{firstCharacter}{secondCharacter}");
                        }
                    }
                }

                return _subscriptionNames;
            }
        }
        private List<string> _subscriptionNames;

        #endregion


        #region Constructor

        /// <summary>
        /// Create a new instance of an AzureServicesHelper that will perform work for a single Azure Service Bus.
        /// </summary>
        /// <param name="environment">
        /// Specifies the single specific Azure Service Bus against which all work will be performed.
        /// </param>
        public AzureServicesHelper(EnvironmentType environment)
        {
            EnvironmentType = environment;
            SetAzureServiceManager(environment);
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Determine the Get Fees Subscription name for a Loan GUID 
        /// </summary>
        public string SubscriptionNameForLoanGuid(Guid loanGuid)
        {
            string modifiedValue = loanGuid.ToString("N").ToUpper(CultureInfo.InvariantCulture);
            int beginningPosition = modifiedValue.Length - 2;
            string newValue = modifiedValue.Substring(beginningPosition, 2);

            if (!SubscriptionNames.Exists(n => n.Equals(newValue)))
            {
                newValue = "00";
            }

            return newValue;
        }

        /// <summary>
        /// Create a Message Queue, if it does not already exist, and indicate if the attempt was successful.
        /// </summary>
        public bool CreateQueue()
        {
            string queueName = EnvironmentType.MessageQueueName();
            bool queueExists = QueueExists;

            if (!queueExists)
            {
                _namespaceManager.CreateQueueAsync(queueName).Wait();
                queueExists = QueueExists;
            }

            return queueExists;
        }

        /// <summary>
        /// Delete a Message Queue, if it exists, and indicate if the attempt was successful.
        /// </summary>
        public bool DeleteQueue()
        {
            string queueName = EnvironmentType.MessageQueueName();
            bool queueExists = QueueExists;

            if (queueExists)
            {
                _namespaceManager.DeleteQueueAsync(queueName).Wait();
                queueExists = QueueExists;
            }

            return !queueExists;
        }

        /// <summary>
        /// Create a Topic, if it does not already exist, and indicate if the attempt was successful.
        /// </summary>
        public bool CreateTopic()
        {
            string topicName = EnvironmentType.AzureTopicName();
            bool topicExists = TopicExists;

            if (!topicExists)
            {
                _namespaceManager.CreateTopicAsync(topicName).Wait();
                topicExists = TopicExists;
            }

            return topicExists;
        }

        /// <summary>
        /// Delete a Topic, if it exists, and indicate if the attempt was successful.
        /// </summary>
        public bool DeleteTopic()
        {
            string topicName = EnvironmentType.AzureTopicName();
            bool topicExists = TopicExists;

            if (topicExists)
            {
                _namespaceManager.DeleteTopicAsync(topicName).Wait();
                topicExists = TopicExists;
            }

            return !topicExists;
        }

        /// <summary>
        /// Create a Topic Subscription, if it does not already exist, and indicate if the attempt was successful.
        /// </summary>
        public bool CreateSubscription(string subscriptionName)
        {
            bool subscriptionCreated = false;

            if (!string.IsNullOrWhiteSpace(subscriptionName))
            {
                string topicName = EnvironmentType.AzureTopicName();

                if (CreateTopic())
                {
                    subscriptionCreated = _namespaceManager.SubscriptionExistsAsync(topicName, subscriptionName).Result;
                    if (!subscriptionCreated)
                    {
                        _namespaceManager.CreateSubscriptionAsync(topicName, subscriptionName).Wait();
                        subscriptionCreated = _namespaceManager.SubscriptionExistsAsync(topicName, subscriptionName).Result;
                    }
                }
            }

            return subscriptionCreated;
        }

        /// <summary>
        /// Delete a Topic Subscription, if it exists, and indicate if the attempt was successful.
        /// </summary>
        public bool DeleteSubscription(string subscriptionName)
        {
            bool subscriptionDeleted = false;

            if (!string.IsNullOrWhiteSpace(subscriptionName))
            {
                string topicName = EnvironmentType.AzureTopicName();

                if (TopicExists)
                {
                    subscriptionDeleted = !_namespaceManager.SubscriptionExistsAsync(topicName, subscriptionName).Result;
                    if (!subscriptionDeleted)
                    {
                        _namespaceManager.DeleteSubscriptionAsync(topicName, subscriptionName).Wait();
                        subscriptionDeleted = !_namespaceManager.SubscriptionExistsAsync(topicName, subscriptionName).Result;
                    }
                }
                else
                {
                    subscriptionDeleted = true;
                }
            }

            return subscriptionDeleted;
        }

        #endregion


        #region Private Methods

        private void SetAzureServiceManager(EnvironmentType environment)
        {
            _namespaceManager = NamespaceManager.CreateFromConnectionString(environment.AzureServiceConnectionString());
        }

        #endregion
    }
}
