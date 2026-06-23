using BCrypt.Net;
using ConnectHub.Application.DTO_s.Auth;
using ConnectHub.Application.Interfaces;
using ConnectHub.Application.Settings;
using ConnectHub.Domain.Entities;
using ConnectHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _dbContext;
        private readonly JWTSettings _jWTSettings;

        public AuthService(AppDbContext dbContext,IOptions<JWTSettings> JWTSettings)
        {
            _dbContext = dbContext;
            _jWTSettings = JWTSettings.Value;
        }
        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email)
                ?? throw new Exception("Invalid email or password");
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                throw new Exception("Invalid email or password");

            return await GenerateAuthResponse(user);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u=>u.RefreshToken==refreshToken)
                ?? throw new Exception("Invalid refresh token");

            if (user.RefreshTokenExpiry < DateTime.UtcNow)
                throw new Exception("Refresh token has expired , please log in again");

            return await GenerateAuthResponse(user);
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            if (await _dbContext.Users.AnyAsync(u => u.Email == registerDto.Email))
                throw new Exception("Email already in use");

            if (await _dbContext.Users.AnyAsync(u => u.UserName == registerDto.UserName))
                throw new Exception("User name taken");

            var user = new User
            {
                Email = registerDto.Email,
                UserName = registerDto.UserName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password)
            };

            await _dbContext.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            return await GenerateAuthResponse(user);
            
        }

        private static string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }
        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jWTSettings.SecretKey));
            var Credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var Claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Name,user.UserName),
                new Claim(ClaimTypes.Email,user.Email)
            };

            var Token = new JwtSecurityToken(
                issuer: _jWTSettings.Issuer,
                audience: _jWTSettings.Audience,
                claims: Claims,
                expires: DateTime.UtcNow.AddMinutes(_jWTSettings.ExpiryMinutes),
                signingCredentials: Credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(Token);
        }

        private async Task<AuthResponseDto> GenerateAuthResponse(User user)
        {
            var accessToken = GenerateJwtToken(user);
            var refreshToken= GenerateRefreshToken();
            var expiry = DateTime.UtcNow.AddDays(_jWTSettings.RefreshTokenExpiryDays);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = expiry;
            await _dbContext.SaveChangesAsync();

            return new AuthResponseDto()
            {
                AccessToken = accessToken,
                AccessTokenExpiry = expiry,
                Email = user.Email,
                RefreshToken = refreshToken,
                UserName = user.UserName
            };
        }
    }
}
