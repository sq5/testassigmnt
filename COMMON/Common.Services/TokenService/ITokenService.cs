using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ARCHIVE.COMMON.DTOModels;
using ARCHIVE.COMMON.DTOModels.Admin;

namespace COMMON.Common.Services.TokenService
{
    public interface ITokenService
    {
        TokenDTO GenerateToken(string clientName);
        Task<TokenDTO> GetTokenAsync(int clientId);
        Task<string> CheckToken(string token, ClientDTO client);
        (bool res, string exType) ValidateToken(string authToken);
    }
}
