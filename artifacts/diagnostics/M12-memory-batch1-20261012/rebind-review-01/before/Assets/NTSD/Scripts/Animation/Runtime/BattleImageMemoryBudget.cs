using System;
using System.IO;

namespace NTSD.Animation
{
    // Engineering image allocation estimates; these are not physical/device memory allowances.
    internal sealed class BattleImageMemoryBudget
    {
        internal const long DefaultResidentLimit = 512L * 1024 * 1024;
        internal const long DefaultLoadingLimit = 768L * 1024 * 1024;
        internal const long PageBytes = 4L * BattleAtlasLayoutPlanner.PageSize * BattleAtlasLayoutPlanner.PageSize;
        private readonly long[] largestDecodeWork = new long[4];
        private long maximumUploadBytes;
        private long maximumClampWorkBytes;
        internal long SourceTextureBytes { get; private set; }
        internal long ReadableTextureBytes { get; private set; }
        internal long RetainedPixelBytes { get; private set; }
        internal long OldPublicationBytes { get; }
        internal long ReservedAtlasBytes { get; private set; }
        internal long ResidentBytes { get; private set; }
        internal long LoadingBytes { get; private set; }
        internal long ResidentLimit { get; }
        internal long LoadingLimit { get; }

        internal BattleImageMemoryBudget(long oldPublicationBytes,
            long residentLimit = DefaultResidentLimit, long loadingLimit = DefaultLoadingLimit)
        {
            if (oldPublicationBytes < 0 || residentLimit <= 0 || loadingLimit <= 0)
                throw new ArgumentOutOfRangeException(nameof(oldPublicationBytes));
            OldPublicationBytes = oldPublicationBytes;
            ResidentLimit = residentLimit;
            LoadingLimit = loadingLimit;
        }

        internal void AddImage(BMPLoader.VerifiedImageLayout image, bool retainPixels, bool readableTexture)
        {
            long pixels = PixelBytes(image.Width, image.Height);
            checked
            {
                SourceTextureBytes += pixels;
                if (readableTexture) ReadableTextureBytes += pixels;
                if (retainPixels) RetainedPixelBytes += pixels;
                maximumUploadBytes = Math.Max(maximumUploadBytes, pixels);
                // RGBA output + compact copy, compressed data/IDAT copies and row buffers.
                // Legacy BMP keeps the float route; count its additional 16 bytes/pixel.
                long work = pixels * (image.IsPng ? 2 : 6) + 4 * image.FileBytes +
                    8L * image.Width + 65536;
                for (int i = 0; i < largestDecodeWork.Length; i++)
                {
                    if (work <= largestDecodeWork[i]) continue;
                    long displaced = largestDecodeWork[i];
                    largestDecodeWork[i] = work;
                    work = displaced;
                }
            }
        }

        internal void AddClampedCell(int width, int height)
        {
            long bytes = PixelBytes(width, height);
            checked
            {
                SourceTextureBytes += bytes;
                RetainedPixelBytes += bytes;
                maximumUploadBytes = Math.Max(maximumUploadBytes, bytes);
                maximumClampWorkBytes = Math.Max(maximumClampWorkBytes, 2 * bytes + 8);
            }
        }

        internal void ReserveAtlas(long bytes)
        {
            if (bytes < 0) throw new ArgumentOutOfRangeException(nameof(bytes));
            ReservedAtlasBytes = bytes;
            Validate(bytes);
        }

        internal void Validate(long atlasBytes)
        {
            if (atlasBytes < 0 || atlasBytes > ReservedAtlasBytes)
                throw new InvalidDataException("Actual atlas exceeds the preflight reservation.");
            checked
            {
                ResidentBytes = OldPublicationBytes + SourceTextureBytes + ReadableTextureBytes + atlasBytes;
                // Array CPU staging can hold every slice until Apply. Fallback may retain a failed
                // array until Unity destroys it, so reserve another CPU/GPU pair conservatively.
                LoadingBytes = ResidentBytes + RetainedPixelBytes + maximumUploadBytes +
                    4 * maximumClampWorkBytes + (atlasBytes == 0 ? 0 : 3 * atlasBytes + PageBytes);
                for (int i = 0; i < largestDecodeWork.Length; i++) LoadingBytes += largestDecodeWork[i];
            }
            if (ResidentBytes > ResidentLimit || LoadingBytes > LoadingLimit)
                throw new InvalidDataException("Image batch budget refused before publication: " + Describe());
        }

        internal string Describe()
        {
            return $"resident={ResidentBytes}/{ResidentLimit}, loading={LoadingBytes}/{LoadingLimit}, " +
                $"sourceGPU={SourceTextureBytes}, readableCPU={ReadableTextureBytes}, " +
                $"retainedRGBA={RetainedPixelBytes}, oldPublication={OldPublicationBytes}, " +
                $"atlasReservation={ReservedAtlasBytes} bytes; engineering estimate, not process/RAM peak";
        }

        internal static long PixelBytes(int width, int height)
        {
            if (width <= 0 || height <= 0) throw new InvalidDataException("Image dimensions are unknown or invalid.");
            return checked(4L * width * height);
        }
    }
}
