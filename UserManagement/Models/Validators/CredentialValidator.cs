using FluentValidation;
using LinqToDB;
using UserManagement.DataAccess;

namespace UserManagement.Models.Validators
{
    internal class CredentialValidator : AbstractValidator<Credential>
    {
        public CredentialValidator(UserDataConnection connection)
        {
            RuleFor(x => x.Identifier).NotEmpty().NotNull();
            RuleFor(x => x.Secret).NotEmpty().NotNull();
            RuleFor(x => x.CredentialId).NotEmpty().NotNull()

                // Ensures no other credential in the database has the same
                // credential ID by querying the database if another exists.
                .MustAsync(
                    async (guid, token) =>
                        await connection.Credentials.FirstOrDefaultAsync(
                            c => c.CredentialId == guid
                        ) == null
                );
        }
    }
}