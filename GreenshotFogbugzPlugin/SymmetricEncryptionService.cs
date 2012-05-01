using System;
using System.IO;
using System.Security.Cryptography;

namespace GreenshotFogbugzPlugin
{
    public class SymmetricEncryptionService
    {
        // create and initialize a crypto algorithm
        private static SymmetricAlgorithm getAlgorithm(string password)
        {
            SymmetricAlgorithm algorithm = Rijndael.Create();
            Rfc2898DeriveBytes rdb = new Rfc2898DeriveBytes(
                password, new byte[] {
                    0x21,0x3f,0x12,0x2A,0x7a,0x6e,0x10,
                    0x4A,0x18,0x6D,0x69,0x72,0x39,0xA1,0x35
                }
            );
            algorithm.Padding = PaddingMode.ISO10126;
            algorithm.Key = rdb.GetBytes(32);
            algorithm.IV = rdb.GetBytes(16);
            return algorithm;
        }

        public static string Encrypt(string password, string clearText)
        {
            SymmetricAlgorithm algorithm = getAlgorithm(password);
            byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(clearText);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, algorithm.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(clearBytes, 0, clearBytes.Length);
                    cs.Close();
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public static string Decrypt(string password, string cipherText)
        {
            SymmetricAlgorithm algorithm = getAlgorithm(password);
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, algorithm.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                    cs.Close();
                }
                return System.Text.Encoding.Unicode.GetString(ms.ToArray());
            }
        }
    }
}
