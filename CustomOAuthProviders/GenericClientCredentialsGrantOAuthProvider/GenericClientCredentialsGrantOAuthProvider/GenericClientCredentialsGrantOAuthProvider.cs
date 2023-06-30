using System;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Windows.Forms;
using Nemiro.OAuth;
using Neuron.Esb.OAuth;

namespace Neuron.Esb.Samples
{
    [DisplayName("Generic Client Credentials Grant OAuth Provider")]
    public class GenericClientCredentialsOAuthProvider : OAuthProvider
    {
        private string clientId;
        private string clientSecret;
        private string tokenUrl;
        private string scope;

        [DisplayName("Client ID")]
        [Description("The Application Id assigned to your app.")]
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
        [Description("The application secret assigned to your app.")]
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

        [DisplayName("Token Url")]
        [Description("The Url used to retrieve an access token.")]
        [PropertyOrder(4)]
        public string TokenUrl
        {
            get { return this.tokenUrl; }
            set
            {
                this.tokenUrl = value;
            }
        }

        [DisplayName("Scope")]
        [Description("A list of scope values, separated by spaces. For more information about scopes, please refer to the OAuth provider's documentation.")]
        [PropertyOrder(5)]
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
            return new GenericClientCredentialsGrantOAuth2Client(tokenUrl, this.clientId, this.clientSecret, this.scope);
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
                    MessageBox.Show(mainForm, "Generic Client Credentials OAuth Test Successful", "Success");
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

    public class GenericClientCredentialsGrantOAuth2Client : OAuth2Client
    {
        public override string ProviderName
        {
            get { return "Generic Client Credentials Grant OAuth Provider"; }
        }

        public GenericClientCredentialsGrantOAuth2Client(string tokenUrl, string clientId, string clientSecret, string scope)
            : base(tokenUrl, tokenUrl, clientId, clientSecret)
        {
            base.Scope = scope;
            base.SupportRefreshToken = false;
        }

        protected override void GetAccessToken()
        {
            var parameters = new NameValueCollection
            {
                { "grant_type", GrantType.ClientCredentials },
                { "client_id", this.ApplicationId },
                { "client_secret", this.ApplicationSecret }
            };

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
