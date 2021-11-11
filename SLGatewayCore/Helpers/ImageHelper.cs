using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace SLGatewayCore.Helpers
{
    public static class ImageHelper
    {
        public static bool IsValidImage(byte[] bytes, int maxSize) => IsValidImage(bytes, (long)maxSize);
        public static bool IsValidImage(byte[] bytes, long maxSize = long.MaxValue)
        {
            if (bytes.Length > maxSize)
            {
                return false;
            }

            try
            {
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    Image.FromStream(ms);
                }
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        }

        public static string? GetImageType(byte[] bytes)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    var img = Image.FromStream(ms);

                    var format = img.RawFormat;
                    var codec = ImageCodecInfo.GetImageDecoders().FirstOrDefault(c => c.FormatID == format.Guid);
                    var mimeType = codec?.MimeType;

                    return mimeType;
                }
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public static string ConvertBytesToDataUrl(byte[] imageBytes)
        {
            return $"data:{GetImageType(imageBytes) ?? "image/jpg"};base64,{Convert.ToBase64String(imageBytes)}";
        }
    }

}
