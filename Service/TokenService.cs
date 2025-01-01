using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DemoAuth.Data;
using DemoAuth.Interface;
using DemoAuth.Models.Accounts;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace DemoAuth.Service
{
    public class TokenService : ITokenService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly SymmetricSecurityKey _key;
        private readonly int _refreshTokenExpiration;
        private readonly int _accessTokenExpiration;
        public TokenService(
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SigningKey"]));
            _refreshTokenExpiration = _configuration.GetValue<int>("JWT:RefreshTokenExpiry", 5);
            _accessTokenExpiration = _configuration.GetValue<int>("JWT:AccessTokenExpiry", 5);
        }

        // Generates JWT Access Token
        public string GenerateAccessToken(AppUser user)
        {

            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, user.Id.ToString()),
                new (ClaimTypes.Email, user.Email),
                new ("OrganizationId", user.OrganizationId.ToString()),
                new (ClaimTypes.Role, user.EmployeeType),
                new (JwtRegisteredClaimNames.Sub, user.Email),
                new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_accessTokenExpiration),
                NotBefore = DateTime.UtcNow,
                SigningCredentials = creds,
                Issuer = _configuration["JWT:Issuer"],
                Audience = _configuration["JWT:Audience"],
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }


        public async Task<string> GenerateAndStoreRefreshTokenAsync(AppUser user)
        {
            // Generate a secure random refreshtoken
            var refreshToken = GenerateRefreshToken();

            // Hash the token before storing it in the database
            var hashedToken = HashRefreshToken(refreshToken);

            // Store the refresh token in the database
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = hashedToken,
                ExpirationDate = DateTime.UtcNow.AddDays(_refreshTokenExpiration),
                Revoked = false
            };

            await _context.RefreshTokens.AddAsync(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return refreshToken;
        }

        // Rotate refresh token (issue a new one)
        public async Task<string> RotateRefreshTokenAsync(string oldRefreshToken)
        {
            // Hash the incoming refresh token before checking in the database
            var oldTokenHash = HashRefreshToken(oldRefreshToken);

            // Invalidate the old refresh token
            var oldToken = _context.RefreshTokens.FirstOrDefault(t => t.Token == oldTokenHash && !t.Revoked);
            if (oldToken == null || oldToken.ExpirationDate < DateTime.UtcNow) return null;


            // Mark the old refresh token as revoked
            oldToken.Revoked = true;
            await _context.SaveChangesAsync();


            var user = await _context.Users.FindAsync(oldToken.UserId);
            if (user == null) return null;

            // Issue and store a new refresh token
            return await GenerateAndStoreRefreshTokenAsync(user);
        }

        public async Task<bool> VerifyRefreshTokenAsync(string incomingRefreshToken)
        {
            // Hash the incoming token
            var incomingTokenHash = HashRefreshToken(incomingRefreshToken);

            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rf => rf.Token == incomingTokenHash && !rf.Revoked);

            if (refreshToken == null || refreshToken.ExpirationDate < DateTime.UtcNow)
            {
                await RevokeRefreshTokenAsync(incomingRefreshToken);
                return false;
            }
            return true;
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            var tokenHash = HashRefreshToken(refreshToken);
            Console.WriteLine($"Hashed Token: {tokenHash}");

            // Find and revoke the refresh token
            var tokenEntity = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == tokenHash);

            if (tokenEntity != null)
            {
                // Check if already revoked
                if (tokenEntity.Revoked)
                {
                    Console.WriteLine("Token is already revoked.");
                    return;  // Token is already revoked, no action needed.
                }
                tokenEntity.Revoked = true;
                await _context.SaveChangesAsync();
                Console.WriteLine("Successfully revoked token");
                return;
            }
            else
            {
                Console.WriteLine("Attempted to revoke a token that does not exist.");
            }

        }

        public async Task<RefreshToken> GetRefreshTokenEntityAsync(string refreshToken)
        {
            var tokenHash = HashRefreshToken(refreshToken);
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == tokenHash);
        }

        // Helper methods for generating and hashing tokens

        // Generates Refresh Token
        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            return Convert.ToBase64String(randomNumber);
        }

        private static string HashRefreshToken(string refreshToken)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(refreshToken));
            return Convert.ToBase64String(hashBytes);
        }
    }
}