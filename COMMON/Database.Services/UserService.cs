using Microsoft.AspNetCore.Identity;
using ARCHIVE.COMMON.DTOModels;
using ARCHIVE.COMMON.Entities;
using DATABASE.Context;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ARCHIVE.COMMON.Extensions;
using ARCHIVE.COMMON.Servises;

namespace DATABASE.Services
{
    public class UserService : IUserService
    {
        private readonly SearchServiceDBContext _dbContext;
        private readonly UserManager<AppUser> _userManager;

        public UserService(SearchServiceDBContext dbContext, UserManager<AppUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<UserDTO> GetUserAsync(string userId)
        {
            return await _dbContext.Users
                .Select(user => new UserDTO
                {
                    Id = user.Id,
                    Email = user.Email,
                    Blocked = user.Blocked,
                    //Client = _dbContext.Clients.FirstOrDefault(c => c.Id == _dbContext.AppUsers.FirstOrDefault(u => u.UserId == user.Id).ClientId),
                    ClientId = !_dbContext.AppUsers.Any(u => u.UserId == user.Id) ? -1 : _dbContext.Clients.FirstOrDefault(c => c.Id == _dbContext.AppUsers.FirstOrDefault(u => u.UserId == user.Id).ClientId).Id,
                    UserName = user.UserName,
                    DisplayName = user.DisplayName,
                    Password = string.Empty,
                    ClientAdmin = _dbContext.UserRoles.Any(ur => ur.UserId.Equals(user.Id) && ur.RoleId == _dbContext.Roles.FirstOrDefault(r => r.Name.Equals("ClientAdmin")).Id),
                    EnDocsAdmin = _dbContext.UserRoles.Any(ur => ur.UserId.Equals(user.Id) && ur.RoleId == _dbContext.Roles.FirstOrDefault(r => r.Name.Equals("EndocsAdmin")).Id),
                    Demo = _dbContext.UserRoles.Any(ur => ur.UserId.Equals(user.Id) && ur.RoleId == _dbContext.Roles.FirstOrDefault(r => r.Name.Equals("Demo")).Id),
                }).FirstOrDefaultAsync(u => u.Id.Equals(userId));
        }

        public async Task<UserDTO> GetUserByEmailAsync(string email)
        {
            return await _dbContext.Users
                .Select(user => new UserDTO
                {
                    Id = user.Id,
                    Email = user.Email,
                    Blocked = user.Blocked,
                    //Client = _dbContext.Clients.FirstOrDefault(c => c.Id == _dbContext.AppUsers.FirstOrDefault(u => u.UserId == user.Id).ClientId),
                    ClientId = !_dbContext.AppUsers.Any(u => u.UserId == user.Id) ? -1 : _dbContext.Clients.FirstOrDefault(c => c.Id == _dbContext.AppUsers.FirstOrDefault(u => u.UserId == user.Id).ClientId).Id,
                    UserName = user.UserName,
                    DisplayName = user.DisplayName,
                    Password = string.Empty,
                    ClientAdmin = _dbContext.UserRoles.Any(ur => ur.UserId.Equals(user.Id) && ur.RoleId == _dbContext.Roles.FirstOrDefault(r => r.Name.Equals("ClientAdmin")).Id),
                    EnDocsAdmin = _dbContext.UserRoles.Any(ur => ur.UserId.Equals(user.Id) && ur.RoleId == _dbContext.Roles.FirstOrDefault(r => r.Name.Equals("EndocsAdmin")).Id),
                    Demo = _dbContext.UserRoles.Any(ur => ur.UserId.Equals(user.Id) && ur.RoleId == _dbContext.Roles.FirstOrDefault(r => r.Name.Equals("Demo")).Id),
                    LastLogin = user.LastLogin
                }).FirstOrDefaultAsync(u => u.Email.Equals(email));
        }

        public async Task<UserDTOWithPicture> GetUserByEmailWithPictureAsync(string email)
        {
            return await _dbContext.Users
                .Select(user => new UserDTOWithPicture
                {
                    Id = user.Id,
                    ClientId = !_dbContext.AppUsers.Any(u => u.UserId == user.Id) ? -1 : _dbContext.Clients.FirstOrDefault(c => c.Id == _dbContext.AppUsers.FirstOrDefault(u => u.UserId == user.Id).ClientId).Id,
                    Email = user.Email,
                    Blocked = user.Blocked,
                    DisplayName = user.DisplayName,
                    Picture = user.Picture
                }).FirstOrDefaultAsync(u => u.Email.Equals(email));
        }

        public async Task<IEnumerable<UserDTO>> GetUsersAsync()
        {
            return await _dbContext.Users
                .OrderBy(u => u.Email)
                .Select(user => new UserDTO
                {
                    Id = user.Id,
                    Email = user.Email,
                    Blocked = user.Blocked,
                    //Client = _dbContext.Clients.FirstOrDefault(c => c.Id == _dbContext.AppUsers.FirstOrDefault(u => u.UserId == user.Id).ClientId),
                    ClientId = !_dbContext.AppUsers.Any(u => u.UserId == user.Id) ? -1 : _dbContext.Clients.FirstOrDefault(c => c.Id == _dbContext.AppUsers.FirstOrDefault(u => u.UserId == user.Id).ClientId).Id,
                    UserName = user.UserName,
                    DisplayName = user.DisplayName,
                    Password = string.Empty,
                    ClientAdmin = _dbContext.UserRoles.Any(ur => ur.UserId.Equals(user.Id) && ur.RoleId == _dbContext.Roles.FirstOrDefault(r => r.Name.Equals("ClientAdmin")).Id),
                    EnDocsAdmin = _dbContext.UserRoles.Any(ur => ur.UserId.Equals(user.Id) && ur.RoleId == _dbContext.Roles.FirstOrDefault(r => r.Name.Equals("EndocsAdmin")).Id),
                    Demo = _dbContext.UserRoles.Any(ur => ur.UserId.Equals(user.Id) && ur.RoleId == _dbContext.Roles.FirstOrDefault(r => r.Name.Equals("Demo")).Id),
                    LastLogin = user.LastLogin,
                    //ClientAdmin = _dbContext.UserRoles.Any(ur => ur.UserId.Equals(user.Id) && ur.RoleId.Equals("35626c48-f1cb-43cb-9cd6-9a4597b0eadc")),
                    //EnDocsAdmin = _dbContext.UserRoles.Any(ur => ur.UserId.Equals(user.Id) && ur.RoleId.Equals("f8003bb0-3872-4790-845a-d4a0f119bc60")),
                    Roles = null// _dbContext.Roles.Where(r => r.Id == _dbContext.UserRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId).Select(x => x.Id).ToList()
                }).ToListAsync();
        }

        public List<UserDTO> GetUsersByClient(int id)
        {
            var uu = _dbContext.Users.Join(_dbContext.AppUsers, u => u.Id, c => c.UserId, (u, c)
             => new UserDTO
             {
                 Id = u.Id,
                 Email = u.Email,
                 Blocked = u.Blocked,
                 DisplayName = u.DisplayName,
                 UserName = u.UserName,
                 Position = u.Position,
                 Phone = u.Phone,
                 ExtPhone = u.ExtPhone,
                 ClientId = c.ClientId.HasValue ? c.ClientId.Value : 0,
                 ClientAdmin = _dbContext.UserRoles.Any(ur => ur.UserId.Equals(u.Id) && ur.RoleId == _dbContext.Roles.FirstOrDefault(r => r.Name.Equals("ClientAdmin")).Id)
             }).Where(x => x.ClientId == id).ToList();
            return uu;
        }

        public List<UserDTOWithPicture> GetUsersByOrganization(int id, int orgid)
        {
            var uu = _dbContext.Users.AsNoTracking().Join(_dbContext.AppUsers, u => u.Id, c => c.UserId, (u, c)
             => new UserDTOWithPicture
             {
                 Id = u.Id,
                 Email = u.Email,
                 Blocked = u.Blocked,
                 DisplayName = u.DisplayName,
                 UserName = u.UserName,
                 Picture = u.Picture,
                 Position = u.Position,
                 ClientId = c.ClientId.HasValue ? c.ClientId.Value : 0,
                 ClientAdmin = _dbContext.UserRoles.Any(ur => ur.UserId.Equals(u.Id) && ur.RoleId == _dbContext.Roles.FirstOrDefault(r => r.Name.Equals("ClientAdmin")).Id)
             }).Where(x => x.ClientId == id);
            var orgPerms = _dbContext.OrgPerms.AsNoTracking().Where(r => r.OrganizationId == orgid).Select(x => x.User);
            return uu.Where(x => !orgPerms.Contains(x.Email)).ToList();
        }

        public List<UserDTOWithPicture> GetUsersWithPictureByClient(int id)
        {
            var uu = _dbContext.Users.Join(_dbContext.AppUsers, u => u.Id, c => c.UserId, (u, c)
             => new UserDTOWithPicture
             {
                 Id = u.Id,
                 Email = u.Email,
                 Blocked = u.Blocked,
                 DisplayName = u.DisplayName,
                 UserName = u.UserName,
                 Picture = u.Picture,
                 Position = u.Position,
                 ClientId = c.ClientId.HasValue ? c.ClientId.Value : 0,
                 ClientAdmin = _dbContext.UserRoles.Any(ur => ur.UserId.Equals(u.Id) && ur.RoleId == _dbContext.Roles.FirstOrDefault(r => r.Name.Equals("ClientAdmin")).Id)
             }).Where(x => x.ClientId == id).ToList();
            return uu;
        }

        public async Task<KeyValuePair<string, IdentityResult>> AddUserAsync(RegisterUserDTO user)
        {
            var dbUser = new AppUser { UserName = user.Email, Email = user.Email, EmailConfirmed = true };
            var result = await _userManager.CreateAsync(dbUser, user.Password);
            KeyValuePair<string, IdentityResult> res = new KeyValuePair<string, IdentityResult>(dbUser.Id, result);
            return res;
        }

        public async Task<KeyValuePair<string, IdentityResult>> AddUserAsync(UserDTO user)
        {
            var dbUser = new AppUser
            {
                DisplayName = user.DisplayName,
                UserName = user.Email,
                Email = user.Email,
                EmailConfirmed = true,
                Phone = user.Phone,
                Position = user.Position,
                Blocked = user.Blocked
            };
            var result = await _userManager.CreateAsync(dbUser, user.Password);
            KeyValuePair<string, IdentityResult> res = new KeyValuePair<string, IdentityResult>(dbUser.Id, result);
            return res;
        }

        public async Task<bool> UpdateUserAsync(UserDTO user)
        {
            var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id.Equals(user.Id));
            if (dbUser == null)
                return false;
            if (string.IsNullOrEmpty(user.Email))
                return false;
            dbUser.Email = user.Email;
            dbUser.Blocked = user.Blocked;
            dbUser.UserName = user.Email;
            dbUser.DisplayName = user.DisplayName;
            dbUser.Phone = user.Phone;
            dbUser.ExtPhone = user.ExtPhone;
            dbUser.Position = user.Position;

            var result = await _dbContext.SaveChangesAsync();
            return result >= 0;
        }

        public async Task<bool> UpdateUserWithPictureAsync(UserDTOWithPicture user)
        {
            var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id.Equals(user.Id));
            if (dbUser == null)
                return false;
            if (string.IsNullOrEmpty(user.Email))
                return false;
            dbUser.Email = user.Email;
            dbUser.UserName = user.Email;
            dbUser.DisplayName = user.DisplayName;
            dbUser.Position = user.Position;
            dbUser.Phone = user.Phone;
            dbUser.ExtPhone = user.ExtPhone;
            dbUser.Picture = user.Picture;

            var result = await _dbContext.SaveChangesAsync();
            return result >= 0;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                var dbUser = await _userManager.FindByIdAsync(userId);
                if (dbUser == null)
                    return true;
                var userRoles = await _userManager.GetRolesAsync(dbUser);
                var roleRemoved = await _userManager.RemoveFromRolesAsync(dbUser, userRoles);
                var deleted = await _userManager.DeleteAsync(dbUser);
                return deleted.Succeeded;
            }
            catch
            {
                return false;
            }
        }

        public async Task<AppUser> GetUserAsync(LoginUserDTO loginUser, bool includeClaims = false)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginUser.Email);
                if (user == null)
                    return null;
                if (loginUser.Password.IsNullOrEmptyOrWhiteSpace() &&
                    loginUser.PasswordHash.IsNullOrEmptyOrWhiteSpace())
                    return null;
                if (loginUser.Password.Length > 0)
                {
                    var password = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, loginUser.Password);
                    if (password == PasswordVerificationResult.Failed)
                        return null;
                }
                else
                {
                    if (!user.PasswordHash.Equals(loginUser.PasswordHash))
                        return null;
                }
                if (includeClaims)
                    user.Claims = await _userManager.GetClaimsAsync(user);
                return user;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
