using System;
using System.Drawing;
using System.Windows.Forms;

namespace ASB_ManageAzureServicesGetFees
{
    public partial class Form1 : Form
    {
        #region Properties

        private string ImageNameGood => "CheckMarkGreen";
        private string ImageNameBad => "Cancel";
        private bool IsDeleting { get; set; }
        private bool IsWorking { get; set; }

        #endregion


        #region Constructor

        public Form1()
        {
            InitializeComponent();
        }

        #endregion


        #region Events

        private void btnCreateServicesDEV_Click(object sender, EventArgs e)
        {
            HandleCreateServices_Click(EnvironmentType.Development);
        }

        private void btnCopyFailuresDEV_Click(object sender, EventArgs e)
        {
            HandleRequestToCopyText(EnvironmentType.Development, false);
        }

        private void btnCopySuccessesDEV_Click(object sender, EventArgs e)
        {
            HandleRequestToCopyText(EnvironmentType.Development, true);
        }

        private void btnCreateServicesUAT_Click(object sender, EventArgs e)
        {
            HandleCreateServices_Click(EnvironmentType.UserAcceptanceTesting);
        }

        private void btnCopyFailuresUAT_Click(object sender, EventArgs e)
        {
            HandleRequestToCopyText(EnvironmentType.UserAcceptanceTesting, false);
        }

        private void btnCopySuccessesUAT_Click(object sender, EventArgs e)
        {
            HandleRequestToCopyText(EnvironmentType.UserAcceptanceTesting, true);
        }

        private void btnCreateServicesSTG_Click(object sender, EventArgs e)
        {
            HandleCreateServices_Click(EnvironmentType.Staging);
        }

        private void btnCopyFailuresSTG_Click(object sender, EventArgs e)
        {
            HandleRequestToCopyText(EnvironmentType.Staging, false);
        }

        private void btnCopySuccessesSTG_Click(object sender, EventArgs e)
        {
            HandleRequestToCopyText(EnvironmentType.Staging, true);
        }

        private void btnCreateServicesPRD_Click(object sender, EventArgs e)
        {
            HandleCreateServices_Click(EnvironmentType.Production);
        }

        private void btnCopyFailuresPRD_Click(object sender, EventArgs e)
        {
            HandleRequestToCopyText(EnvironmentType.Production, false);
        }

        private void btnCopySuccessesPRD_Click(object sender, EventArgs e)
        {
            HandleRequestToCopyText(EnvironmentType.Production, true);
        }

        private void btnDeleteServicesDEV_Click(object sender, EventArgs e)
        {
            HandleDeleteServices_Click(EnvironmentType.Development);
        }

        private void btnDeleteServicesPRD_Click(object sender, EventArgs e)
        {
            HandleDeleteServices_Click(EnvironmentType.Production);
        }

        private void btnDeleteServicesSTG_Click(object sender, EventArgs e)
        {
            HandleDeleteServices_Click(EnvironmentType.Staging);
        }

        private void btnDeleteServicesUAT_Click(object sender, EventArgs e)
        {
            HandleDeleteServices_Click(EnvironmentType.UserAcceptanceTesting);
        }

        #endregion


        #region Private Methods

        private void HandleCreateServices_Click(EnvironmentType environment)
        {
            if (IsWorking) { return; }

            IsWorking = true;
            IsDeleting = false;

            bool queueExists = false;
            bool topicExists = false;

            Cursor = Cursors.WaitCursor;
            InitializeUi(environment);

            try
            {
                AzureServicesHelper azureServices = new AzureServicesHelper(environment);

                queueExists = azureServices.CreateQueue();
                topicExists = azureServices.CreateTopic();

                if (topicExists)
                {
                    foreach (string subscriptionName in azureServices.SubscriptionNames)
                    {
                        bool subscriptionExists = azureServices.CreateSubscription(subscriptionName);

                        AddEntryToSuccessFailureList(subscriptionExists, subscriptionName, environment);
                    }
                }
                else
                {
                    foreach (string subscriptionName in azureServices.SubscriptionNames)
                    {
                        AddEntryToSuccessFailureList(false, subscriptionName, environment);
                    }
                }
            }
            catch (Exception ex)
            {
                string caption = $"Create Services for {environment.ShortName()}";
                string message = $"{ex.Message}{Environment.NewLine}{new String('*', 80)}{ex.StackTrace}";

                MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ShowQueueCreationSuccess(queueExists, environment);
                ShowTopicCreationSuccess(topicExists, environment);
                IsWorking = false;
                UpdateUiWithActionMessage(environment);
                Cursor = Cursors.Default;
            }
        }

        private void HandleDeleteServices_Click(EnvironmentType environment)
        {
            if (IsWorking) { return; }

            string prompt = "Are you certain you want to DELETE all of the services on this Azure Service Bus?";
            string caption = $"Delete Services for {environment.ShortName()}";

            if (MessageBox.Show(prompt, caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation) != DialogResult.Yes) { return; }

            IsWorking = true;
            IsDeleting = true;

            bool queueDeleted = false;
            bool topicDeleted = false;

            Cursor = Cursors.WaitCursor;
            InitializeUi(environment);

            try
            {
                AzureServicesHelper azureServices = new AzureServicesHelper(environment);

                queueDeleted = azureServices.DeleteQueue();
                topicDeleted = !azureServices.TopicExists;

                if (!topicDeleted)
                {
                    foreach (string subscriptionName in azureServices.SubscriptionNames)
                    {
                        bool subscriptionDeleted = azureServices.DeleteSubscription(subscriptionName);

                        AddEntryToSuccessFailureList(subscriptionDeleted, subscriptionName, environment);
                        Application.DoEvents();
                    }
                    topicDeleted = azureServices.DeleteTopic();
                }
                else
                {
                    foreach (string subscriptionName in azureServices.SubscriptionNames)
                    {
                        AddEntryToSuccessFailureList(true, subscriptionName, environment);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = $"{ex.Message}{Environment.NewLine}{new String('*', 80)}{ex.StackTrace}";

                MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ShowQueueCreationSuccess(queueDeleted, environment);
                ShowTopicCreationSuccess(topicDeleted, environment);
                IsWorking = false;
                UpdateUiWithActionMessage(environment);
                Cursor = Cursors.Default;
            }
        }

        private void InitializeUi(EnvironmentType environment)
        {
            UpdateUiWithQueueNameForEnvironment(environment.MessageQueueName(), environment);
            UpdateUiWithTopicNameForEnvironment(environment.AzureTopicName(), environment);
            UpdateUiWithHostForEnvironment(environment);
            UpdateUiWithPrefixForEnvironment(environment);
            UpdateUiWithActionMessage(environment);
            ClearSuccessFailureListForEnvironment(environment);
        }

        private void UpdateUiWithActionMessage(EnvironmentType environment)
        {
            string actionText = IsDeleting ? "Deleting Services" : "Creating Services";
            Color actionColor = IsDeleting ? Color.DarkRed : Color.SteelBlue;

            lblActionDEV.Text = actionText;
            lblActionUAT.Text = actionText;
            lblActionSTG.Text = actionText;
            lblActionPRD.Text = actionText;
            lblActionDEV.ForeColor = actionColor;
            lblActionUAT.ForeColor = actionColor;
            lblActionSTG.ForeColor = actionColor;
            lblActionPRD.ForeColor = actionColor;
            lblActionDEV.Visible = false;
            lblActionUAT.Visible = false;
            lblActionSTG.Visible = false;
            lblActionPRD.Visible = false;

            switch (environment)
            {
                case EnvironmentType.Development:
                    lblActionDEV.Visible = IsWorking;
                    break;
                case EnvironmentType.UserAcceptanceTesting:
                    lblActionUAT.Visible = IsWorking;
                    break;
                case EnvironmentType.Staging:
                    lblActionSTG.Visible = IsWorking;
                    break;
                case EnvironmentType.Production:
                    lblActionPRD.Visible = IsWorking;
                    break;
            }
        }

        private void HandleRequestToCopyText(EnvironmentType environment, bool copySuccesses)
        {
            string entriesList = GetSuccessListForEnvironment(environment, copySuccesses);
            string success = copySuccesses ? "Successful" : "Failed";
            string action = IsDeleting ? "deleted" : "created";
            string caption = $"{success} {action} Subscription value(s) for {environment.ShortName()}";
            string clipboardText = $"{caption}{Environment.NewLine}{entriesList}";

            Clipboard.Clear();
            Clipboard.SetText(clipboardText);

            MessageBox.Show(entriesList, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private TextBox GetControlForEnvironmentAndSuccess(EnvironmentType environment, bool isSuccess)
        {
            TextBox control = null;

            switch (environment)
            {
                case EnvironmentType.Development:
                    control = isSuccess ? txtNamesSubscriptionsSuccessfulDEV : txtNamesSubscriptionsFailuresDEV;
                    break;
                case EnvironmentType.UserAcceptanceTesting:
                    control = isSuccess ? txtNamesSubscriptionsSuccessfulSTG : txtNamesSubscriptionsFailuresSTG;
                    break;
                case EnvironmentType.Staging:
                    control = isSuccess ? txtNamesSubscriptionsSuccessfulUAT : txtNamesSubscriptionsFailuresUAT;
                    break;
                case EnvironmentType.Production:
                    control = isSuccess ? txtNamesSubscriptionsSuccessfulPRD : txtNamesSubscriptionsFailuresPRD;
                    break;
            }

            return control;
        }

        private void ShowQueueCreationSuccess(bool queueExists, EnvironmentType environment)
        {
            Image imgSuccess = queueExists ? imageList1.Images[ImageNameGood] : imageList1.Images[ImageNameBad];

            SuspendLayout();

            switch (environment)
            {
                case EnvironmentType.Development:
                    picQueueDEV.Image = imgSuccess;
                    break;
                case EnvironmentType.UserAcceptanceTesting:
                    picQueueUAT.Image = imgSuccess;
                    break;
                case EnvironmentType.Staging:
                    picQueueSTG.Image = imgSuccess;
                    break;
                case EnvironmentType.Production:
                    picQueuePRD.Image = imgSuccess;
                    break;
            }

            ResumeLayout(true);
        }

        private void ShowTopicCreationSuccess(bool topicExists, EnvironmentType environment)
        {
            Image imgSuccess = topicExists ? imageList1.Images[ImageNameGood] : imageList1.Images[ImageNameBad];

            SuspendLayout();

            switch (environment)
            {
                case EnvironmentType.Development:
                    picTopicDEV.Image = imgSuccess;
                    break;
                case EnvironmentType.UserAcceptanceTesting:
                    picTopicUAT.Image = imgSuccess;
                    break;
                case EnvironmentType.Staging:
                    picTopicSTG.Image = imgSuccess;
                    break;
                case EnvironmentType.Production:
                    picTopicPRD.Image = imgSuccess;
                    break;
            }

            ResumeLayout(true);
        }

        private void UpdateUiWithQueueNameForEnvironment(string displayValue, EnvironmentType environment)
        {
            switch (environment)
            {
                case EnvironmentType.Development:
                    txtNameQueueDEV.Text = displayValue;
                    break;
                case EnvironmentType.UserAcceptanceTesting:
                    txtNameQueueUAT.Text = displayValue;
                    break;
                case EnvironmentType.Staging:
                    txtNameQueueSTG.Text = displayValue;
                    break;
                case EnvironmentType.Production:
                    txtNameQueuePRD.Text = displayValue;
                    break;
            }
        }

        private void UpdateUiWithTopicNameForEnvironment(string displayValue, EnvironmentType environment)
        {
            switch (environment)
            {
                case EnvironmentType.Development:
                    txtNameTopicDEV.Text = displayValue;
                    break;
                case EnvironmentType.UserAcceptanceTesting:
                    txtNameTopicUAT.Text = displayValue;
                    break;
                case EnvironmentType.Staging:
                    txtNameTopicSTG.Text = displayValue;
                    break;
                case EnvironmentType.Production:
                    txtNameTopicPRD.Text = displayValue;
                    break;
            }
        }

        private void UpdateUiWithPrefixForEnvironment(EnvironmentType environment)
        {
            switch (environment)
            {
                case EnvironmentType.Development:
                    txtPrefixDev.Text = environment.ShortName();
                    break;
                case EnvironmentType.UserAcceptanceTesting:
                    txtPrefixUAT.Text = environment.ShortName();
                    break;
                case EnvironmentType.Staging:
                    txtPrefixSTG.Text = environment.ShortName();
                    break;
                case EnvironmentType.Production:
                    txtPrefixPRD.Text = environment.ShortName();
                    break;
            }
        }

        private void UpdateUiWithHostForEnvironment(EnvironmentType environment)
        {
            switch (environment)
            {
                case EnvironmentType.Development:
                    txtUriDEV.Text = environment.AzureServiceBaseUri();
                    break;
                case EnvironmentType.UserAcceptanceTesting:
                    txtUriUAT.Text = environment.AzureServiceBaseUri();
                    break;
                case EnvironmentType.Staging:
                    txtUriSTG.Text = environment.AzureServiceBaseUri();
                    break;
                case EnvironmentType.Production:
                    txtUriPRD.Text = environment.AzureServiceBaseUri();
                    break;
            }
        }

        private void ClearSuccessFailureListForEnvironment(EnvironmentType environment)
        {
            switch (environment)
            {
                case EnvironmentType.Development:
                    txtNamesSubscriptionsFailuresDEV.Clear();
                    txtNamesSubscriptionsFailuresDEV.Clear();
                    break;
                case EnvironmentType.UserAcceptanceTesting:
                    txtNamesSubscriptionsFailuresUAT.Clear();
                    txtNamesSubscriptionsFailuresUAT.Clear();
                    break;
                case EnvironmentType.Staging:
                    txtNamesSubscriptionsFailuresSTG.Clear();
                    txtNamesSubscriptionsFailuresSTG.Clear();
                    break;
                case EnvironmentType.Production:
                    txtNamesSubscriptionsFailuresPRD.Clear();
                    txtNamesSubscriptionsFailuresPRD.Clear();
                    break;
            }
        }

        private void AddEntryToSuccessFailureList(bool isSuccess, string entryValue, EnvironmentType environment)
        {
            if (!string.IsNullOrWhiteSpace(entryValue))
            {
                TextBox control = GetControlForEnvironmentAndSuccess(environment, isSuccess);

                if (control != null)
                {
                    string currentText = control.TextLength > 0 ? $"{Environment.NewLine}{entryValue}" : entryValue;

                    control.AppendText(currentText);
                }
            }
        }

        private string GetSuccessListForEnvironment(EnvironmentType environment, bool isSuccess)
        {
            TextBox control = GetControlForEnvironmentAndSuccess(environment, isSuccess);

            return control?.Text;
        }


        #endregion
    }
}
