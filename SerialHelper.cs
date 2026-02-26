using System;
using System.Security.Cryptography;

namespace SistemaPintoSalinas
{
    public static class SerialHelper
    {
        private const string CHARSET = "ABCDEFGHJKMNPQRSTUVWXYZ23456789"; // sin I,O,0,1 para menos confusión

        public static string GenerateSerial(int length = 12)
        {
            var bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            char[] chars = new char[length];
            for (int i = 0; i < length; i++) chars[i] = CHARSET[bytes[i] % CHARSET.Length];
            return new string(chars);
        }

        public static string HashSerial(string serial)
        {
            // Reuse Utils PBKDF2 format
            return Utils.CreatePasswordHash(serial);
        }

        public static bool VerifyHashedSerial(string storedHash, string providedSerial)
        {
            bool needsUpgrade;
            return Utils.VerifyPassword(storedHash, providedSerial, out needsUpgrade);
        }
    }
}
