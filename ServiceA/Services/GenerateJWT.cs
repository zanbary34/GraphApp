using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ServiceA.Services
{
    public class GenerateJWT
    {
        public string GenerateJwtToken(IConfiguration configuration)
        {
            var secretKey = configuration["JwtSettings:SecretKey"];


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "serviceA",
                audience: "serviceB",
                claims: new[] { new Claim("role", "ServiceA") },
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
