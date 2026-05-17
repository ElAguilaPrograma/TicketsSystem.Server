using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
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

            var headerBytes = reader.ReadBytes(12);

            var isWebp = headerBytes.Length >= 12
                && headerBytes[0] == 0x52 && headerBytes[1] == 0x49 && headerBytes[2] == 0x46 && headerBytes[3] == 0x46
                && headerBytes[8] == 0x57 && headerBytes[9] == 0x45 && headerBytes[10] == 0x42 && headerBytes[11] == 0x50;

            if (isWebp)
                return true;

            return imageSignatures.Any(signature =>
                headerBytes.Take(signature.Length).SequenceEqual(signature));
        }

        public static bool IsValidBusinessFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var contentType = (file.ContentType ?? string.Empty).ToLowerInvariant();

            var allowedExtensions = new HashSet<string>
            {
                ".jpg", ".jpeg", ".png", ".gif", ".webp",
                ".pdf",
                ".doc", ".docx",
                ".ppt", ".pptx",
                ".xls", ".xlsx",
                ".txt", ".csv", ".rtf"
            };

            var allowedContentTypes = new HashSet<string>
            {
                "image/jpeg",
                "image/png",
                "image/gif",
                "image/webp",
                "application/pdf",
                "application/msword",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/vnd.ms-powerpoint",
                "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                "application/vnd.ms-excel",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "text/plain",
                "text/csv",
                "application/rtf"
            };

            if (!allowedExtensions.Contains(extension))
                return false;

            if (!allowedContentTypes.Contains(contentType) && contentType != "application/octet-stream")
                return false;

            if (contentType.StartsWith("image/") || extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif" || extension == ".webp")
                return IsValidImage(file);

            if (extension == ".pdf")
            {
                using var stream = file.OpenReadStream();
                using var reader = new BinaryReader(stream);
                var headerBytes = reader.ReadBytes(4);
                return headerBytes.Length >= 4
                    && headerBytes[0] == 0x25 && headerBytes[1] == 0x50 && headerBytes[2] == 0x44 && headerBytes[3] == 0x46;
            }

            return true;
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
