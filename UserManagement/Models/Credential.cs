using System;
using LinqToDB.Mapping;

namespace UserManagement.Models
{
    public enum CredentialKind
    {
        EmailPassword = 1
    }

    [Table(Name = "credentials")]
    public record Credential
    {
        [Column(Name = "credentialid")]
        public Guid CredentialId { get; set; } // ID of the specific credential for primary key

        [Column(Name = "userid")] public Guid UserId { get; set; } // User that this credential belongs to

        [Column(Name = "kind")] public CredentialKind Kind { get; set; }

        [Column(Name = "identifier")] public string Identifier { get; set; } // Email or third-party UUID

        [Column(Name = "secret")] public string Secret { get; set; } // Password or third-party secret


        public virtual string GetEmail()
        {
            throw new NotImplementedException("Please implement this method in an inherited class");
        }
    }

    public record EmailPassCredential : Credential
    {
        public EmailPassCredential()
        {
        }

        public EmailPassCredential(User owner, string email, string passwordHash)
        {
            Kind = CredentialKind.EmailPassword;
            CredentialId = Guid.NewGuid();
            UserId = owner.UserId;
            Identifier = email;
            Secret = passwordHash;
        }


        public override string GetEmail()
        {
            return Identifier;
        }
    }
}