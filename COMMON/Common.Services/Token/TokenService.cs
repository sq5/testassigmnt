// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ARCHIVE.COMMON.DTOModels;
using ARCHIVE.COMMON.DTOModels.Admin;
using ARCHIVE.COMMON.Entities;
using ARCHIVE.COMMON.Servises;
using DATABASE.Services;

namespace CloudArchive.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly IAdminService _db;

        public TokenService(IConfiguration configuration, IUserService userService,
            IAdminService db)
        {
            _configuration = configuration;
            _userService = userService;
            _db = db;
        }

        public TokenDTO GenerateToken(string clientName)
        {
            try
            {
                var token = CreateToken(clientName);
                return token;
            }
            catch
            {
                throw;
            }
        }

        public async Task<TokenDTO> GetTokenAsync(int clientId)
        {
            try
            {
                var client = await _db.SingleAsync<Client, ClientDTO>(c => c.Id.Equals(clientId));
                if (client == null)
                    throw new UnauthorizedAccessException();
                return new TokenDTO(client.Token, client.TokenExpires);
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> CheckToken(string token, ClientDTO client)
        {
            string err = string.Empty;
            try
            {
                var resValidate = ValidateToken(token);
                if (!resValidate.res)
                {
                    if (resValidate.exType.Equals("SecurityTokenExpiredException"))
                    {
                        if (client == null)
                            return err = "Could not validate token";
                        var jwt = GenerateToken(client.Name);
                        if (jwt.Token == null)
                        {
                            return err = "Could not validate token";
                        }
                        client.Token = jwt.Token;
                        client.TokenExpires = jwt.TokenExpires;
                        var res = await _db.UpdateAsync<ClientDTO, Client>(client);
                        if (!res)
                            return err = "Could not validate token";
                    }
                    else
                    {
                        return err = "Could not validate token";
                    }
                }
                return err;
            }
            catch (Exception ex)
            {
                return err = ex.Message + "StackTrace: " + ex.StackTrace;
            }
        }

        private List<Claim> GetClaims(AppUser user, bool includeUserClaims)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (includeUserClaims)
            {
                foreach (var claim in user.Claims)
                {
                    if (!claim.Type.Equals("Token") && !claim.Type.Equals("TokenExpires"))
                        claims.Add(claim);
                }
            }
            return claims;
        }

        private TokenDTO CreateToken(string clientName)
        {
            try
            {
                var claimsdata = new[] { new Claim("client", clientName), };
                var signingKey = Convert.FromBase64String(_configuration["Jwt:SigningSecret"]);
                var credentials = new SigningCredentials(
                    new SymmetricSecurityKey(signingKey),
                    SecurityAlgorithms.HmacSha256Signature);
                var duration = int.Parse(_configuration["Jwt:Duration"]);
                var now = DateTime.UtcNow;
                var jwtToken = new JwtSecurityToken(
                            issuer: "Ensol.ru",
                            audience: "Ensol.ru",
                            notBefore: now,
                            expires: now.AddDays(duration),
                            claims: claimsdata,
                            signingCredentials: credentials);
                var jwtTokenHandler = new JwtSecurityTokenHandler();
                var token = jwtTokenHandler.WriteToken(jwtToken);
                return new TokenDTO(token, jwtToken.ValidTo);
            }
            catch
            {
                throw;
            }
        }

        private async Task<bool> AddTokenToClientAsync(int clientId, TokenDTO token)
        {
            var clientDTO = await _db.SingleAsync<Client, ClientDTO>(c => c.Id == clientId);
            if (string.IsNullOrEmpty(clientDTO.Token))
            {
                TokenDTO _tokenDto = new TokenDTO(token.Token, token.TokenExpires);
                clientDTO.Token = _tokenDto.Token;
                clientDTO.TokenExpires = _tokenDto.TokenExpires;
            }

            return await _db.UpdateAsync<ClientDTO, Client>(clientDTO);
        }
        public (bool res, string exType) ValidateToken(string authToken)
        {
            string err = string.Empty;
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = this.GetValidationParameters();

            SecurityToken validatedToken;
            if (tokenHandler.CanReadToken(authToken))
            {
                ClaimsPrincipal principal;
                try
                {
                    principal = tokenHandler.ValidateToken(authToken, validationParameters, out validatedToken);
                    var success = principal != null && principal.HasClaim(c => c.Type == "client");
                    return (res: success, exType: err);
                }
                catch (Exception e)
                {
                    return (res: false, exType: e.GetType().Name);
                }
            }
            return (res: false, exType: err);
        }

        private TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(_configuration["Jwt:SigningSecret"])),
                ClockSkew = TimeSpan.Zero
            };
        }
    }
}
