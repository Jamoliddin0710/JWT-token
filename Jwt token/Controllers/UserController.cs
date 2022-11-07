using Jwt_token.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Jwt_token.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly string FilePath;
        private readonly string Key;

        private readonly UserService _userService;
        public UserController(IOptions<JsonSettings> options, UserService userService)
        {
            FilePath = options.Value.FilePath;
            Key = options.Value.Key;
            _userService = userService;

        }

        [HttpGet]
        [Authorize]
        public IActionResult GetUser()
        {
            return Ok("Data user");
        }


        [HttpPost]
        public IActionResult SignUp(User user)
        {
          _userService.Users.Add(user);
          SaveUser(_userService.Users);
            var keybyte = System.Text.Encoding.UTF8.GetBytes(Key);
            var securitykey = new SigningCredentials(new SymmetricSecurityKey(keybyte), SecurityAlgorithms.HmacSha256);
            var security = new JwtSecurityToken(
                issuer: "Project",
                audience: "Room",
                new Claim[]
                {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.MobilePhone, user.MobilePhone),
                new Claim("Password",user.Password)
                },
               expires: DateAndTime.Now.AddMinutes(20),
               signingCredentials: securitykey);

            var token = new JwtSecurityTokenHandler().WriteToken(security);

            return Ok(token);
        }

        [Authorize]
        [HttpGet("sigIn")]
        public IActionResult SignIn([FromForm]UserDto userDto)
        {
            var user = _userService.Users.FirstOrDefault(user => user.Token == userDto.Token);
            if (user == null)
                return Unauthorized();
            return Ok(user);
        }

        [Authorize]
        [HttpGet("get users")]
        public IActionResult GetUsers()
        {
            return Ok(_userService.Users);
        }
        private void SaveUser(List<User> users)
        {
            string? jsondata = JsonConvert.SerializeObject(users);
            System.IO.File.WriteAllText(FilePath, jsondata);
        }
        private List<User>? ReadUser()
        {
            if (!System.IO.File.Exists(FilePath))
            {
                return null;
            }

            var jsondata = System.IO.File.ReadAllText(FilePath);
            var users = JsonConvert.DeserializeObject<List<User>>(jsondata);

            return users;
        }
    }
}
