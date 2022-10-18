using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DayTrip.Shared.UserManagement
{
    public class LoginModel
    {
        
        [Required(ErrorMessage = "Please enter your email")]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Please enter a password")]
        [PasswordPropertyText]
        public string Password { get; set; }


    }
}