using System;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Windows.Forms;
using Nemiro.OAuth;
using Neuron.Esb.OAuth;

namespace Neuron.Esb.Samples
{
    [DisplayName("Azure Client Credentials Grant OAuth Provider")]
    public class AzureClientCredentialsOAuthProvider : OAuthProvider
    {
        private string clientId;
        private string clientSecret;
        private string tenant;
        private string resource;

        [DisplayName("Client ID")]
        [Description("The Application Id assigned to your app when you registered it with Azure AD.")]
        [PropertyOrder(2)]
        public string ClientId
        {
            get { return this.clientId; }
            set
            {
                this.clientId = value;
            }
        }

        [DisplayName("Client Secret")]
        [PasswordPropertyText(true)]
        [Description("The application secret that you created in the app registration portal for your app.")]
        [PropertyOrder(3)]
        [EncryptValue]
        public string ClientSecret
        {
            get { return this.clientSecret; }
            set
            {
                this.clientSecret = value;
            }
        }

        [DisplayName("Tenant")]
        [Description("The {tenant} value in the path of the request can be used to control who can sign into the application.  The Tenant can be found by logging into the Azure Active Directory Portal as an administrator, click on Active Directory, click the directory you want to authenticate with, and the tenant will be displayed as part of the URL: https://manage.windowsazure.com/tenantname#Workspaces/ActiveDirectoryExtension/Directory/<TenantID>/directoryQuickStart")]
        [PropertyOrder(4)]
        public string Tenant
        {
            get { return this.tenant; }

            set
            {
                this.tenant = value;
            }
        }

        [DisplayName("Resource")]
        [Description("The URL of the resource you want to access.")]
        [PropertyOrder(6)]
        public string Resource
        {
            get { return this.resource; }

            set
            {
                this.resource = value;
            }
        }

        public override OAuthBase GetClient()
        {
            var authorizeUrl = string.Format("https://login.windows.net/{0}/oauth2/authorize", this.tenant);
            var accessTokenUrl = string.Format("https://login.windows.net/{0}/oauth2/token", this.tenant);
            return new AzureClientCredentialsGrantOAuth2Client(authorizeUrl, accessTokenUrl, this.clientId, this.clientSecret, this.resource);
        }

        public override AccessToken ClientLogin(System.Windows.Forms.Form mainForm)
        {
            bool success = false;

            try
            {
                var client = this.GetClient();
                var token = client.AccessTokenValue;
                if (!String.IsNullOrEmpty(token))
                    success = true;

                if (success)
                {
                    MessageBox.Show(mainForm, "Azure Client Credentials OAuth Test Successfull", "Success");
                    return client.AccessToken;
                }
                else
                {
                    MessageBox.Show(mainForm, "Unable to obtain an access token - unknown error", "Test Failed");
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(mainForm, String.Format("Unable to obtain an access token - {0}", ex.Message), "Test Failed");
            }

            return null;
        }
    }

    public class AzureClientCredentialsGrantOAuth2Client : OAuth2Client
    {
        private string resource;

        public override string ProviderName
        {
            get { return "Azure Client Credentials Grant OAuth Provider"; }
        }

        public AzureClientCredentialsGrantOAuth2Client(string authorizeUrl, string accessTokenUrl, string clientId, string clientSecret, string resource)
            : base(authorizeUrl, accessTokenUrl, clientId, clientSecret)
        {
            this.resource = resource;
            base.SupportRefreshToken = false;
        }

        protected override void GetAccessToken()
        {
            var parameters = new NameValueCollection
            {
                { "grant_type", GrantType.ClientCredentials },
                { "client_id", this.ApplicationId },
                { "client_secret", this.ApplicationSecret },
                { "resource", this.resource },
            };

            var result = OAuthUtility.Post
            (
              endpoint: this.AccessTokenUrl,
              parameters: parameters
            );

            if (result.ContainsKey("error"))
            {
                this.AccessToken = new OAuth2AccessToken(new ErrorResult(result));
            }
            else
            {
                this.AccessToken = new OAuth2AccessToken(result);
            }
        }
    }
}
