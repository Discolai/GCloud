using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GCloud.Dtos.Auth;
using GCloud.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GCloud.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtTokenSettings _jwtTokenSettings;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AuthController(JwtTokenSettings jwtTokenSettings, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _jwtTokenSettings = jwtTokenSettings;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public ActionResult<JwtTokenSettings> GetTokenSettings()
        {
            return _jwtTokenSettings;
        }

        [HttpPost("signup")]
        public async Task<ActionResult> SignUpAsync(SignupDto signupDto)
        {
            if (signupDto.Password1 != signupDto.Password2)
            {
                ModelState.AddModelError("Password1", "The passwords must match");
                return ValidationProblem(ModelState);
            }

            var user = new IdentityUser { UserName = signupDto.UserName, Email = signupDto.Email };

            var result = await _userManager.CreateAsync(user, signupDto.Password1);

            if (!result.Succeeded)
            {
                ParseIdentityErrors(result.Errors);
                return ValidationProblem(ModelState);
            }

            return Ok(_jwtTokenSettings.GenToken(user));
        }

        [HttpPost("login")]
        public async Task<ActionResult> LoginAsync(LoginDto loginDto)
        {
            var result = await _signInManager.PasswordSignInAsync(loginDto.UserName, loginDto.Password, false, false);

            if (!result.Succeeded) return Forbid();
            return Ok(_jwtTokenSettings.GenToken(await _userManager.FindByNameAsync(loginDto.UserName)));

        }

        private void ParseIdentityErrors(IEnumerable<IdentityError> errors)
        {
            foreach (var error in errors)
            {
                switch (error.Code)
                {
                    case "DuplicateUserName":
                    case "InvalidUserName":
                        ModelState.AddModelError("UserName", error.Description);
                        break;
                    case "DuplicateEmail":
                    case "InvalidEmail":
                        ModelState.AddModelError("Email", error.Description);
                        break;
                    case "PasswordTooShort":
                    case "PasswordRequiresUniqueChars":
                    case "PasswordRequiresNonAlphanumeric":
                    case "PasswordRequiresLower":
                    case "PasswordRequiresUpper":
                    case "PasswordMismatch":
                        ModelState.AddModelError("Password1", error.Description);
                        break;
                    case "DefaultError":
                        ModelState.AddModelError("Errors", error.Description);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
