using System;
using System.IO;
using System.Security.Cryptography;

namespace NTSD.Animation
{
    internal static class BattleAstcBlockData
    {
        internal const int HeaderBytes = 16;
        internal const string PixelContract = "srgb-straight-rgba-bottom-left-no-mips-v1";

        internal static int PayloadBytes(int width, int height)
        {
            if (width <= 0 || height <= 0 || width > 0xFFFFFF || height > 0xFFFFFF)
                throw new InvalidDataException("Invalid ASTC dimensions.");
            return checked((int)(((width + 3L) / 4) * ((height + 3L) / 4) * 16));
        }

        internal static void ValidateHeader(byte[] header, long fileBytes, int width, int height)
        {
            if (header == null || header.Length < HeaderBytes ||
                header[0] != 0x13 || header[1] != 0xAB || header[2] != 0xA1 || header[3] != 0x5C ||
                header[4] != 4 || header[5] != 4 || header[6] != 1 ||
                Read24(header, 7) != width || Read24(header, 10) != height || Read24(header, 13) != 1 ||
                fileBytes != (long)HeaderBytes + PayloadBytes(width, height))
                throw new InvalidDataException("ASTC header, geometry or payload length mismatch.");
        }

        internal static void ValidateFile(byte[] bytes, int width, int height, string expectedSha256)
        {
            ValidateHeader(bytes, bytes?.LongLength ?? 0, width, height);
            using (var hash = SHA256.Create())
            {
                string actual = BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", string.Empty);
                if (!string.Equals(actual, expectedSha256, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("ASTC file changed after capture.");
            }
        }

        private static int Read24(byte[] bytes, int offset)
        {
            return bytes[offset] | bytes[offset + 1] << 8 | bytes[offset + 2] << 16;
        }
    }
}
