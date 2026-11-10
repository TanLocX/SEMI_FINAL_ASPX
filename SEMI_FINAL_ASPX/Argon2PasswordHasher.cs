using System;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace SEMI_FINAL_ASPX
{
    /// <summary>
    /// Xử lý băm và kiểm tra mật khẩu bằng thuật toán Argon2id (Chuẩn OWASP cao nhất).
    /// Hỗ trợ định dạng PHC (Password Hashing Competition) string.
    /// </summary>
    public static class Argon2PasswordHasher
    {
        private const int MemorySizeKb = 65536; // 64 MB
        private const int Iterations = 3;
        private const int DegreeOfParallelism = 2;
        private const int SaltSizeBytes = 16;
        private const int HashSizeBytes = 32;

        /// <summary>
        /// Băm mật khẩu người dùng với muối ngẫu nhiên (Salt).
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            byte[] salt = new byte[SaltSizeBytes];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            using (var argon2 = new Argon2id(passwordBytes))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = DegreeOfParallelism;
                argon2.Iterations = Iterations;
                argon2.MemorySize = MemorySizeKb;

                byte[] hash = argon2.GetBytes(HashSizeBytes);

                string saltBase64 = Convert.ToBase64String(salt);
                string hashBase64 = Convert.ToBase64String(hash);

                // Định dạng chuẩn PHC
                return $"$argon2id$v=19$m={MemorySizeKb},t={Iterations},p={DegreeOfParallelism}${saltBase64}${hashBase64}";
            }
        }

        /// <summary>
        /// Kiểm tra tính hợp lệ của mật khẩu so với chuỗi hash trong CSDL.
        /// </summary>
        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
                return false;

            // 1. Nếu là chuỗi Argon2id
            if (storedHash.StartsWith("$argon2id$", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    string[] parts = storedHash.Split('$');
                    // Định dạng: "" (0), "argon2id" (1), "v=19" (2), "m=...,t=...,p=..." (3), salt (4), hash (5)
                    if (parts.Length < 6)
                        return false;

                    string paramPart = parts[3];
                    string saltBase64 = parts[4];
                    string expectedHashBase64 = parts[5];

                    int memory = MemorySizeKb;
                    int iterations = Iterations;
                    int parallelism = DegreeOfParallelism;

                    foreach (var param in paramPart.Split(','))
                    {
                        var kv = param.Split('=');
                        if (kv.Length == 2)
                        {
                            if (kv[0] == "m") int.TryParse(kv[1], out memory);
                            else if (kv[0] == "t") int.TryParse(kv[1], out iterations);
                            else if (kv[0] == "p") int.TryParse(kv[1], out parallelism);
                        }
                    }

                    byte[] salt = Convert.FromBase64String(saltBase64);
                    byte[] expectedHash = Convert.FromBase64String(expectedHashBase64);

                    byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                    using (var argon2 = new Argon2id(passwordBytes))
                    {
                        argon2.Salt = salt;
                        argon2.DegreeOfParallelism = parallelism;
                        argon2.Iterations = iterations;
                        argon2.MemorySize = memory;

                        byte[] computedHash = argon2.GetBytes(expectedHash.Length);
                        return SlowEquals(computedHash, expectedHash);
                    }
                }
                catch
                {
                    return false;
                }
            }

            // 2. Tương thích ngược: Mật khẩu chưa băm (Plain-text)
            return string.Equals(password, storedHash, StringComparison.Ordinal);
        }

        /// <summary>
        /// So sánh 2 mảng byte bằng thời gian cố định (Constant-time) để chống tấn công Timing Attack.
        /// </summary>
        private static bool SlowEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }
    }
}
