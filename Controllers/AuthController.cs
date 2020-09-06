using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GCloud.Data;
using GCloud.Dtos.Auth;
using GCloud.Models;
using GCloud.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GCloud.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtTokenSettings _jwtTokenSettings;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly GCloudDbContext _context;
        private readonly RsyncdSecretsManager _rsyncdSecretsManager;
        private readonly RsyncdConfManager _rsyncdConfManager;

        public AuthController(
            JwtTokenSettings jwtTokenSettings, 
            SignInManager<IdentityUser> signInManager, 
            UserManager<IdentityUser> userManager, 
            GCloudDbContext context, 
            RsyncdSecretsManager rsyncdSecretsManager,
            RsyncdConfManager rsyncdConfManager)
        {
            _jwtTokenSettings = jwtTokenSettings;
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _rsyncdSecretsManager = rsyncdSecretsManager;
            _rsyncdConfManager = rsyncdConfManager;
        }


        [HttpPost("removemodule/{module}")]
        public async Task<object> RemoveModuleAsync(string module)
        {
            return await _rsyncdConfManager.RemoveModuleAsync(module);
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

            var keyPair = new RsaKeyPair(RSA.Create()) { User = user };

            _context.RsaKeyPairs.Add(keyPair);
            await _context.SaveChangesAsync();

            if (!await _rsyncdSecretsManager.AddUserAsync(user, keyPair)) return StatusCode(500);

            if (!await _rsyncdConfManager.AddModuleAsync(user.UserName)) return StatusCode(500);

            return Ok(_jwtTokenSettings.GenToken(user));
        }

        [HttpPost("login")]
        public async Task<ActionResult> LoginAsync(LoginDto loginDto)
        {
            var result = await _signInManager.PasswordSignInAsync(loginDto.UserName, loginDto.Password, false, false);

            if (!result.Succeeded) return Forbid();
            return Ok(_jwtTokenSettings.GenToken(await _userManager.FindByNameAsync(loginDto.UserName)));

        }

        [Authorize]
        [HttpGet("publickey")]
        public async Task<ActionResult> GetPublicKeyAsync()
        {
            var keyPair = await _context.RsaKeyPairs.SingleOrDefaultAsync(c => c.User.UserName == User.Identity.Name);
            if (keyPair == null) return NotFound();

            return Ok(keyPair.ExportPemPublicKey());
        }

        [Authorize]
        [HttpGet("publicsshkey")]
        public async Task<ActionResult> GetPublicSSHKeyAsync()
        {
            var keyPair = await _context.RsaKeyPairs.SingleOrDefaultAsync(c => c.User.UserName == User.Identity.Name);
            if (keyPair == null) return NotFound();

            return Ok(keyPair.ExportOpenSSHPublicKey());
        }

        [Authorize]
        [HttpGet("privatekey")]
        public async Task<ActionResult> GetPrivatekeyAsync()
        {
            var keyPair = await _context.RsaKeyPairs.SingleOrDefaultAsync(c => c.User.UserName == User.Identity.Name);
            if (keyPair == null) return NotFound();

            return Ok(keyPair.ExportPemPrivateKey());
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
