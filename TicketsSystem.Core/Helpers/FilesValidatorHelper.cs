using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace TicketsSystem.Core.Helpers
{
    public static class FilesValidatorHelper
    {
        public static bool IsValidImage(IFormFile file)
        {
            if (!file.ContentType.StartsWith("image/"))
                return false;

            var imageSignatures = new List<byte[]>
        {
            new byte[] { 0xFF, 0xD8, 0xFF }, // JPEG
            new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, // PNG
            new byte[] { 0x47, 0x49, 0x46, 0x38 } // GIF
        };

            using var stream = file.OpenReadStream();
            using var reader = new BinaryReader(stream);

            var headerBytes = reader.ReadBytes(8);

            return imageSignatures.Any(signature =>
                headerBytes.Take(signature.Length).SequenceEqual(signature));
        }

        public static bool IsValidSize(IFormFile file)
        {
            long maxFileSizeInBytes = 25 * 1024 * 1024;

            if (file.Length > maxFileSizeInBytes)
                return false;

            return true;
        }
    }
}
