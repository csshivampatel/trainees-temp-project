using System;
using System.ComponentModel;
using System.Windows.Forms;
using Nemiro.OAuth;
using Nemiro.OAuth.LoginForms;
using Neuron.Esb.OAuth;

namespace Neuron.Esb.Samples
{
    [DisplayName("Generic Authorization Code Grant OAuth Provider")]
    public class GenericAuthCodeGrantOAuthProvider : OAuthProvider
    {
        private string clientId;
        private string clientSecret;
        private string authorizationUrl;
        private string tokenUrl;
        private string redirectUri;
        private string scope;

        [DisplayName("Client ID")]
        [Description("The identifier of the application that was provided by the OAuth provider.")]
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
        [Description("The shared secret value that is used to authenticate requests between Neuron ESB and the remote OAuth provider.")]
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

        [DisplayName("Authorization Url")]
        [Description("The authorization Url for the OAuth provider")]
        [PropertyOrder(5)]
        public string AuthorizationUrl
        {
            get { return this.authorizationUrl; }
            set
            {
                this.authorizationUrl = value;
            }
        }

        [DisplayName("Token Url")]
        [Description("The Url used to retrieve an access token.")]
        [PropertyOrder(6)]
        public string TokenUrl
        {
            get { return this.tokenUrl; }
            set
            {
                this.tokenUrl = value;
            }
        }

        [DisplayName("Redirect Url")]
        [Description("The redirect Url that was registered for the application with the OAuth provider.")]
        [PropertyOrder(7)]
        public string RedirectUri
        {
            get { return this.redirectUri; }
            set
            {
                this.redirectUri = value;
            }
        }

        [DisplayName("Scope")]
        [Description("A list of scope values, separated by spaces. For more information about scopes, please refer to the OAuth provider's documentation.")]
        [PropertyOrder(9)]
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
            return new GenericAuthCodeGrantOAuth2Client(this.authorizationUrl, this.tokenUrl, this.redirectUri, this.clientId, this.clientSecret, this.scope);
        }

        public override AccessToken ClientLogin(Form mainForm)
        {
            GenericAuthCodeGrantLogin login = null;

            try
            {
                var client = this.GetClient();
                login = new GenericAuthCodeGrantLogin(client);

                using (login)
                {
                    login.ShowDialog(mainForm);
                    if (login.IsSuccessfully)
                    {
                        return client.AccessToken;
                    }
                    else
                    {
                        MessageBox.Show(mainForm, String.Format("Unable to obtain an access token - {0}", login.ErrorMessage), "Test Failed");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(mainForm, String.Format("Unable to obtain an access token - {0}", ex.Message), "Test Failed");
            }

            return null;
        }
    }

    public class GenericAuthCodeGrantOAuth2Client : OAuth2Client
    {
        public override string ProviderName
        {
            get { return "Generic Authorization Code Grant OAuth Provider"; }
        }

        public GenericAuthCodeGrantOAuth2Client(string authUri, string tokenUri, string redirectUri, string clientId, string clientSecret, string scope)
            : base(authUri, tokenUri, clientId, clientSecret)
        {
            base.ReturnUrl = redirectUri;
            base.SupportRefreshToken = true;
            base.Scope = scope;
        }
    }

    public class GenericAuthCodeGrantLogin : Login
    {
        public GenericAuthCodeGrantLogin(OAuthBase client) :
            base(client, responseType: "code")
        {
        }
    }
}
