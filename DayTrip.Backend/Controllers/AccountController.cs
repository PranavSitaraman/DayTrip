using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using DayTrip.Backend.UserManagement.Models;
using DayTrip.Shared.UserManagement;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagement;
using UserManagement.DataAccess;
using UserManagement.Models;

namespace DayTrip.Backend.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserActionManager _userActionManager;

        public AccountController([FromServices] UserDataConnection connection)
        {
            _userActionManager = new(connection);
        }

        [HttpGet]
        [Route("Account/CheckLogin")]
        public async Task<IActionResult> CheckLogin()
        {
            Guid.TryParse(User.FindFirstValue(ClaimTypes.Sid), out Guid userId);
            return Json(userId != Guid.Empty);
        }

        [HttpPost]
        [Route("Account/Register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
        {
            // Gets a new Credential and User object from the model
            var user = registerModel.GetNewUser();
            var credential = registerModel.GetNewCredential(user);

            RegisterResult result = await _userActionManager.RegisterNewUser(user, credential);

            switch (result)
            {
                case RegisterResult.Success:
                    return Ok();
                case RegisterResult.DatabaseConflict:
                    return Conflict();
                case RegisterResult.ModelValidationError:
                    return BadRequest();
                default:
                    return Forbid();
            }
        }

        [HttpPost]
        [Route("Account/Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            Credential cred = new EmailPassCredential()
            {
                Kind = CredentialKind.EmailPassword,
                Identifier = model.Email,
                Secret = model.Password
            };
            var result = await _userActionManager.LoginUser(cred);

            switch (result)
            {
                case LoginResult.NotExists:
                    return NoContent();
                case LoginResult.Failure:
                    return BadRequest();
            }

            var successResult = result as LoginResult.Success;

            Debug.Assert(successResult != null, nameof(successResult) + " != null");
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Sid, successResult.User.UserId.ToString())
            };
            ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new(identity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(480)
                });

            return Ok();
        }

        [HttpGet]
        [Route("Account/EmailTaken")]
        public async Task<IActionResult> EmailTaken([FromQuery] string email)
        {
            var user = await _userActionManager.GetUserByEmail(email);
            return Json(!(user == null));
        }

        [HttpGet]
        [Route("Account/GetName")]
        public async Task<IActionResult> GetName()
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.Sid), out Guid userId))
            {
                return Json(null);
            }

            var user = await _userActionManager.GetUserById(userId);
            Console.WriteLine(user);
            if (user == null)
            {
                return Json(null);
            }

            return Json(user.FriendlyName);
        }

        [Authorize("UserOnly")]
        [HttpGet]
        [Route("Account/GetName/{id}")]
        public async Task<IActionResult> GetName(Guid id)
        {
            var user = await _userActionManager.GetUserById(id);
            Console.WriteLine(user);
            if (user == null)
            {
                return Json(null);
            }

            return Json(user.FriendlyName);
        }
    }
}