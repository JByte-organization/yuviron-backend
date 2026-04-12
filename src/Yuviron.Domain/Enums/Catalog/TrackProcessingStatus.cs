namespace Yuviron.Domain.Enums;

public enum TrackProcessingStatus
{
    Processing = 1, // FFmpeg converts the track
    Ready = 2,      // The track is cut on HLS and ready for streaming
    Failed = 3      // Error during conversion
}