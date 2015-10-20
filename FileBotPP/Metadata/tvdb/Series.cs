using System;
using System.Collections.Generic;
using System.Linq;
using FileBotPP.Metadata.tvdb.Interfaces;

namespace FileBotPP.Metadata.tvdb
{
    public class Series : ISeries
    {
        private readonly List< ISeason > _seasons;
        private string _actors;
        private string _genre;

        public Series( string series )
        {
            this.SeriesName = series;
            this._seasons = new List< ISeason >();
        }

        public string Id { get; set; }

        public string Actors
        {
            get
            {
                if ( !this._actors.Contains( '|' ) )
                {
                    return this._actors;
                }

                var actorrarr = this._actors.Split( '|' ).ToList();
                actorrarr.RemoveAll( is_blank );
                return String.Join( Environment.NewLine, actorrarr );
            }
            set { this._actors = value; }
        }

        public string AirsDayOfWeek { get; set; }
        public string AirsTime { get; set; }
        public string ContentRating { get; set; }
        public string FirstAired { get; set; }

        public string Genre
        {
            get
            {
                if ( !this._genre.Contains( '|' ) )
                {
                    return this._genre;
                }

                var actorrarr = this._genre.Split( '|' ).ToList();
                actorrarr.RemoveAll( is_blank );
                return String.Join( Environment.NewLine, actorrarr );
            }
            set { this._genre = value; }
        }

        public string ImdbId { get; set; }
        public string Language { get; set; }
        public string Network { get; set; }
        public string NetworkId { get; set; }
        public string Overview { get; set; }
        public string Rating { get; set; }
        public string RatingCount { get; set; }
        public string Runtime { get; set; }
        public string SeriesId { get; set; }
        public string Status { get; set; }
        public string Added { get; set; }
        public string AddedBy { get; set; }
        public string Banner { get; set; }
        public string Fanart { get; set; }
        public string Lastupdated { get; set; }
        public string Poster { get; set; }
        public string TmsWantedOld { get; set; }
        public string Zap2ItId { get; set; }
        public string SeriesName { get; set; }

        public string TvdbHyperlink
        {
            get { return "http://thetvdb.com/?tab=series&id=" + this.Id; }
            set { }
        }

        public string ImdbHyperlink
        {
            get { return "http://www.imdb.com/title/" + this.ImdbId; }
            set { }
        }

        public List< ISeason > get_seasons()
        {
            return this._seasons;
        }

        public bool has_seasons( int num )
        {
            return this._seasons.Any( season => season.get_season_num() == num );
        }

        public ISeason get_season( int num )
        {
            foreach ( var season in this._seasons )
            {
                if ( season.get_season_num() == num )
                {
                    return season;
                }
            }

            var newseason = new Season( num );

            this._seasons.Add( newseason );

            return newseason;
        }

        public string get_name()
        {
            return this.SeriesName;
        }

        private static bool is_blank( String s )
        {
            return String.Compare( s.Trim(), "", StringComparison.Ordinal ) == 0;
        }
    }
}