using ARCHIVE.COMMON.DTOModels;
using ARCHIVE.COMMON.DTOModels.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudArchive.Services
{
    public interface ITokenService
    {
        TokenDTO GenerateToken(string clientName);
        Task<TokenDTO> GetTokenAsync(int clientId);
        Task<string> CheckToken(string token, ClientDTO client);
        (bool res, string exType) ValidateToken(string authToken);
    }
}
