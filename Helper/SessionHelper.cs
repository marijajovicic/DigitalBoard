using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalBoard.Helper
{
    public static class SessionHelper
    {
        public static readonly string UsernameKey = "Session.Username";
        public static readonly string UserIdKey = "Session.UserIdKey";
        public static bool IsUsernameEmpty(ISession session)
        {
            return string.IsNullOrEmpty(session.GetString(UsernameKey));
        }

        public static void SetUsername(ISession session, string username)
        {
            session.SetString(UsernameKey, username);
        }

        public static void SetUserId(ISession session, int id)
        {
            session.SetInt32(UserIdKey, id);
        }

        public static string GetUsername(ISession session)
        {
            return session.GetString(UsernameKey);
        }

        public static int? GetUserId(ISession session)
        {
            return session.GetInt32(UserIdKey);
        }
    }
}
