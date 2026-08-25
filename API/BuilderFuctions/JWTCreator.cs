using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Domain.DTO;
using Microsoft.IdentityModel.Tokens;

namespace API.BuilderFuctions;

internal static class JWTCreator
{
    internal static void SetupJWT(this WebApplicationBuilder builder)
    {
        string key = CreateJWTKey();
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "TrDB-sys",
                    ValidAudience = "TrDB-usr",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                };
            });
    }
    
    
    
    static string CreateJWTKey()
    {
        byte[] keyBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(keyBytes);
        }
        string secretKey = Convert.ToBase64String(keyBytes);
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", secretKey);
        return secretKey;
    }
}