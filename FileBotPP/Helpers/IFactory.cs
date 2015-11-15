using System.Collections.Concurrent;
using System.Collections.Generic;
using FileBotPP.Metadata;
using FileBotPP.Tree;

namespace FileBotPP.Helpers
{
    public interface IFactory
    {
        ConcurrentQueue<string> LogLines { get; }
        string[] LocationParts { get; set; }
        string AddSeriesName { get; set; }
        IWindowFileBotPp WindowFileBotPp { get; set; }
        IEztv Eztv { get; set; }
        ITvdb Tvdb { get; set; }
        IKat Kat { get; set; }
        IFilebot Filebot { get; set; }
        ISeriesAnalyzer SeriesAnalyzer { get; set; }
        IUtils Utils { get; set; }
        ISettings Settings { get; set; }
        string ScanLocation { get; set; }
        List< ISupportsStop > Working { get; set; }
        int MetaDataReady { get; set; }
        string AppDataFolder { get; set; }
        bool EztvAvailable { get; set; }
        bool TvdbAvailable { get; set; }
        void fetch_tvdb_metadata();
        void fetch_eztv_metadata();
        void fetch_kat_metadata();
        void stop_all_workers();
        void dispose_all_workers();
    }
}