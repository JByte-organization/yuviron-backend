using System.IO;
using System.Linq;

namespace Yuviron.Application.Common.Utilities;

public static class FileSignatureDetector
{
    
    public static (string? Extension, string ContentType) Detect(Stream stream)
    {
        if (stream == null || !stream.CanRead || !stream.CanSeek)
            return (null, "application/octet-stream");

        byte[] header = new byte[12];
        stream.Position = 0;

        try
        {
            int bytesRead = stream.Read(header, 0, 12);
            
            if (bytesRead < 3) 
                return (null, "application/octet-stream");

            // Изображения
            if (header.Take(3).SequenceEqual(new byte[] { 0xFF, 0xD8, 0xFF })) 
                return (".jpg", "image/jpeg");
            
            if (header.Take(8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })) 
                return (".png", "image/png");
            
            if (header.Take(4).SequenceEqual(new byte[] { 0x52, 0x49, 0x46, 0x46 }) && 
                header.Skip(8).Take(4).SequenceEqual(new byte[] { 0x57, 0x45, 0x42, 0x50 })) 
                return (".webp", "image/webp");

            // Аудио
            if (header.Take(3).SequenceEqual(new byte[] { 0x49, 0x44, 0x33 }) || 
               (header[0] == 0xFF && (header[1] == 0xFB || header[1] == 0xF3 || header[1] == 0xF2))) 
                return (".mp3", "audio/mpeg");
               
            if (bytesRead >= 12 && 
                header.Take(4).SequenceEqual(new byte[] { 0x52, 0x49, 0x46, 0x46 }) && 
                header.Skip(8).Take(4).SequenceEqual(new byte[] { 0x57, 0x41, 0x56, 0x45 })) 
                return (".wav", "audio/wav");

            return (null, "application/octet-stream");
        }
        finally
        {
            
            stream.Position = 0;
        }
    }
}