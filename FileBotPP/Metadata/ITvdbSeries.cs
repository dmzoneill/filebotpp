using System.Collections.Generic;

namespace FileBotPP.Metadata
{
    public interface ITvdbSeries
    {
        string Id { get; set; }
        string Actors { get; set; }
        string AirsDayOfWeek { get; set; }
        string AirsTime { get; set; }
        string ContentRating { get; set; }
        string FirstAired { get; set; }
        string Genre { get; set; }
        string ImdbId { get; set; }
        string Language { get; set; }
        string Network { get; set; }
        string NetworkId { get; set; }
        string Overview { get; set; }
        string Rating { get; set; }
        string RatingCount { get; set; }
        string Runtime { get; set; }
        string SeriesId { get; set; }
        string Status { get; set; }
        string Added { get; set; }
        string AddedBy { get; set; }
        string Banner { get; set; }
        string Fanart { get; set; }
        string Lastupdated { get; set; }
        string Poster { get; set; }
        string TmsWantedOld { get; set; }
        string Zap2ItId { get; set; }
        string SeriesName { get; set; }
        string TvdbHyperlink { get; set; }
        string ImdbHyperlink { get; set; }
        List< ITvdbSeason > get_seasons();
        bool has_seasons( int num );
        ITvdbSeason get_season( int num );
        string get_name();
    }
}