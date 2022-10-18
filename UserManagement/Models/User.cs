using System;
using LinqToDB.Mapping;

namespace UserManagement.Models
{
    [Table(Name = "users")]
    public record User
    {

        public User(Guid userId, string friendlyName, string email, DateTime created)
        {
            UserId = userId;
            FriendlyName = friendlyName;
            Email = email;
            Created = created;
        }

        // Global user identifier
        [PrimaryKey] [Column(Name = "userid")] public Guid UserId { get; set; }

        // Friendly name - ex: "John Doe"
        [Column(Name = "friendlyname")] public string FriendlyName { get; set; }

        // Email address - in non email-password auth, extract from third party on signup
        [Column(Name = "email")] public string Email { get; set; }

        // Date created
        [Column(Name = "created")] public DateTime Created { get; set; }
    }
}