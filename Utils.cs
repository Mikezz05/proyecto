using System;
using System.Security.Cryptography;
using System.Text;

namespace SistemaPintoSalinas
{
    public static class Utils
    {
        // Legacy SHA256 helper (used only for compatibility)
        public static string ComputeSHA256(string input)
        {
            if (input == null) return null;
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        // Create a PBKDF2 password hash in the format: pbkdf2$<iter>$<salt64>$<hash64>
        public static string CreatePasswordHash(string password, int iterations = 10000)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] salt = new byte[16];
                rng.GetBytes(salt);
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
                {
                    byte[] hash = pbkdf2.GetBytes(32);
                    return string.Format("pbkdf2${0}${1}${2}", iterations, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
                }
            }
        }

        // Verify password against stored value. Supports legacy SHA256 hex and pbkdf2 format.
        public static bool VerifyPassword(string stored, string password, out bool needsUpgrade)
        {
            needsUpgrade = false;
            if (string.IsNullOrEmpty(stored)) return false;

            if (stored.StartsWith("pbkdf2$"))
            {
                try
                {
                    var parts = stored.Split('$');
                    int iter = int.Parse(parts[1]);
                    byte[] salt = Convert.FromBase64String(parts[2]);
                    byte[] hash = Convert.FromBase64String(parts[3]);
                    using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iter, HashAlgorithmName.SHA256))
                    {
                        byte[] test = pbkdf2.GetBytes(hash.Length);
                        return CryptographicOperations.FixedTimeEquals(test, hash);
                    }
                }
                catch { return false; }
            }

            // Legacy hex digests: check common algorithms (MD5, SHA1, SHA256, SHA384, SHA512)
            string md5 = HashHex("MD5", password);
            if (!string.IsNullOrEmpty(stored) && stored.Equals(md5, StringComparison.OrdinalIgnoreCase)) { needsUpgrade = true; return true; }

            string sha1 = HashHex("SHA1", password);
            if (!string.IsNullOrEmpty(stored) && stored.Equals(sha1, StringComparison.OrdinalIgnoreCase)) { needsUpgrade = true; return true; }

            string sha256 = HashHex("SHA256", password);
            if (!string.IsNullOrEmpty(stored) && stored.Equals(sha256, StringComparison.OrdinalIgnoreCase)) { needsUpgrade = true; return true; }

            string sha384 = HashHex("SHA384", password);
            if (!string.IsNullOrEmpty(stored) && stored.Equals(sha384, StringComparison.OrdinalIgnoreCase)) { needsUpgrade = true; return true; }

            string sha512 = HashHex("SHA512", password);
            if (!string.IsNullOrEmpty(stored) && stored.Equals(sha512, StringComparison.OrdinalIgnoreCase)) { needsUpgrade = true; return true; }

            return false;
        }

        private static string HashHex(string alg, string input)
        {
            if (input == null) return null;
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] h;
            switch (alg)
            {
                case "MD5": h = System.Security.Cryptography.MD5.Create().ComputeHash(bytes); break;
                case "SHA1": h = System.Security.Cryptography.SHA1.Create().ComputeHash(bytes); break;
                case "SHA256": h = System.Security.Cryptography.SHA256.Create().ComputeHash(bytes); break;
                case "SHA384": h = System.Security.Cryptography.SHA384.Create().ComputeHash(bytes); break;
                case "SHA512": h = System.Security.Cryptography.SHA512.Create().ComputeHash(bytes); break;
                default: return null;
            }
            return BitConverter.ToString(h).Replace("-", "").ToLowerInvariant();
        }
    }
}
