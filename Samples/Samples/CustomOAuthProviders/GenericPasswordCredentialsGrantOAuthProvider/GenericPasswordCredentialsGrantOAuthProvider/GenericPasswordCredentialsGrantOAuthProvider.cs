using System;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Windows.Forms;
using Nemiro.OAuth;
using Neuron.Esb.OAuth;

namespace Neuron.Esb.Samples
{
    [DisplayName("Generic Password Credentials Grant OAuth Provider")]
    public class GenericPasswordCredentialsGrantOAuthProvider : OAuthProvider
    {
        private string tokenUrl;
        private string clientId;
        private string clientSecret;
        private string username;
        private string password;
        private string scope;

        [DisplayName("Token Url")]
        [Description("The Url used to retrieve an access token.")]
        [PropertyOrder(2)]
        public string TokenUrl
        {
            get { return this.tokenUrl; }
            set
            {
                this.tokenUrl = value;
            }
        }

        [DisplayName("Client Id")]
        [Description("The Client Id assigned to your app.")]
        [PropertyOrder(3)]
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
        [Description("The Client Secret assigned to your app.")]
        [PropertyOrder(4)]
        [EncryptValue]
        public string ClientSecret
        {
            get { return this.clientSecret; }
            set
            {
                this.clientSecret = value;
            }
        }

        [DisplayName("Username")]
        [Description("The user account.")]
        [PropertyOrder(5)]
        public string Username
        {
            get { return this.username; }

            set
            {
                this.username = value;
            }
        }

        [DisplayName("Password")]
        [PasswordPropertyText(true)]
        [Description("The password of the user account.")]
        [PropertyOrder(6)]
        [EncryptValue]
        public string Password
        {
            get { return this.password; }

            set
            {
                this.password = value;
            }
        }

        [DisplayName("Scope")]
        [Description("A list of scope values, separated by spaces. For more information about scopes, please refer to the OAuth provider's documentation.")]
        [PropertyOrder(7)]
        public string Scope
        {
            get { return this.scope; }
            set
            {
                this.scope = value;
            }
        }

        public override OAuthBase GetClient()
        {
            return new GenericPasswordCredentialsGrantOAuth2Client(tokenUrl, this.clientId, this.clientSecret, this.username, this.password, this.scope);
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
                    MessageBox.Show(mainForm, "Azure Resource Owner Password Credentials OAuth Test Successful", "Success");
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

    public class GenericPasswordCredentialsGrantOAuth2Client : OAuth2Client
    {
        public override string ProviderName
        {
            get { return "Generic Password Credentials Grant OAuth Provider"; }
        }

        public GenericPasswordCredentialsGrantOAuth2Client(string accessTokenUrl, string clientId, string clientSecret, string username, string password, string scope)
            : base(accessTokenUrl, accessTokenUrl, clientId, clientSecret)
        {
            base.Username = username;
            base.Password = password;
            base.Scope = scope;
            base.SupportRefreshToken = false;
        }

        protected override void GetAccessToken()
        {
            var parameters = new NameValueCollection
            {
                { "grant_type", GrantType.Password },
                { "client_id", base.ApplicationId },
                { "username", base.Username },
                { "password", base.Password }
            };

            if (!String.IsNullOrEmpty(base.ApplicationSecret))
                parameters.Add("client_secret", base.ApplicationSecret);

            if (base.Scope != null)
                parameters.Add("scope", base.Scope);

            var result = OAuthUtility.Post
            (
              endpoint: this.AccessTokenUrl,
              parameters: parameters,
              authorization: new HttpAuthorization(AuthorizationType.Basic, OAuthUtility.ToBase64String("{0}:{1}", this.ApplicationId, this.ApplicationSecret))
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
