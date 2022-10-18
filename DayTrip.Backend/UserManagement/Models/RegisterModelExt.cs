using System;
using CryptoHelper;
using DayTrip.Shared.UserManagement;
using UserManagement.Models;

namespace DayTrip.Backend.UserManagement.Models
{
    public static class RegisterModelExt
    {
        private static void HashPw(this RegisterModel model)
        {
            if (!model._isHashed)
            {
                model.Password = Crypto.HashPassword(model.Password);
            }
        }
        
        public static User GetNewUser(this RegisterModel model)
        {
            return new User(Guid.NewGuid(), $"{model.FirstName} {model.LastName}", model.Email, DateTime.UtcNow);
        }

        public static EmailPassCredential GetNewCredential(this RegisterModel model, User user)
        {
            model.HashPw();

            return new EmailPassCredential(user, model.Email, model.Password);
        }
    }
}