using System;
using System.IO;
using System.Text;

namespace NTSD.DatParser
{
    /// <summary>
    /// LF2 Dat 文件解密工具
    /// 用于读取加密的 .dat 文件
    /// </summary>
    public static class Lf2DatDecryptor
    {
        /// <summary>
        /// 解密 dat 文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="encryptionKey">解密密钥（如果文件加密）</param>
        /// <returns>解密后的文本内容</returns>
        public static string DecryptFile(string filePath, string encryptionKey = null)
        {
            if (!File.Exists(filePath))
            {
                UnityEngine.Debug.LogError($"文件不存在: {filePath}");
                return string.Empty;
            }

            byte[] buffer = File.ReadAllBytes(filePath);

            if (buffer.Length == 0)
                return string.Empty;

            // 检测是否已是明文（与 LF2.IDE IsPlaintext 逻辑一致）
            int plainStart = (buffer.Length >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF) ? 3 : 0;
            string plainHead = Encoding.ASCII.GetString(buffer, plainStart, Math.Min(64, buffer.Length - plainStart));
            if (plainHead.TrimStart().StartsWith("<bmp_begin>") ||
                plainHead.TrimStart().StartsWith("<frame>") ||
                plainHead.TrimStart().StartsWith("<stage>"))
                return Encoding.UTF8.GetString(buffer, plainStart, buffer.Length - plainStart);

            // 按照 LF2.IDE 的逻辑：如果提供了密钥就解密，否则直接返回
            if (string.IsNullOrEmpty(encryptionKey))
            {
                return Encoding.ASCII.GetString(buffer);
            }

            // 有密钥，执行解密
            // LF2 加密格式：前 123 字节跳过，后面的内容用密钥解密
            int len = Math.Max(0, buffer.Length - 123);
            byte[] decrypted = new byte[len];

            for (int i = 0, j = 123; i < len; i++, j++)
            {
                unchecked
                {
                    decrypted[i] = (byte)(buffer[j] - (byte)encryptionKey[i % encryptionKey.Length]);
                }
            }

            return Encoding.ASCII.GetString(decrypted);
        }

    }
}
