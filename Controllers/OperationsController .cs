using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskMongoDb.Data;
using TaskMongoDb.Models;

namespace TaskMongoDb.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class OperationsController : Controller
    {
        private UserManager<ApplicationUser> userManager;
        private readonly IConfiguration configuration;

        public OperationsController(UserManager<ApplicationUser> userManager ,IConfiguration configuration)
        {
            this.userManager = userManager;
            this.configuration = configuration;
        }


       /// <summary>
       /// register user
       /// </summary>
       /// <param name="user"></param>
       /// <returns></returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser appUser = new ApplicationUser
                {
                    UserName = user.Name,
                    Email = user.Email
                };

                IdentityResult result = await userManager.CreateAsync(appUser, user.Password);
                if (result.Succeeded)
                {
                    return new OkObjectResult("user created");
                }
                else
                {
                    return new BadRequestObjectResult(result.Errors);
                }
            }
            return View(user);
        }




        /// <summary>
        /// login user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login(Login loginValues)
        {
            var user =await userManager.FindByEmailAsync(loginValues.Email);
            var result = await userManager.CheckPasswordAsync(user, loginValues.Password);
            if (user == null)
            {
                return new BadRequestObjectResult("invalid credentials");
            }
            else
            {
                if (await userManager.CheckPasswordAsync(user, loginValues.Password))
                {
                    var userName = user.UserName != null ? user.UserName : "";
                    var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, userName),
                        new Claim("UserId",user.Id.ToString()),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    };

                    var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));
                    _ = int.TryParse(configuration["JWT:TokenValidityInMinutes"], out int tokenValidityInMinutes);

                    var token = new JwtSecurityToken(
                                issuer: configuration["JWT:ValidIssuer"],
                                audience: configuration["JWT:ValidAudience"],
                                expires: DateTime.Now.AddMinutes(tokenValidityInMinutes),
                                claims: authClaims,
                                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                              );

                    var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
                    return new OkObjectResult(accessToken);
                }
                else
                {
                    return new BadRequestObjectResult("invalid credentials");
                }
            }
        }

    }
}
