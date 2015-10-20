using System.Collections.ObjectModel;
using System.ComponentModel;
using FileBotPP.Metadata.Interfaces;

namespace FileBotPP.Tree.Interfaces
{
    public interface IItem
    {
        ObservableCollection< IItem > Items { get; set; }
        string ShortName { get; set; }
        string Extension { get; set; }
        string SuggestedName { get; set; }
        string NewPath { get; set; }
        bool Torrent { get; set; }
        string TorrentLink { get; set; }
        bool NewPathExists { get; set; }
        bool Empty { get; set; }
        bool AllowedType { get; set; }
        bool Missing { get; set; }
        bool BadLocation { get; set; }
        bool BadName { get; set; }
        bool BadQuality { get; set; }
        bool Corrupt { get; set; }
        bool Duplicate { get; set; }
        bool Dirty { get; set; }
        bool Extra { get; set; }
        int Count { get; set; }
        IMediaInfo Mediainfo { get; set; }
        IItem Parent { get; set; }
        string FullName { get; set; }
        string Path { get; set; }
        event PropertyChangedEventHandler PropertyChanged;
        void Update();
    }
}