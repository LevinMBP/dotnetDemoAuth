using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DemoAuth.Models.Accounts;

namespace DemoAuth.Interface
{
    public interface ITokenService
    {
        string GenerateAccessToken(AppUser user);
        // Generate and return a refresh token, store it in the database
        Task<string> GenerateAndStoreRefreshTokenAsync(AppUser user);

        // Verify the provided refresh token against the stored hash (used for validation)
        Task<bool> VerifyRefreshTokenAsync(string incomingRefreshToken);

        // Revoke the old refresh token (mark as used or expired) and issue a new one
        Task<string> RotateRefreshTokenAsync(string oldRefreshToken);

        // Revoke a specific refresh token (make it invalid)
        Task RevokeRefreshTokenAsync(string refreshToken);

        // Fetch a refresh token entity from the database (for management or other purposes)
        Task<RefreshToken> GetRefreshTokenEntityAsync(string refreshToken);

        // Optionally, you can add other methods like retrieving the refresh token from DB, etc.
    }
}