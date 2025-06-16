using Google.Apis.Auth;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TJobs.DTOs.Requests;

namespace TJobs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public AccountsController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _context = context;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            //ApplicationUser applicationUser = new()
            //{
            //    Email = registerRequest.Email,
            //    UserName = registerRequest.UserName,
            //    Address = registerRequest.Address,
            //    Age = registerRequest.Age
            //};

            ApplicationUser applicationUser = registerRequest.Adapt<ApplicationUser>();

            var result = await _userManager.CreateAsync(applicationUser, registerRequest.Password);

            if (result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);

                var confirmationLink = Url.Action("ConfirmEmail", "Accounts", new { userId = applicationUser.Id, token }, Request.Scheme);

                await _emailSender.SendEmailAsync(registerRequest.Email, "Confirmation Your Account", $"Please Confirm Your Account By Clicking <a href='{confirmationLink}'>Here</a>");

                await _userManager.AddToRoleAsync(applicationUser, SD.Customer);

                return Created();
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var applicationUser = await _userManager.FindByEmailAsync(loginRequest.EmailOrUserName);
            ModelStateDictionary keyValuePairs = new();

            if (applicationUser is null)
            {
                applicationUser = await _userManager.FindByNameAsync(loginRequest.EmailOrUserName);
            }

            if (applicationUser is not null)
            {
                if (applicationUser.LockoutEnabled)
                {

                    var result = await _userManager.CheckPasswordAsync(applicationUser, loginRequest.Password);

                    if (result)
                    {
                        // Login
                        await _signInManager.SignInAsync(applicationUser, loginRequest.RememberMe);

                        var roles = await _userManager.GetRolesAsync(applicationUser);

                        var claims = new[]
                        {
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(ClaimTypes.Name, applicationUser!.UserName ?? "none"),
                            new Claim(ClaimTypes.NameIdentifier, applicationUser.Id),
                            new Claim(ClaimTypes.Role, String.Join(",", roles))
                        };

                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("EraaSoft##EraaSoft##EraaSoft##EraaSoft##EraaSoft##"));
                        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var token = new JwtSecurityToken(
                            claims: claims,
                            expires: DateTime.UtcNow.AddHours(3),
                            signingCredentials: creds
                            );

                        return Ok(new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token)
                        });
                    }
                    else
                    {
                        keyValuePairs.AddModelError("EmailOrUserName", "Invalid Email Or User Name");
                        keyValuePairs.AddModelError("Password", "Invalid Password");
                    }

                }
                else
                {
                    keyValuePairs.AddModelError("Error", $"You Have Block Until {applicationUser.LockoutEnd}");
                }
            }
            else
            {
                keyValuePairs.AddModelError("EmailOrUserName", "Invalid Email Or User Name");
                keyValuePairs.AddModelError("Password", "Invalid Password");
            }

            return BadRequest(keyValuePairs);
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return NoContent();
        }

        [HttpPost("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var applicationUser = await _userManager.FindByIdAsync(userId);

            if (applicationUser is not null)
            {
                var result = await _userManager.ConfirmEmailAsync(applicationUser, token);

                if (result.Succeeded)
                {
                    return NoContent();
                }
                else
                {
                    return BadRequest(result.Errors);
                }

            }

            return NotFound();
        }

        [HttpPost("ResendEmail")]
        public async Task<IActionResult> ResendEmail([FromBody] ResendEmailRequest resendEmailRequest)
        {
            var applicationUser = await _userManager.FindByEmailAsync(resendEmailRequest.EmailOrUserName);
            ModelStateDictionary keyValuePairs = new();

            if (applicationUser is null)
            {
                applicationUser = await _userManager.FindByNameAsync(resendEmailRequest.EmailOrUserName);
            }

            if (applicationUser is not null)
            {

                if (!applicationUser.EmailConfirmed)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);

                    var confirmationLink = Url.Action("ConfirmEmail", "Accounts", new { userId = applicationUser.Id, token }, Request.Scheme);

                    await _emailSender.SendEmailAsync(applicationUser!.Email ?? "none", "Confirmation Your Account", $"Please Confirm Your Account By Clicking <a href='{confirmationLink}'>Here</a>");

                    return NoContent();
                }
                else
                {
                    keyValuePairs.AddModelError(string.Empty, "Already confirmed!");

                }
            }
            else
            {
                keyValuePairs.AddModelError("EmailOrUserName", "Invalid Email Or User Name");
            }

            return BadRequest(keyValuePairs);
        }

        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest forgetPasswordRequest)
        {
            var applicationUser = await _userManager.FindByEmailAsync(forgetPasswordRequest.EmailOrUserName);
            ModelStateDictionary keyValuePairs = new();

            if (applicationUser is null)
            {
                applicationUser = await _userManager.FindByNameAsync(forgetPasswordRequest.EmailOrUserName);
            }

            if (applicationUser is not null)
            {
                var code = new Random().Next(1000, 9999).ToString();

                _context.PasswordResetCodes.Add(new()
                {
                    ApplicationUserId = applicationUser.Id,
                    Code = code,
                    ExpirationCode = DateTime.UtcNow.AddHours(24)
                });

                await _emailSender.SendEmailAsync(applicationUser!.Email ?? "none", "Reset The Password", $"<h1>Please Reset Your Password Using This Code {code}");

                //var resetPasswordLink = Url.Action("ResetPassword", "Accounts", new { userId = applicationUser.Id, token, email = applicationUser.Email }, Request.Scheme);

                //await _emailSender.SendEmailAsync(applicationUser!.Email ?? "none", "Reset Password", $"Please Reset Your Account Password By Clicking <a href='{resetPasswordLink}'>Here</a>");

                return NoContent();

            }

            keyValuePairs.AddModelError("EmailOrUserName", "Invalid Email Or User Name");

            return BadRequest(keyValuePairs);
        }

        [HttpPost("ConfirmResetPassword")]
        public async Task<IActionResult> ConfirmResetPassword([FromBody] ResetPasswordRequest resetPasswordRequest)
        {
            var applicationUser = await _userManager.FindByEmailAsync(resetPasswordRequest.Email);
            ModelStateDictionary keyValuePairs = new();

            if (applicationUser != null)
            {
                var resetCode = _context.PasswordResetCodes.Where(e => e.ApplicationUserId == applicationUser.Id).OrderByDescending(e => e.ExpirationCode).FirstOrDefault();

                if (resetCode is not null && resetCode.Code == resetPasswordRequest.Code && resetCode.ExpirationCode > DateTime.UtcNow)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(applicationUser);
                    var result = await _userManager.ResetPasswordAsync(applicationUser, token, resetPasswordRequest.Password);

                    if (result.Succeeded)
                    {
                        await _emailSender.SendEmailAsync(resetPasswordRequest.Email, "Reset Password Successfully", $"Reset Password Successfully");

                        return NoContent();
                    }
                    else
                    {
                        return BadRequest(result.Errors);
                    }
                }

                keyValuePairs.AddModelError("Error In Code", "InvalidForgetPasswordCode");
                return BadRequest(keyValuePairs);
            }

            keyValuePairs.AddModelError("Email", "Invalid Email");
            return BadRequest(keyValuePairs);
        }

        [HttpPatch("LockUnLock/{id}")]
        public async Task<IActionResult> LockUnLock(string id)
        {
            var applicationUser = await _userManager.FindByIdAsync(id);

            if (applicationUser is not null)
            {
                if (!applicationUser.LockoutEnabled && applicationUser.LockoutEnd > DateTime.UtcNow)
                {
                    applicationUser.LockoutEnabled = true;
                    applicationUser.LockoutEnd = null;
                }
                else if (applicationUser.LockoutEnabled)
                {
                    applicationUser.LockoutEnabled = false;
                    applicationUser.LockoutEnd = DateTime.UtcNow.AddMonths(1);
                }

                await _userManager.UpdateAsync(applicationUser);
                return NoContent();
            }

            return NotFound();
        }

        [HttpPost("ExternalLogin")]
        public async Task<IActionResult> ExternalLogin([FromBody] ExternalAuthRequest externalAuth)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(externalAuth.IdToken);

            if (payload == null)
                return BadRequest("Invalid External Authentication");

            var info = new UserLoginInfo(externalAuth!.Provider ?? "Google", payload.Subject, externalAuth.Provider);

            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(payload.Email);

                if (user == null)
                {
                    user = new ApplicationUser { Email = payload.Email, UserName = payload.Email };
                    await _userManager.CreateAsync(user);

                    // Prepare and send an email for the email confirmation

                    await _userManager.AddToRoleAsync(user, "Customer");
                    await _userManager.AddLoginAsync(user, info); // Replace => Generate JWT
                }
                else
                {
                    await _userManager.AddLoginAsync(user, info); // Replace => Generate JWT
                }
            }

            // Check for the Locked out account
            return Ok();
        }
    }
}
