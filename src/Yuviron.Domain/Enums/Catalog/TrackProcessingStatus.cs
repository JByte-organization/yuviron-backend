namespace Yuviron.Domain.Enums;

public enum TrackProcessingStatus
{
    Processing = 1, // FFmpeg конвертирует трек
    Ready = 2,      // Трек нарезан на HLS и готов к стримингу
    Failed = 3      // Ошибка при конвертации
}