using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Model;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;


namespace Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private IConfiguration _config;

        public LoginController(IConfiguration config)
        {
            this._config = config;
        }

        [HttpGet]
        public IActionResult Login(string username, string userpass){
                UserModel login = new UserModel();
                login.UserName = username;
                login.UserPass = userpass;
                IActionResult response = Unauthorized();

                var user = AuthenticateUser(login);
                if(user != null){
                    var tokenStr = GenerateJSONWebToken(user);
                    response = Ok(new {token=tokenStr});
                }
                return response;
        }

        private UserModel AuthenticateUser(UserModel login)
        {
            UserModel user = null;
            //For demo I am using static info
            //In real world, DB Context can be used here
            if(login.UserName == "ashproghelp"
                && login.UserPass == "123")
                {
                    user = new UserModel()
                    {
                        UserName = "AshProgHelp",
                        EmailAddr = "demo@ashproghelp.com",
                        UserPass = "123"
                    };
                }
            return user;
        }

        private string GenerateJSONWebToken(UserModel userinfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new []
            {
                new Claim(JwtRegisteredClaimNames.Sub, userinfo.UserName),
                new Claim(JwtRegisteredClaimNames.Email, userinfo.EmailAddr),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            var encodetoken = new JwtSecurityTokenHandler().WriteToken(token);
            return encodetoken;
        }

        [Authorize]
        [HttpPost("Post")]
        public string Post()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            IList<Claim> claim = identity.Claims.ToList();
            var userName = claim[0].Value;
            return "Welcome to: " + userName;
        }
        
        [Authorize]
        [HttpGet("GetValue")]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] {"Value1", "Value2", "Value3"};
        }
    }
    
}