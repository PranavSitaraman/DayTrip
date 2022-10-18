using System.ComponentModel;

namespace DayTrip.Shared.UserManagement
{
    public class RegisterModel
    {
        public bool _isHashed;
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        [PasswordPropertyText] public string Password { get; set; }

        public bool ModelsIsNull()
        {
            return FirstName == null || LastName == null || Email == null;
        }

    }
}