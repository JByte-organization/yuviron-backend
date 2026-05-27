using System;

namespace Yuviron.Application.Configuration;

public class FileAccessOptions
{
    public const string SectionName = "FileAccess";

    public string[] PublicFolders { get; set; } = { "covers/", "avatars/", "banners/", "ads/" };
}