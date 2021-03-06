﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FileBotPP.Helpers;

namespace FileBotPP.Metadata
{
    internal class KatWorker : IKatWorker
    {
        private readonly string _serieslink;
        private readonly string _seriesname;
        private readonly string _seriesnameClean;
        private string _imdbid;
        private bool _working;

        public KatWorker( string link, string seriesname )
        {
            this._serieslink = link;
            this._seriesname = seriesname;

            var invalid = new string( Path.GetInvalidFileNameChars() ) + new string( Path.GetInvalidPathChars() );
            this._seriesnameClean = invalid.Aggregate( this._seriesname, ( current, c ) => current.Replace( c.ToString(), "" ) );
        }

        public void Run()
        {
            this._working = true;
            var worker = new BackgroundWorker();
            worker.DoWork += this.Worker_DoWork;
            worker.RunWorkerCompleted += this.Worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        public bool is_working()
        {
            return this._working;
        }

        public string get_series_name()
        {
            return this._seriesname;
        }

        public bool is_cached()
        {
            try
            {
                var tempFile = Factory.Instance.AppDataFolder + "/kat/" + this._seriesnameClean;

                if ( File.Exists( tempFile ) )
                {
                    if ( ( File.GetLastWriteTime( tempFile ).Ticks/TimeSpan.TicksPerSecond + ( Factory.Instance.Settings.CacheTimeout ) ) > ( DateTime.Now.Ticks/TimeSpan.TicksPerSecond ) )
                    {
                        return true;
                    }

                    File.Delete( tempFile );
                }

                return false;
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
                return false;
            }
        }

        private void Worker_DoWork( object sender, DoWorkEventArgs e )
        {
            this.get_series_data();
        }

        private void Worker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            this._working = false;
        }

        private void get_series_data()
        {
            if ( Directory.Exists( Factory.Instance.AppDataFolder + "/kat/" ) == false )
            {
                Directory.CreateDirectory( Factory.Instance.AppDataFolder + "/kat" );
            }

            var tempFile = Factory.Instance.AppDataFolder + "/kat/" + this._seriesnameClean + ".tmp";

            if ( File.Exists( tempFile ) )
            {
                if ( ( File.GetLastWriteTime( tempFile ).Ticks/TimeSpan.TicksPerSecond + ( Factory.Instance.Settings.CacheTimeout ) ) > ( DateTime.Now.Ticks/TimeSpan.TicksPerSecond ) )
                {
                    var filehtml = File.ReadAllText( tempFile );
                    this.parse_imdb_id( filehtml );
                    this.parse_episodes_pages();
                    return;
                }

                File.Delete( tempFile );
            }

            Factory.Instance.LogLines.Enqueue( @"Dowloading " + this._seriesname + @" torrents..." );

            var temp = Factory.Instance.Utils.FetchDeCompressed( "https://kat.cr" + this._serieslink );

            if ( String.Compare( temp, "", StringComparison.Ordinal ) == 0 || temp == null )
            {
                return;
            }

            if ( Factory.Instance.Utils.write_file( tempFile, temp ) == false )
            {
                return;
            }

            this.parse_imdb_id( temp );
            this.parse_episodes_pages();
        }

        private void parse_imdb_id( string html )
        {
            var imdbmatch = Regex.Match( html, "http://www.imdb.com/title/(tt.*?)/", RegexOptions.IgnoreCase );

            this._imdbid = imdbmatch.Success ? imdbmatch.Groups[ 1 ].Value : null;
        }

        private void parse_episodes_pages()
        {
            if ( this._imdbid == null )
            {
                Factory.Instance.LogLines.Enqueue( @"skipping " + this._seriesname + @", no imdb id..." );
                return;
            }

            Factory.Instance.LogLines.Enqueue( @"Parsing " + this._seriesnameClean + @" metadata..." );

            if ( Directory.Exists( Factory.Instance.AppDataFolder + "/kat/" + this._seriesnameClean ) == false )
            {
                Directory.CreateDirectory( Factory.Instance.AppDataFolder + "/kat/" + this._seriesnameClean );
            }

            for ( var x = 1; x < this.get_series_torrents_pages_count(); x++ )
            {
                var torrentPage = this.get_series_torrent_page( x );
                if ( String.Compare( torrentPage, "", StringComparison.Ordinal ) == 0 )
                {
                    continue;
                }
                this.parse_page_torrents( torrentPage );
            }
        }

        private int get_series_torrents_pages_count()
        {
            var torrentPage = this.get_series_torrent_page( 1 );

            if ( String.Compare( torrentPage, "", StringComparison.Ordinal ) == 0 )
            {
                return 1;
            }

            var pagesButtons = Regex.Matches( torrentPage, "<a rel=\"nofollow\" href=\".*?\" class=\"turnoverButton siteButton bigButton\">(.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Singleline );

            if ( pagesButtons.Count <= 0 )
            {
                return 1;
            }

            var lastPage = pagesButtons[ pagesButtons.Count - 1 ];
            return int.Parse( lastPage.Groups[ 1 ].Value );
        }

        private string get_series_torrent_page( int page )
        {
            var tempFile = Factory.Instance.AppDataFolder + "/kat/" + this._seriesnameClean + "/page" + page;
            string torrentPage;

            if ( File.Exists( tempFile ) == false )
            {
                torrentPage = Factory.Instance.Utils.FetchDeCompressed( "https://kat.cr" + this._serieslink + "torrents/" );

                if ( torrentPage == null )
                {
                    return "";
                }

                Factory.Instance.Utils.write_file( tempFile, torrentPage );
                return torrentPage;
            }

            if ( ( File.GetLastWriteTime( tempFile ).Ticks/TimeSpan.TicksPerSecond + ( Factory.Instance.Settings.CacheTimeout ) ) > ( DateTime.Now.Ticks/TimeSpan.TicksPerSecond ) )
            {
                return File.ReadAllText( tempFile );
            }

            torrentPage = Factory.Instance.Utils.FetchDeCompressed( "https://kat.cr" + this._serieslink + "torrents/" );

            if ( torrentPage == null )
            {
                return "";
            }

            File.Delete( tempFile );

            return torrentPage;
        }

        private void parse_page_torrents( string page )
        {
            foreach ( Match match in Regex.Matches( page, "<tr.*?>(.*?)</tr>", RegexOptions.IgnoreCase | RegexOptions.Singleline ) )
            {
                var epname = Regex.Match( match.Groups[ 1 ].Value, "<a href=\".*?\" class=\"cellMainLink\">(.*?)</a>" );
                var epmagnet = Regex.Match( match.Groups[ 1 ].Value, "\"(magnet:.*?)\"" );

                if ( !epmagnet.Success || !epname.Success )
                {
                    continue;
                }

                var torrent = new Torrent {Epname = epname.Groups[ 1 ].Value.Trim(), Magnetlink = epmagnet.Groups[ 1 ].Value.Trim(), Series = this._seriesname, Imbdid = this._imdbid};
                Factory.Instance.Torrents.Add( torrent );
            }
        }
    }
}