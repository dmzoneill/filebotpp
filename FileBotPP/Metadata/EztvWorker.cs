using System;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using FileBotPP.Helpers;

namespace FileBotPP.Metadata
{
    internal class EztvWorker : IEztvWorker
    {
        private readonly string _series;
        private readonly int _seriesid;
        private string _html;
        private string _imdbid;
        private bool _processed;
        private bool _working;

        public EztvWorker( int seriesid, string seriesname )
        {
            this._series = seriesname;
            this._seriesid = seriesid;
        }

        public void Run()
        {
            this._working = true;
            BackgroundTasks.RunOnce( this.get_series_data, () => this._working = false );
        }

        public bool is_working()
        {
            return this._working;
        }

        public string get_html()
        {
            this._processed = true;
            return this._html;
        }

        public string get_series_name()
        {
            return this._series;
        }

        public bool is_processed()
        {
            return this._processed;
        }

        public bool is_cached()
        {
            try
            {
                return CacheFiles.IsFresh( Factory.Instance.AppDataFolder + "/eztv/" + this._seriesid );
            }
            catch ( Exception ex )
            {
                AppLog.Exception( ex );
                return false;
            }
        }

        private void get_series_data()
        {
            var folder = Factory.Instance.AppDataFolder + "/eztv/";
            CacheFiles.EnsureDirectory( folder );

            var tempFile = folder + this._seriesid;
            var cached = CacheFiles.ReadIfFresh( tempFile, true );
            if ( cached != null )
            {
                this.parse_imdb_id( cached );
                this.strip_unneeded( cached );
                this.parse_episodes();
                return;
            }

            Factory.Instance.LogLines.Enqueue( @"Downloading " + this._series + @" metadata..." );

            var temp = Factory.Instance.Utils.Fetch( "https://eztv.ag/shows/" + this._seriesid + "/" + this._series );

            if ( String.Compare( temp, "", StringComparison.Ordinal ) == 0 )
            {
                return;
            }

            if ( Factory.Instance.Utils.write_file( tempFile, temp ) == false )
            {
                return;
            }

            this.parse_imdb_id( temp );
            this.strip_unneeded( temp );
            this.parse_episodes();
        }

        private void strip_unneeded( string temp )
        {
            string[] parts;

            if ( temp.Contains( "Episode Name" ) )
            {
                parts = temp.Split( new[] {"Episode Name"}, StringSplitOptions.None );
            }
            else if ( temp.Contains( "Episode FullName" ) )
            {
                parts = temp.Split( new[] {"Episode FullName"}, StringSplitOptions.None );
            }
            else
            {
                return;
            }

            var torrents = parts[ 1 ].Split( new[] {"</table>"}, StringSplitOptions.None );

            this._html = torrents[ 0 ];
        }

        private void parse_imdb_id( string html )
        {
            var imdbmatch = Regex.Match( html, "http://www.imdb.com/title/(tt.*?)/", RegexOptions.IgnoreCase );

            if ( imdbmatch.Success )
            {
                this._imdbid = imdbmatch.Groups[ 1 ].Value;
            }
        }

        private void parse_episodes()
        {
            Factory.Instance.LogLines.Enqueue( @"Parsing " + this._series + @" metadata..." );

            TorrentRows.AddFromHtml( this._html, @"class=.epinfo.>(.*?)<\/a>", this._series, this._imdbid );

            this._html = null;
        }
    }
}