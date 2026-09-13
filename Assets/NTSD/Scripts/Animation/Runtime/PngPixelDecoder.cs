using System;
using System.IO;
using System.IO.Compression;

namespace NTSD.Animation
{
    /// <summary>
    /// Small, worker-thread-safe PNG decoder for the image formats used by the
    /// NTSD 2.8-Logan content corpus.
    ///
    /// The decoder deliberately returns raw pixels instead of a Texture2D so it
    /// can be called by the existing background prewarm path.  The returned
    /// pixel array is RGBA8 and is arranged bottom-up (row zero is the bottom
    /// row), matching Unity's Color[] texture order.
    /// </summary>
    public static class PngPixelDecoder
    {
        private const int SignatureLength = 8;
        private const int IhdrLength = 13;
        private const int MaxPaletteEntries = 256;
        private const int MaxPixels = 32 * 1024 * 1024;
        private const int MaxChunkLength = 256 * 1024 * 1024;
        private const int ZlibHeaderLength = 2;
        private const int ZlibTrailerLength = 4;
        private const int ZlibMinimumLength = ZlibHeaderLength + ZlibTrailerLength + 1;

        private static readonly byte[] PngSignature =
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A
        };

        private static readonly uint[] CrcTable = CreateCrcTable();

        /// <summary>
        /// Returns true when bytes start with the PNG signature.
        /// </summary>
        public static bool HasSignature(byte[] bytes)
        {
            if (bytes == null || bytes.Length < SignatureLength)
            {
                return false;
            }

            for (int i = 0; i < SignatureLength; i++)
            {
                if (bytes[i] != PngSignature[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Decodes a supported PNG into bottom-up RGBA8 pixels.
        ///
        /// Supported formats are non-interlaced palette PNGs with bit depths
        /// 1, 2, 4 or 8, and non-interlaced RGBA8 PNGs.  Unsupported formats
        /// fail explicitly instead of being silently approximated.
        /// </summary>
        public static bool TryDecode(
            byte[] bytes,
            out int width,
            out int height,
            out byte[] rgbaBottomUp,
            out string error)
        {
            width = 0;
            height = 0;
            rgbaBottomUp = null;
            error = null;

            try
            {
                bool success = TryDecodeCore(bytes, out width, out height, out rgbaBottomUp, out error);
                if (!success)
                {
                    width = 0;
                    height = 0;
                    rgbaBottomUp = null;
                    if (string.IsNullOrEmpty(error))
                    {
                        error = "PNG decode failed.";
                    }
                }

                return success;
            }
            catch (OutOfMemoryException)
            {
                width = 0;
                height = 0;
                rgbaBottomUp = null;
                error = "PNG decode failed: image exceeds the decoder memory limits.";
                return false;
            }
            catch (Exception exception)
            {
                width = 0;
                height = 0;
                rgbaBottomUp = null;
                error = "PNG decode failed: " + exception.Message;
                return false;
            }
        }

        private static bool TryDecodeCore(
            byte[] bytes,
            out int width,
            out int height,
            out byte[] rgbaBottomUp,
            out string error)
        {
            width = 0;
            height = 0;
            rgbaBottomUp = null;
            error = null;

            if (bytes == null)
            {
                error = "PNG input is null.";
                return false;
            }

            if (!HasSignature(bytes))
            {
                error = "PNG signature is missing or invalid.";
                return false;
            }

            if (bytes.Length == SignatureLength)
            {
                error = "PNG is truncated after the signature.";
                return false;
            }

            bool sawIhdr = false;
            bool sawPlte = false;
            bool sawTrns = false;
            bool sawIdat = false;
            bool closedIdat = false;
            bool sawIend = false;
            byte colorType = 0;
            byte bitDepth = 0;
            byte[] palette = null;
            byte[] paletteAlpha = null;
            byte[] idatBytes;

            using (MemoryStream idat = new MemoryStream())
            {
                int offset = SignatureLength;
                while (offset < bytes.Length)
                {
                    if (bytes.Length - offset < 12)
                    {
                        error = "PNG is truncated in a chunk header or CRC.";
                        return false;
                    }

                    uint chunkLengthUnsigned = ReadUInt32BigEndian(bytes, offset);
                    int chunkTypeOffset = offset + 4;
                    int chunkDataOffset = offset + 8;

                    if (!IsChunkType(bytes, chunkTypeOffset))
                    {
                        error = "PNG contains an invalid chunk type.";
                        return false;
                    }

                    if (chunkLengthUnsigned > MaxChunkLength || chunkLengthUnsigned > int.MaxValue)
                    {
                        error = "PNG chunk is too large.";
                        return false;
                    }

                    int chunkLength = (int)chunkLengthUnsigned;
                    long chunkEndLong = (long)chunkDataOffset + chunkLength + 4L;
                    if (chunkEndLong > bytes.Length)
                    {
                        error = "PNG chunk is truncated.";
                        return false;
                    }

                    int crcOffset = chunkDataOffset + chunkLength;
                    uint actualCrc = ComputeChunkCrc(bytes, chunkTypeOffset, chunkDataOffset, chunkLength);
                    uint expectedCrc = ReadUInt32BigEndian(bytes, crcOffset);
                    if (actualCrc != expectedCrc)
                    {
                        error = "PNG chunk CRC mismatch.";
                        return false;
                    }

                    bool isIhdr = ChunkEquals(bytes, chunkTypeOffset, 'I', 'H', 'D', 'R');
                    bool isPlte = ChunkEquals(bytes, chunkTypeOffset, 'P', 'L', 'T', 'E');
                    bool isIdat = ChunkEquals(bytes, chunkTypeOffset, 'I', 'D', 'A', 'T');
                    bool isIend = ChunkEquals(bytes, chunkTypeOffset, 'I', 'E', 'N', 'D');
                    bool isTrns = ChunkEquals(bytes, chunkTypeOffset, 't', 'R', 'N', 'S');

                    if (!sawIhdr && !isIhdr)
                    {
                        error = "PNG IHDR must be the first chunk.";
                        return false;
                    }

                    if (IsCriticalChunk(bytes, chunkTypeOffset) &&
                        !isIhdr && !isPlte && !isIdat && !isIend)
                    {
                        error = "PNG contains an unsupported critical chunk.";
                        return false;
                    }

                    if (isIhdr)
                    {
                        if (sawIhdr || offset != SignatureLength || chunkLength != IhdrLength)
                        {
                            error = "PNG IHDR is missing, duplicated, or has an invalid length.";
                            return false;
                        }

                        uint widthUnsigned = ReadUInt32BigEndian(bytes, chunkDataOffset);
                        uint heightUnsigned = ReadUInt32BigEndian(bytes, chunkDataOffset + 4);
                        bitDepth = bytes[chunkDataOffset + 8];
                        colorType = bytes[chunkDataOffset + 9];
                        byte compressionMethod = bytes[chunkDataOffset + 10];
                        byte filterMethod = bytes[chunkDataOffset + 11];
                        byte interlaceMethod = bytes[chunkDataOffset + 12];

                        if (widthUnsigned == 0 || heightUnsigned == 0 ||
                            widthUnsigned > int.MaxValue || heightUnsigned > int.MaxValue)
                        {
                            error = "PNG IHDR dimensions are invalid.";
                            return false;
                        }

                        long pixelCount = (long)widthUnsigned * heightUnsigned;
                        if (pixelCount > MaxPixels)
                        {
                            error = "PNG image exceeds the decoder pixel limit.";
                            return false;
                        }

                        if (compressionMethod != 0 || filterMethod != 0)
                        {
                            error = "PNG uses an unsupported compression or filter method.";
                            return false;
                        }

                        if (interlaceMethod != 0)
                        {
                            error = "Interlaced PNG is unsupported.";
                            return false;
                        }

                        if (!IsSupportedFormat(colorType, bitDepth))
                        {
                            error = "PNG color type or bit depth is unsupported.";
                            return false;
                        }

                        width = (int)widthUnsigned;
                        height = (int)heightUnsigned;
                        sawIhdr = true;
                    }
                    else if (isPlte)
                    {
                        if (!sawIhdr || sawPlte || sawIdat || chunkLength == 0 ||
                            chunkLength > MaxPaletteEntries * 3 || chunkLength % 3 != 0)
                        {
                            error = "PNG PLTE is duplicated, misplaced, or has an invalid length.";
                            return false;
                        }

                        int paletteEntries = chunkLength / 3;
                        if (colorType == 3 && paletteEntries > (1 << bitDepth))
                        {
                            error = "PNG palette has more entries than its bit depth permits.";
                            return false;
                        }

                        palette = new byte[chunkLength];
                        Buffer.BlockCopy(bytes, chunkDataOffset, palette, 0, chunkLength);
                        sawPlte = true;
                    }
                    else if (isTrns)
                    {
                        if (!sawIhdr || sawTrns || sawIdat || colorType != 3 || !sawPlte ||
                            chunkLength == 0 || chunkLength > MaxPaletteEntries)
                        {
                            error = "PNG tRNS is unsupported, duplicated, misplaced, or invalid.";
                            return false;
                        }

                        if (palette == null || chunkLength > palette.Length / 3)
                        {
                            error = "PNG tRNS contains more alpha entries than the palette.";
                            return false;
                        }

                        paletteAlpha = new byte[palette.Length / 3];
                        for (int i = 0; i < paletteAlpha.Length; i++)
                        {
                            paletteAlpha[i] = 255;
                        }

                        Buffer.BlockCopy(bytes, chunkDataOffset, paletteAlpha, 0, chunkLength);
                        sawTrns = true;
                    }
                    else if (isIdat)
                    {
                        if (!sawIhdr || closedIdat || (colorType == 3 && !sawPlte))
                        {
                            error = "PNG IDAT is misplaced or appears after a non-IDAT chunk.";
                            return false;
                        }

                        if (idat.Length > MaxChunkLength - chunkLength)
                        {
                            error = "PNG compressed image data is too large.";
                            return false;
                        }

                        idat.Write(bytes, chunkDataOffset, chunkLength);
                        sawIdat = true;
                    }
                    else if (isIend)
                    {
                        if (!sawIhdr || !sawIdat || chunkLength != 0 || sawIend)
                        {
                            error = "PNG IEND is missing, duplicated, or appears before IDAT.";
                            return false;
                        }

                        sawIend = true;
                        offset = (int)chunkEndLong;
                        if (offset != bytes.Length)
                        {
                            error = "PNG contains data after IEND.";
                            return false;
                        }

                        break;
                    }
                    else
                    {
                        if (sawIdat)
                        {
                            closedIdat = true;
                        }
                    }

                    if (!isIdat && sawIdat)
                    {
                        closedIdat = true;
                    }

                    offset = (int)chunkEndLong;
                }

                if (!sawIend)
                {
                    error = "PNG is missing IEND.";
                    return false;
                }

                if (colorType == 3 && !sawPlte)
                {
                    error = "Palette PNG is missing PLTE.";
                    return false;
                }

                idatBytes = idat.ToArray();
            }

            if (paletteAlpha == null && colorType == 3)
            {
                paletteAlpha = new byte[palette.Length / 3];
                for (int i = 0; i < paletteAlpha.Length; i++)
                {
                    paletteAlpha[i] = 255;
                }
            }

            return DecodeImageData(
                idatBytes,
                width,
                height,
                colorType,
                bitDepth,
                palette,
                paletteAlpha,
                out rgbaBottomUp,
                out error);
        }

        private static bool DecodeImageData(
            byte[] zlibBytes,
            int width,
            int height,
            byte colorType,
            byte bitDepth,
            byte[] palette,
            byte[] paletteAlpha,
            out byte[] rgbaBottomUp,
            out string error)
        {
            rgbaBottomUp = null;
            error = null;

            if (zlibBytes == null || zlibBytes.Length < ZlibMinimumLength)
            {
                error = "PNG IDAT zlib stream is truncated.";
                return false;
            }

            byte cmf = zlibBytes[0];
            byte flg = zlibBytes[1];
            int zlibHeader = (cmf << 8) | flg;
            if ((cmf & 0x0F) != 8 || (cmf >> 4) > 7 || zlibHeader % 31 != 0)
            {
                error = "PNG zlib header is invalid.";
                return false;
            }

            if ((flg & 0x20) != 0)
            {
                error = "PNG zlib preset dictionary is unsupported.";
                return false;
            }

            int compressedOffset = ZlibHeaderLength;
            int compressedLength = zlibBytes.Length - ZlibHeaderLength - ZlibTrailerLength;
            if (compressedLength <= 0)
            {
                error = "PNG zlib stream has no deflate payload.";
                return false;
            }

            long rowBits = (long)width * (colorType == 3 ? bitDepth : 32);
            long rowBytesLong = (rowBits + 7L) / 8L;
            long scanlineLengthLong = rowBytesLong + 1L;
            long rawLengthLong = scanlineLengthLong * height;
            long pixelLengthLong = (long)width * height * 4L;

            if (rowBytesLong <= 0 || rowBytesLong > int.MaxValue ||
                scanlineLengthLong > int.MaxValue || rawLengthLong > int.MaxValue ||
                pixelLengthLong > int.MaxValue)
            {
                error = "PNG scanline dimensions exceed the decoder limits.";
                return false;
            }

            int rowBytes = (int)rowBytesLong;
            int expectedRawLength = (int)rawLengthLong;
            int outputLength = (int)pixelLengthLong;
            int filterBytesPerPixel = colorType == 3 ? 1 : 4;

            byte[] output = new byte[outputLength];
            byte[] currentRow = new byte[rowBytes];
            byte[] previousRow = new byte[rowBytes];
            Adler32 adler = new Adler32();

            try
            {
                using (MemoryStream compressedStream = new MemoryStream(
                           zlibBytes,
                           compressedOffset,
                           compressedLength,
                           false,
                           true))
                using (DeflateStream deflate = new DeflateStream(
                           compressedStream,
                           CompressionMode.Decompress,
                           true))
                {
                    int bytesRead = 0;
                    for (int sourceRow = 0; sourceRow < height; sourceRow++)
                    {
                        int filterType = deflate.ReadByte();
                        if (filterType < 0)
                        {
                            error = "PNG image data is truncated before a scanline filter byte.";
                            return false;
                        }

                        adler.Update((byte)filterType);
                        if (!ReadDeflateExact(deflate, currentRow, 0, rowBytes, adler, ref bytesRead))
                        {
                            error = "PNG image data is truncated in a scanline.";
                            return false;
                        }

                        if (filterType > 4)
                        {
                            error = "PNG uses an unsupported scanline filter.";
                            return false;
                        }

                        Unfilter(currentRow, previousRow, filterType, filterBytesPerPixel);
                        WriteRgbaRow(
                            currentRow,
                            sourceRow,
                            width,
                            height,
                            colorType,
                            bitDepth,
                            palette,
                            paletteAlpha,
                            output);

                        byte[] swap = previousRow;
                        previousRow = currentRow;
                        currentRow = swap;
                    }

                    if (bytesRead + height != expectedRawLength)
                    {
                        error = "PNG internal scanline length accounting failed.";
                        return false;
                    }

                    if (deflate.ReadByte() >= 0)
                    {
                        error = "PNG decompressed data exceeds the expected image size.";
                        return false;
                    }
                }
            }
            catch (InvalidDataException exception)
            {
                error = "PNG deflate stream is invalid: " + exception.Message;
                return false;
            }
            catch (IOException exception)
            {
                error = "PNG deflate stream could not be read: " + exception.Message;
                return false;
            }

            uint expectedAdler = ReadUInt32BigEndian(
                zlibBytes,
                zlibBytes.Length - ZlibTrailerLength);
            if (adler.Value != expectedAdler)
            {
                error = "PNG Adler-32 checksum mismatch.";
                return false;
            }

            rgbaBottomUp = output;
            return true;
        }

        private static bool ReadDeflateExact(
            Stream stream,
            byte[] buffer,
            int offset,
            int count,
            Adler32 adler,
            ref int bytesRead)
        {
            int remaining = count;
            while (remaining > 0)
            {
                int read = stream.Read(buffer, offset, remaining);
                if (read <= 0)
                {
                    return false;
                }

                adler.Update(buffer, offset, read);
                bytesRead = checked(bytesRead + read);
                offset += read;
                remaining -= read;
            }

            return true;
        }

        private static void Unfilter(
            byte[] row,
            byte[] previous,
            int filterType,
            int bytesPerPixel)
        {
            switch (filterType)
            {
                case 0:
                    return;

                case 1:
                    for (int i = bytesPerPixel; i < row.Length; i++)
                    {
                        row[i] = unchecked((byte)(row[i] + row[i - bytesPerPixel]));
                    }

                    return;

                case 2:
                    for (int i = 0; i < row.Length; i++)
                    {
                        row[i] = unchecked((byte)(row[i] + previous[i]));
                    }

                    return;

                case 3:
                    for (int i = 0; i < row.Length; i++)
                    {
                        int left = i >= bytesPerPixel ? row[i - bytesPerPixel] : 0;
                        int up = previous[i];
                        row[i] = unchecked((byte)(row[i] + ((left + up) >> 1)));
                    }

                    return;

                case 4:
                    for (int i = 0; i < row.Length; i++)
                    {
                        int left = i >= bytesPerPixel ? row[i - bytesPerPixel] : 0;
                        int up = previous[i];
                        int upperLeft = i >= bytesPerPixel ? previous[i - bytesPerPixel] : 0;
                        row[i] = unchecked((byte)(row[i] + PaethPredictor(left, up, upperLeft)));
                    }

                    return;

                default:
                    throw new ArgumentOutOfRangeException(nameof(filterType));
            }
        }

        private static int PaethPredictor(int left, int up, int upperLeft)
        {
            int p = left + up - upperLeft;
            int pa = Math.Abs(p - left);
            int pb = Math.Abs(p - up);
            int pc = Math.Abs(p - upperLeft);

            if (pa <= pb && pa <= pc)
            {
                return left;
            }

            return pb <= pc ? up : upperLeft;
        }

        private static void WriteRgbaRow(
            byte[] row,
            int sourceRow,
            int width,
            int height,
            byte colorType,
            byte bitDepth,
            byte[] palette,
            byte[] paletteAlpha,
            byte[] output)
        {
            int outputRow = height - 1 - sourceRow;
            int outputOffset = checked(outputRow * width * 4);

            if (colorType == 6)
            {
                Buffer.BlockCopy(row, 0, output, outputOffset, width * 4);
                return;
            }

            int mask = (1 << bitDepth) - 1;
            int paletteCount = palette.Length / 3;
            for (int x = 0; x < width; x++)
            {
                int bitOffset = x * bitDepth;
                int byteOffset = bitOffset >> 3;
                int shift = 8 - bitDepth - (bitOffset & 7);
                int paletteIndex = (row[byteOffset] >> shift) & mask;
                if (paletteIndex < 0 || paletteIndex >= paletteCount)
                {
                    throw new InvalidDataException("PNG palette index is out of bounds.");
                }

                int paletteOffset = paletteIndex * 3;
                int pixelOffset = outputOffset + x * 4;
                output[pixelOffset] = palette[paletteOffset];
                output[pixelOffset + 1] = palette[paletteOffset + 1];
                output[pixelOffset + 2] = palette[paletteOffset + 2];
                output[pixelOffset + 3] = paletteAlpha[paletteIndex];
            }
        }

        private static bool IsSupportedFormat(byte colorType, byte bitDepth)
        {
            if (colorType == 3)
            {
                return bitDepth == 1 || bitDepth == 2 || bitDepth == 4 || bitDepth == 8;
            }

            return colorType == 6 && bitDepth == 8;
        }

        private static bool IsChunkType(byte[] bytes, int offset)
        {
            for (int i = 0; i < 4; i++)
            {
                byte value = bytes[offset + i];
                if (!((value >= (byte)'A' && value <= (byte)'Z') ||
                      (value >= (byte)'a' && value <= (byte)'z')))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsCriticalChunk(byte[] bytes, int offset)
        {
            return bytes[offset] >= (byte)'A' && bytes[offset] <= (byte)'Z';
        }

        private static bool ChunkEquals(
            byte[] bytes,
            int offset,
            char first,
            char second,
            char third,
            char fourth)
        {
            return bytes[offset] == (byte)first &&
                   bytes[offset + 1] == (byte)second &&
                   bytes[offset + 2] == (byte)third &&
                   bytes[offset + 3] == (byte)fourth;
        }

        private static uint ReadUInt32BigEndian(byte[] bytes, int offset)
        {
            return ((uint)bytes[offset] << 24) |
                   ((uint)bytes[offset + 1] << 16) |
                   ((uint)bytes[offset + 2] << 8) |
                   bytes[offset + 3];
        }

        private static uint ComputeChunkCrc(
            byte[] bytes,
            int typeOffset,
            int dataOffset,
            int dataLength)
        {
            uint crc = 0xFFFFFFFFU;
            for (int i = 0; i < 4; i++)
            {
                crc = (crc >> 8) ^ CrcTable[(crc ^ bytes[typeOffset + i]) & 0xFF];
            }

            for (int i = 0; i < dataLength; i++)
            {
                crc = (crc >> 8) ^ CrcTable[(crc ^ bytes[dataOffset + i]) & 0xFF];
            }

            return crc ^ 0xFFFFFFFFU;
        }

        private static uint[] CreateCrcTable()
        {
            uint[] table = new uint[256];
            for (uint i = 0; i < table.Length; i++)
            {
                uint value = i;
                for (int bit = 0; bit < 8; bit++)
                {
                    value = (value & 1U) != 0
                        ? (value >> 1) ^ 0xEDB88320U
                        : value >> 1;
                }

                table[i] = value;
            }

            return table;
        }

        private sealed class Adler32
        {
            private const uint Modulus = 65521U;
            private uint _sum1 = 1U;
            private uint _sum2;

            public uint Value
            {
                get { return (_sum2 << 16) | _sum1; }
            }

            public void Update(byte value)
            {
                _sum1 += value;
                if (_sum1 >= Modulus)
                {
                    _sum1 -= Modulus;
                }

                _sum2 += _sum1;
                if (_sum2 >= Modulus)
                {
                    _sum2 %= Modulus;
                }
            }

            public void Update(byte[] values, int offset, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    Update(values[offset + i]);
                }
            }
        }
    }
}
