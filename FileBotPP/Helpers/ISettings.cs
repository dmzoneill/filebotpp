using System.Collections.Generic;

namespace FileBotPP.Helpers
{
    public interface ISettings
    {
        string ProxyServerHost { get; set; }
        int ProxyServerPort { get; set; }
        List< string > AllowedTypes { get; set; }
        string TvdbApiKey { get; set; }
        int CacheTimeout { get; set; }
        int PoorQualityP { get; set; }
        string TorrentPreferredQuality { get; set; }
        string FFmpegConvert { get; set; }
        int FilesSystemWatcherMinRefreshTime { get; set; }
        int FilesSystemWatcherMaxRefreshTime { get; set; }
        bool FilesSystemWatcherEnabled { get; set; }
    }
}