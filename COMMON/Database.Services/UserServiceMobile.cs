// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using COMMON.Common.Services.ContextService;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using static DATABASE.Services.IUserServiceMobile;

namespace DATABASE.Services
{
    public class UserServiceMobile : IUserServiceMobile
    {
        public static string KEY = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("SecurityKey")["SymmetricSecurityKey"];
        private readonly IContextService _context;

        public UserServiceMobile(IContextService contextService)
        {
            _context = contextService;
        }
        public UserEntities AuthUser(string username)
        {
            var user = _context.DbContext.Users.FirstOrDefault(u=>u.UserName == username);
            if (user == null)
            {
                return null;
            }

            var userEntity = new UserEntities();

            List<Claim> lstClaim = new List<Claim>(); //CLAIM USER INFO
            lstClaim.Add(new Claim(ClaimTypes.Name, user.UserName));

            //CREATE JWT TOKEN
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(KEY);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(lstClaim.ToArray()),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)

            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            userEntity.Token = tokenHandler.WriteToken(token);
            //userEntity.Picture = user.Picture;

            return userEntity;
        }
    }
}
