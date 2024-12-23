using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Models.Entities;

namespace BLL
{
    public class Login
    {
        private readonly IConfiguration _configuration;

        public Login(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        //agregada salt: cabe destacar que todos los users tienen la misma salt, lo cual es un problema
        //porque si usan la misma pass, tienen el mismo hash.
        //en el futuro, deberia crear un hash unico por cada usuario creado, y que quede asociado a su cuenta
        //eso podria hacerlo agregando un row en la tabla Users donde se cree un hash unico cuando se registre la cuenta
        public string EncryptSHA256(string text)
        {
            string saltFromConfig = _configuration["Salt"];

            byte[] salt;

                salt = Convert.FromBase64String(saltFromConfig);

            byte[] textBytes = Encoding.UTF8.GetBytes(text);
            byte[] textWithSaltBytes = new byte[textBytes.Length + salt.Length];
            Buffer.BlockCopy(textBytes, 0, textWithSaltBytes, 0, textBytes.Length);
            Buffer.BlockCopy(salt, 0, textWithSaltBytes, textBytes.Length, salt.Length);

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] hashBytes = sha256Hash.ComputeHash(textWithSaltBytes);

                StringBuilder builder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    builder.Append(b.ToString("x2"));
                }

                return Convert.ToBase64String(salt) + ":" + builder.ToString();
            }
        }

            public string GenerateJWT(User usuario)
        {
            var jwtKey = _configuration["JWT:Key"];
            var jwtIssuer = _configuration["JWT:Issuer"];
            var jwtAudience = _configuration["JWT:Audience"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new ArgumentNullException("JWT:Key", "JWT Secret Key is not configured.");
            }

            var userClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.ID.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email!)
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var jwtConfig = new JwtSecurityToken(
                 issuer: jwtIssuer,
                 audience: jwtAudience,
                 claims: userClaims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(jwtConfig);
        }
    }
}