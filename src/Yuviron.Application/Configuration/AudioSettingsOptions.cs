namespace Yuviron.Application.Configuration;

public class AudioSettingsOptions
{
    public int[] HlsQualities { get; set; } = { 128, 320 };
    public string FfmpegAudioFilters { get; set; } = "-af loudnorm=I=-14:LRA=11:TP=-1.5"; 
}