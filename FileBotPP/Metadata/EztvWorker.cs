using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using FileBotPP.Helpers;
using FileBotPP.Interfaces;
using FileBotPP.Metadata.eztv;

namespace FileBotPP.Metadata
{
    internal class EztvWorker : IEztvWorker
    {
        private readonly string _series;
        private readonly int _seriesid;
        private readonly List< ITorrent > _torrents;
        private string _html;
        private string _imdbid;
        private bool _processed;
        private bool _working;

        public EztvWorker( int seriesid, string seriesname )
        {
            this._series = seriesname;
            this._seriesid = seriesid;
            this._torrents = new List< ITorrent >();
        }

        public void Run()
        {
            this._working = true;
            var worker = new BackgroundWorker();
            worker.DoWork += this.Worker_DoWork;
            worker.RunWorkerCompleted += this.Worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        private void Worker_DoWork( object sender, DoWorkEventArgs e )
        {
            this.get_series_data();
        }

        private void Worker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            this._working = false;
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

        public List< ITorrent > get_torrents()
        {
            return this._torrents;
        }

        public bool is_cached()
        {
            try
            {
                var tempFile = Common.AppDataFolder + "/eztv/" + this._seriesid;

                if ( File.Exists( tempFile ) )
                {
                    if ( ( File.GetLastWriteTime( tempFile ).Ticks/TimeSpan.TicksPerSecond + ( Settings.CacheTimeout ) ) > ( DateTime.Now.Ticks/TimeSpan.TicksPerSecond ) )
                    {
                        return true;
                    }

                    File.Delete( tempFile );
                }

                return false;
            }
            catch ( Exception )
            {
                return false;
            }
        }

        private void get_series_data()
        {
            if ( !Directory.Exists(Common.AppDataFolder + "/eztv/") )
            {
                Directory.CreateDirectory(Common.AppDataFolder + "/eztv");
            }

            var tempFile =  Common.AppDataFolder + "/eztv/" + this._seriesid;

            if ( File.Exists( tempFile ) )
            {
                if ( ( File.GetLastWriteTime( tempFile ).Ticks/TimeSpan.TicksPerSecond + ( Settings.CacheTimeout ) ) > ( DateTime.Now.Ticks/TimeSpan.TicksPerSecond ) )
                {
                    var filehtml = File.ReadAllText( tempFile );
                    this.parse_imdb_id( filehtml );
                    this.strip_unneeded( filehtml );
                    this.parse_episodes();
                    return;
                }

                File.Delete( tempFile );
            }

            Utils.LogLines.Enqueue( @"Dowloading " + this._series + @" metadata..." );

            var temp = Utils.Fetch( "https://eztv.ag/shows/" + this._seriesid + "/" + this._series );

            if ( String.Compare( temp, "", StringComparison.Ordinal ) == 0 )
            {
                return;
            }

            if ( Utils.write_file( tempFile, temp ) == false )
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
            Utils.LogLines.Enqueue( @"Parsing " + this._series + @" metadata..." );

            foreach ( Match match in Regex.Matches( this._html, "<tr.*?>(.*?)</tr>", RegexOptions.IgnoreCase | RegexOptions.Singleline ) )
            {
                var epname = Regex.Match( match.Groups[ 1 ].Value, @"class=.epinfo.>(.*?)<\/a>" );
                var epmagnet = Regex.Match( match.Groups[ 1 ].Value, "\"(magnet:.*?)\"" );

                if ( !epmagnet.Success || !epname.Success )
                {
                    continue;
                }

                var torrent = new Torrent {Epname = epname.Groups[ 1 ].Value.Trim(), Magnetlink = epmagnet.Groups[ 1 ].Value.Trim(), Series = this._series, Imbdid = this._imdbid};
                this._torrents.Add( torrent );
            }

            this._html = null;
        }
    }
}