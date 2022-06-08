using System.Collections.Generic;
using ARCHIVE.COMMON.Entities;
using System.Linq;

namespace ARCHIVE.COMMON.Utilities
{
    public static class SessionManager
    {
        private static List<SessionUser> _sessions = new List<SessionUser>();

        public static void RegisterLogin(SessionUser user)
        {
            if (user != null)
            {
                _sessions.RemoveAll(u => u.UserName == user.UserName);
                _sessions.Add(user);
            }
        }

        public static void DeregisterLogin(SessionUser user)
        {
            if (user != null)
                _sessions.RemoveAll(u => u.UserName == user.UserName && u.SessionId == user.SessionId);
        }

        public static bool ValidateCurrentLogin(AppUser User, string SessionID)
        {
            if (User == null || User.ResetPassword)
            {
                return false;
            }
            if (User.Blocked)
            {
                return false;
            }
            if (!_sessions.Any(u => u.UserName ==  User.Id && u.SessionId == SessionID))
            {
                return false;
            }
            return true;
        }
    }

    public class SessionUser
    {
        public string UserName { get; set; }
        public string SessionId { get; set; }
    }
}