using Microsoft.AspNetCore.Identity;
using ARCHIVE.COMMON.DTOModels;
using ARCHIVE.COMMON.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DATABASE.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetUsersAsync();
        Task<UserDTO> GetUserAsync(string userId);
        Task<UserDTO> GetUserByEmailAsync(string email);
        Task<UserDTOWithPicture> GetUserByEmailWithPictureAsync(string email);
        Task<KeyValuePair<string, IdentityResult>> AddUserAsync(RegisterUserDTO user);
        Task<KeyValuePair<string, IdentityResult>> AddUserAsync(UserDTO user);
        Task<bool> UpdateUserAsync(UserDTO user);
        Task<bool> UpdateUserWithPictureAsync(UserDTOWithPicture user);
        Task<bool> DeleteUserAsync(string userId);
        Task<AppUser> GetUserAsync(LoginUserDTO loginUser, bool includeClaims = false);
        List<UserDTO> GetUsersByClient(int id);
        List<UserDTOWithPicture> GetUsersWithPictureByClient(int id);
        List<UserDTOWithPicture> GetUsersByOrganization(int id, int orgid);
    }
}
