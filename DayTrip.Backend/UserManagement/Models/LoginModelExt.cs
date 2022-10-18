using DayTrip.Shared.UserManagement;
using UserManagement.Models;

namespace DayTrip.Backend.UserManagement.Models
{
    public static class LoginModelExt
    {
        public static EmailPassCredential GetCredential(this LoginModel model)
        {
            return new EmailPassCredential(null!, model.Email, model.Password);
        }
    }
}