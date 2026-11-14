using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AyuUI.Controll.Helpers
{
    public static class EncryptHelper
    {
        // 默认 32 位密钥（生产环境中建议存入配置或由外部传入）
        private static readonly byte[] DefaultAesKey = Encoding.UTF8.GetBytes("AyuUI_Framework_Secret_Key_2026#");
        // 默认 16 位偏移量
        private static readonly byte[] DefaultAesIV = Encoding.UTF8.GetBytes("AyuUI_Secure_IV#");

        #region AES 对称加解密

        /// <summary>
        /// AES 加密文本（返回 Base64 密文字符串）
        /// </summary>
        public static string AesEncrypt(string plainText, string? secretKey = null)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;

            byte[] keyBytes = secretKey != null ? GetValidKeyBytes(secretKey, 32) : DefaultAesKey;
            byte[] ivBytes = DefaultAesIV;

            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.IV = ivBytes;

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                using var sw = new StreamWriter(cs, Encoding.UTF8);
                sw.Write(plainText);
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        /// AES 解密文本
        /// </summary>
        public static string AesDecrypt(string cipherText, string? secretKey = null)
        {
            if (string.IsNullOrEmpty(cipherText)) return string.Empty;

            try
            {
                byte[] keyBytes = secretKey != null ? GetValidKeyBytes(secretKey, 32) : DefaultAesKey;
                byte[] ivBytes = DefaultAesIV;

                using var aes = Aes.Create();
                aes.Key = keyBytes;
                aes.IV = ivBytes;

                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs, Encoding.UTF8);
                return sr.ReadToEnd();
            }
            catch
            {
                return string.Empty; // 解密失败返回空
            }
        }

        private static byte[] GetValidKeyBytes(string key, int desiredLength)
        {
            var data = Encoding.UTF8.GetBytes(key);
            if (data.Length == desiredLength) return data;

            var result = new byte[desiredLength];
            Array.Copy(data, result, Math.Min(data.Length, desiredLength));
            return result;
        }

        #endregion

        #region 摘要算法 (MD5 / SHA256)

        /// <summary>
        /// 计算文本的 32 位大写 MD5 值
        /// </summary>
        public static string ToMd5(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            var bytes = Encoding.UTF8.GetBytes(text);
            var hashBytes = MD5.HashData(bytes);

            var sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 计算文本的 SHA256 哈希值
        /// </summary>
        public static string ToSha256(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            var bytes = Encoding.UTF8.GetBytes(text);
            var hashBytes = SHA256.HashData(bytes);

            var sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }

        #endregion
    }
}
