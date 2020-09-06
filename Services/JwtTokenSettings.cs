using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GCloud.Services
{
    public class JwtTokenSettings
    {
        const string securityAlgorithm = SecurityAlgorithms.HmacSha256Signature;

        private string _key;
        public string Key 
        { 
            get { return _key; } 
            set 
            {
                _key = value;
                SecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
                Credentials = new SigningCredentials(SecurityKey, securityAlgorithm);
            } 
        }
        public SymmetricSecurityKey SecurityKey { get; private set; }
        public SigningCredentials Credentials { get; private set; }


        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int TimeSpanSeconds { get; set; }

        public string GenToken(IdentityUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserName.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                }),

                Expires = DateTime.UtcNow.AddSeconds(TimeSpanSeconds),
                SigningCredentials = Credentials,
                Audience = Audience,
                Issuer = Issuer
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
