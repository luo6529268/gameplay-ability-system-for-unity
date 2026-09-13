using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using NTSD.Animation;

internal static class PngCorpusVerifier
{
    private static string Hash(byte[] bytes)
    {
        using (SHA256 hash = SHA256.Create())
            return BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", string.Empty);
    }

    private static int Main(string[] args)
    {
        if (args.Length != 1)
            throw new ArgumentException("Expected a PNG reference TSV manifest.");
        int passed = 0;
        int failed = 0;
        using (var reader = new StreamReader(args[0]))
        {
            if (reader.ReadLine() != "path\tinputSha256\twidth\theight\trgbaBottomUpSha256")
                throw new InvalidDataException("Wrong reference manifest header.");
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] fields = line.Split('\t');
                if (fields.Length != 5)
                    throw new InvalidDataException("Wrong reference manifest row shape.");
                string result;
                try
                {
                    byte[] bytes = File.ReadAllBytes(fields[0]);
                    if (Hash(bytes) != fields[1])
                        throw new InvalidDataException("Input hash changed after reference capture.");
                    if (!PngPixelDecoder.TryDecode(bytes, out int width, out int height, out byte[] rgba, out string error))
                        throw new InvalidDataException(error);
                    if (width != int.Parse(fields[2], CultureInfo.InvariantCulture) ||
                        height != int.Parse(fields[3], CultureInfo.InvariantCulture) || Hash(rgba) != fields[4])
                        throw new InvalidDataException("Decoded dimensions or RGBA hash differs from Pillow reference.");
                    passed++;
                    result = "PASS";
                }
                catch (Exception error)
                {
                    failed++;
                    result = "FAIL " + error.Message.Replace('\t', ' ').Replace('\n', ' ');
                }
                Console.WriteLine(result + "\t" + fields[0]);
            }
        }
        Console.WriteLine($"ACTUAL_DECODER_SOURCE_VS_PILLOW passed={passed} failed={failed}");
        return failed == 0 && passed > 0 ? 0 : 1;
    }
}
