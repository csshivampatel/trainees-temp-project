using System;
using System.Runtime.Serialization;


namespace Neuron.Samples.Soap.Headers
{
    /// <summary>
    /// Represents a username and password for a Login
    /// </summary>
    public interface ICredentials
    {
        /// <summary>
        /// The username
        /// </summary>
        string Username { get; set; }

        /// <summary>
        /// The password
        /// </summary>
        string Password { get; set; }
    }

    /// <summary>
    /// Represents a security token that can be reused after credentials have been authenticated
    /// </summary>
    public interface ISecurityToken
    {
        /// <summary>
        /// A token returned by the Authentication method. The string must be a valid guid.
        /// </summary>
        string Token { get; set; }

        /// <summary>
        /// Provides access to the token as a Guid
        /// </summary>
        Guid TokenAsGuid { get; set; }
    }

    /// <summary>
    /// Custom SOAP header object to support the platform security infrastructure
    /// </summary>
    [DataContract(Name = "SecurityHeader", Namespace = "http://www.neuronesb.com/samples/soapheaders")]
    public class SecurityHeader : ICredentials, ISecurityToken
    {
        /// <summary>
        /// Session token.  String representation of a Guid.
        /// </summary>
        [DataMember(Order = 0, IsRequired = false)]
        public string Token { get; set; }

        /// <summary>
        /// The active User's login Username
        /// </summary>
        [DataMember(Order = 1, IsRequired = false)]
        public string Username { get; set; }

        /// <summary>
        /// The User's password
        /// </summary>
        [DataMember(Order = 2, IsRequired = false)]
        public string Password { get; set; }

        /// <summary>
        /// The service name for routing
        /// </summary>
        [DataMember(Order = 3, IsRequired = false)]
        public string Service { get; set; }


        /// <summary>
        /// Convenience method to get the token string as a Guid instead
        /// </summary>
        [IgnoreDataMember]
        public Guid TokenAsGuid
        {
            get { return String.IsNullOrEmpty(Token) ? Guid.Empty : new Guid(Token); }
            set { Token = value.ToString(); }
        }
   
    }
}