using System;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace NTSD.DatParser
{
    internal static class LoganNumericDecoder
    {
        internal static bool TryParseInt32(string text, out int value)
        {
            return TryParseInteger(text, 0, true, out value);
        }

        internal static int ParseInt32OrZero(string text)
        {
            TryParseInt32(text, out int value);
            return value;
        }

        internal static int ParseFirstInt32OrZero(string text)
        {
            int start = 0;
            if (text != null)
                while (start < text.Length && (text[start] == ' ' || text[start] == '\t')) start++;
            TryParseInteger(text, start, false, out int value);
            return value;
        }

        private static bool TryParseInteger(string text, int start, bool requireEnd, out int value)
        {
            value = 0;
            if (text == null || start >= text.Length) return false;
            bool negative = text[start] == '-';
            if (negative) start++;
            int digitsStart = start;
            uint magnitude = 0;
            uint limit = negative ? 2147483648u : 2147483647u;
            while (start < text.Length && text[start] >= '0' && text[start] <= '9')
            {
                uint digit = (uint)(text[start++] - '0');
                if (magnitude > (limit - digit) / 10) return false;
                magnitude = magnitude * 10 + digit;
            }
            if (start == digitsStart || (requireEnd && start != text.Length)) return false;
            value = negative ? unchecked(-(int)magnitude) : (int)magnitude;
            return true;
        }

        internal static float ParseFiniteFloat32OrZero(string text)
        {
            return BitConverter.Int32BitsToSingle(unchecked((int)ParseFiniteBits(text, false, out _)));
        }

        internal static double ParseFiniteFloat64OrZero(string text)
        {
            return BitConverter.Int64BitsToDouble(unchecked((long)ParseFiniteBits(text, true, out _)));
        }

        internal static bool TryParseFiniteFloat64(string text, out double value)
        {
            ulong bits = ParseFiniteBits(text, true, out bool valid);
            value = BitConverter.Int64BitsToDouble(unchecked((long)bits));
            return valid;
        }

        private static ulong ParseFiniteBits(string text, bool binary64, out bool valid)
        {
            valid = false;
            if (string.IsNullOrEmpty(text)) return 0;
            int end = binary64 ? text.Length : text.IndexOf('\0');
            if (end < 0) end = text.Length;
            if (!binary64)
                while (end > 0 && (text[end - 1] == ' ' || text[end - 1] == '\t')) end--;
            int position = 0;
            while (position < end && IsLeadingSpace(text[position])) position++;
            ulong sign = 0;
            if (position < end && (text[position] == '+' || text[position] == '-'))
                sign = text[position++] == '-' ? (binary64 ? 0x8000000000000000UL : 0x80000000UL) : 0;
            bool hex = position + 1 < end && text[position] == '0' &&
                (text[position + 1] == 'x' || text[position + 1] == 'X');
            if (hex) position += 2;
            var digits = new StringBuilder();
            bool dot = false;
            int fractionDigits = 0;
            while (position < end)
            {
                char character = text[position];
                if (character == '.' && !dot)
                {
                    dot = true;
                    position++;
                    continue;
                }
                int digit = DigitValue(character);
                if (digit < 0 || digit >= (hex ? 16 : 10)) break;
                digits.Append(character);
                if (dot) fractionDigits++;
                position++;
            }
            if (digits.Length == 0) return 0;
            long exponent = 0;
            if (position < end && (hex
                ? text[position] == 'p' || text[position] == 'P'
                : text[position] == 'e' || text[position] == 'E'))
            {
                position++;
                bool negativeExponent = position < end && text[position] == '-';
                if (position < end && (text[position] == '+' || text[position] == '-')) position++;
                int exponentStart = position;
                long exponentLimit = text.Length * 4L + (binary64 ? 4096 : 1024);
                while (position < end && text[position] >= '0' && text[position] <= '9')
                {
                    exponent = Math.Min(exponentLimit, exponent * 10 + text[position++] - '0');
                }
                if (position == exponentStart) return 0;
                if (negativeExponent) exponent = -exponent;
            }
            if (position != end) return 0;
            int first = 0;
            while (first < digits.Length && digits[first] == '0') first++;
            if (first == digits.Length)
            {
                valid = true;
                return sign;
            }
            int last = digits.Length;
            while (digits[last - 1] == '0') last--;
            long scale = exponent + (digits.Length - last - (long)fractionDigits) * (hex ? 4 : 1);
            int count = last - first;
            if (hex)
            {
                int firstBits = 0;
                for (int value = DigitValue(digits[first]); value > 0; value >>= 1) firstBits++;
                long order = 4L * (count - 1) + firstBits - 1 + scale;
                if (order > (binary64 ? 1023 : 127)) return 0;
                if (order < (binary64 ? -1075 : -150))
                {
                    valid = true;
                    return sign;
                }
            }
            else
            {
                long order = count + scale;
                if (order > (binary64 ? 309 : 39)) return 0;
                if (order < (binary64 ? -323 : -45))
                {
                    valid = true;
                    return sign;
                }
            }
            string significant = digits.ToString(first, count);
            BigInteger numerator = BigInteger.Parse(hex ? "0" + significant : significant,
                hex ? NumberStyles.AllowHexSpecifier : NumberStyles.None, CultureInfo.InvariantCulture);
            BigInteger denominator = BigInteger.One;
            if (hex)
            {
                if (scale >= 0) numerator <<= checked((int)scale);
                else denominator <<= checked((int)-scale);
            }
            else
            {
                if (scale >= 0) numerator *= BigInteger.Pow(10, checked((int)scale));
                else denominator = BigInteger.Pow(10, checked((int)-scale));
            }
            return RoundBinary(numerator, denominator, sign, binary64, out valid);
        }

        private static ulong RoundBinary(BigInteger numerator, BigInteger denominator, ulong sign, bool binary64, out bool valid)
        {
            valid = true;
            // Alignment contract: NTSD28-Q05-NATIVE-FRAME-TYPED-CONVERSION-001; round once to the caller's IEEE width.
            int fractionBits = binary64 ? 52 : 23;
            int maximumExponent = binary64 ? 1023 : 127;
            int minimumExponent = binary64 ? -1022 : -126;
            int halfSubnormalExponent = minimumExponent - fractionBits - 1;
            int exponent = BitLength(numerator) - BitLength(denominator);
            if (exponent >= 0 ? numerator < (denominator << exponent) : (numerator << -exponent) < denominator)
                exponent--;
            if (exponent > maximumExponent)
            {
                valid = false;
                return 0;
            }
            if (exponent < halfSubnormalExponent) return sign;
            int shift = exponent >= minimumExponent ? fractionBits - exponent : fractionBits - minimumExponent;
            if (shift >= 0) numerator <<= shift;
            else denominator <<= -shift;
            BigInteger rounded = BigInteger.DivRem(numerator, denominator, out BigInteger remainder);
            int midpoint = (remainder << 1).CompareTo(denominator);
            if (midpoint > 0 || (midpoint == 0 && !rounded.IsEven)) rounded++;
            if (exponent < minimumExponent) return sign | (ulong)rounded;
            if (rounded == (BigInteger.One << (fractionBits + 1)))
            {
                rounded >>= 1;
                if (++exponent > maximumExponent)
                {
                    valid = false;
                    return 0;
                }
            }
            return sign | ((ulong)(exponent + maximumExponent) << fractionBits) | ((ulong)rounded - (1UL << fractionBits));
        }

        private static int BitLength(BigInteger value)
        {
            byte[] bytes = value.ToByteArray();
            int last = bytes.Length - 1;
            while (last > 0 && bytes[last] == 0) last--;
            int bits = last * 8;
            for (int top = bytes[last]; top > 0; top >>= 1) bits++;
            return bits;
        }

        private static int DigitValue(char value)
        {
            if (value >= '0' && value <= '9') return value - '0';
            if (value >= 'a' && value <= 'f') return value - 'a' + 10;
            if (value >= 'A' && value <= 'F') return value - 'A' + 10;
            return -1;
        }

        private static bool IsLeadingSpace(char value)
        {
            return value == ' ' || (value >= '\t' && value <= '\r');
        }

    }
}
