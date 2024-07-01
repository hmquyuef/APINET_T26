using APINET_T26.Data;
using APINET_T26.Models.Authen;
using APINET_T26.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace APINET_T26.Controllers
{
    [Route("api/v{version:apiVersion}/auth")]
    [ApiController]
    [ApiVersion("1.0")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly APINET_T26IdentityContext _context;
        private readonly IConfiguration _config;
        public AuthController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            APINET_T26IdentityContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _config = configuration;
        }

        #region POST: /api/authen/register -> Đăng ký User
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser(InputAccount input)
        {
            var user = new IdentityUser
            {
                UserName = input.Username,
                NormalizedUserName = input.Username.ToUpper(),
                Email = input.Email,
                NormalizedEmail = input.Email.ToUpper(),
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user, input.Password);
            if (result.Succeeded)
            {
                return StatusCode(StatusCodes.Status201Created);
            }
            return StatusCode(StatusCodes.Status400BadRequest, result.Errors);
        }
        #endregion

        #region POST: /api/v1/auth/login -> Đăng nhập User
        [HttpPost("login")]
        /// <summary>
        /// 
        /// </summary>
        public async Task<IActionResult> LoginUser(InputLogin input)
        {
            //Tìm người dùng theo tên đăng nhập
            var user = await _userManager.FindByNameAsync(input.Username);
            if (user == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Không tìm thấy người dùng");
            }
            //Thực hiện đăng nhập cho người dùng và lưu thông tin Token
            var result = await _signInManager.PasswordSignInAsync(user, input.Password, false, false);

            if (result.Succeeded)
            {
                //Tìm kiếm token của người dùng
                var userToken = await _context.UserTokens
                    .Where(x => x.UserId == user.Id).FirstOrDefaultAsync();
                if (userToken != null)
                {
                    var remainingTime = GetExpiresInSecond(userToken.Value);
                    if (remainingTime > 0)
                    {
                        return StatusCode(StatusCodes.Status200OK, new OutputToken
                        {
                            Token = userToken.Value,
                            ExpiresIn = remainingTime
                        });
                    }
                    else
                    {
                        _context.UserTokens.Remove(userToken);
                        await _context.SaveChangesAsync();
                    }
                }

                //Tạo mới Token
                var token = GenerateToken(user);
                var usertoken = new IdentityUserToken<string>
                {
                    UserId = user.Id,
                    LoginProvider = "Bearer",
                    Name = "JWT",
                    Value = token
                };
                _context.UserTokens.Add(usertoken);
                await _context.SaveChangesAsync();
                return StatusCode(StatusCodes.Status200OK, new OutputToken
                {
                    Token = token,
                    ExpiresIn = int.Parse(_config["Jwt:Seconds"])
                });
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new OutputToken
            {
                Token = "",
                ExpiresIn = 0
            });
        }
        #endregion


        private int GetExpiresInSecond(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            var exp = jsonToken.Claims.First(claim => claim.Type == "exp").Value;

            var expDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp));

            var remainingTime = (int)(expDate - DateTime.UtcNow).TotalSeconds;

            return remainingTime;
        }

        private string GenerateToken(IdentityUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //var userRole = _userManager.GetRolesAsync(user).Result;

            //Khai Payload
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                //new Claim(ClaimTypes.Role, userRole.ToString()),
                new Claim(ClaimTypes.Role, "Admin"),
            };

            var token = new JwtSecurityToken(
                _config["JWT:Issuer"],
                _config["JWT:Issuer"],
                claims,
                expires: DateTime.Now.AddSeconds(int.Parse(_config["Jwt:Seconds"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
