using System;
using System.IO;
using System.Text;
using NTSD.DatParser;

internal static class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("id\tfloat_bits\tstrict_int\tfirst_int\tstrict_valid");
        foreach (string line in File.ReadLines(args[0]))
        {
            string[] columns = line.Split('\t');
            string value = Encoding.UTF8.GetString(Convert.FromBase64String(columns[1]));
            uint bits = unchecked((uint)BitConverter.SingleToInt32Bits(LoganNumericDecoder.ParseFiniteFloat32OrZero(value)));
            bool valid = LoganNumericDecoder.TryParseInt32(value, out int integer);
            Console.WriteLine($"{columns[0]}\t{bits:X8}\t{integer}\t{LoganNumericDecoder.ParseFirstInt32OrZero(value)}\t{(valid ? 1 : 0)}");
        }
    }
}
