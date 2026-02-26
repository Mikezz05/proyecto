using System;

namespace SistemaPintoSalinas
{
    public static class SessionManager
    {
        public static int UserId { get; set; } = 0;
        public static string CurrentUser { get; set; } = "SYSTEM";
        public static string Role { get; set; } = "";
    }
}
