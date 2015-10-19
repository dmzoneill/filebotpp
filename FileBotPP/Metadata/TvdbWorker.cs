using System;
using System.ComponentModel;
using System.IO;
using System.Xml;
using FileBotPP.Helpers;
using FileBotPP.Interfaces;
using FileBotPP.Metadata.tvdb;

namespace FileBotPP.Metadata
{
    internal class TvdbWorker : ITvdbWorker
    {
        private readonly string _seriesName;
        private ISeries _series;
        private int _seriesid = -1;
        private bool _working;
        private string _xml;

        public TvdbWorker( string series )
        {
            this._seriesName = series;
        }

        public void Run()
        {
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
            return this._seriesName;
        }

        public bool is_cached()
        {
            try
            {
                this.get_series_id_local();

                var cticks = DateTime.Now.Ticks/TimeSpan.TicksPerSecond;
                var tempFile1 = Common.AppDataFolder + "/tvdb/" + this._seriesid;

                var tempFile2 = Common.AppDataFolder + "/tvdbids/" + this._seriesName;

                if ( !File.Exists( tempFile1 ) || !File.Exists( tempFile2 ) )
                {
                    return false;
                }

                return ( File.GetLastWriteTime( tempFile1 ).Ticks/TimeSpan.TicksPerSecond + Settings.CacheTimeout ) > cticks && ( File.GetLastWriteTime( tempFile2 ).Ticks/TimeSpan.TicksPerSecond + Settings.CacheTimeout ) > cticks;
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
                return false;
            }
        }

        public void parse_series_data()
        {
            try
            {
                Utils.LogLines.Enqueue( @"Parsing " + this._seriesName + @" metadata..." );

                var document = new XmlDocument();
                document.LoadXml( this._xml );
                this._series = new Series( this._seriesName );

                this.parse_series_episodes_metadata( document );
                this.parse_series_metadata( document );

                this._xml = null;
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void Worker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            this._working = false;
        }

        private void Worker_DoWork( object sender, DoWorkEventArgs e )
        {
            this._working = true;
            try
            {
                this.get_series_id();

                if ( this._seriesid > 0 )
                {
                    this.get_series_data();

                    if ( this._xml != null )
                    {
                        this.parse_series_data();
                    }
                }
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        public ISeries get_series()
        {
            return this._series;
        }

        private void get_series_id()
        {
            try
            {
                if ( !Directory.Exists( Common.AppDataFolder + "/tvdbids/" ) )
                {
                    Directory.CreateDirectory( Common.AppDataFolder + "/tvdbids" );
                }

                var tempFile = Common.AppDataFolder + "/tvdbids/" + this._seriesName;

                string xml;

                if ( File.Exists( tempFile ) )
                {
                    if ( ( File.GetLastWriteTime( tempFile ).Ticks/TimeSpan.TicksPerSecond + ( Settings.CacheTimeout ) ) > ( DateTime.Now.Ticks/TimeSpan.TicksPerSecond ) )
                    {
                        xml = File.ReadAllText( tempFile );
                    }
                    else
                    {
                        File.Delete( tempFile );
                        xml = Utils.Fetch( "http://thetvdb.com/api/GetSeries.php?seriesname=" + this._seriesName );
                    }
                }
                else
                {
                    xml = Utils.Fetch( "http://thetvdb.com/api/GetSeries.php?seriesname=" + this._seriesName );
                }

                Utils.write_file( tempFile, xml );

                this.parse_Series_id( xml );
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void get_series_id_local()
        {
            try
            {
                if ( !Directory.Exists( Common.AppDataFolder + "/tvdbids/" ) )
                {
                    Directory.CreateDirectory( Common.AppDataFolder + "/tvdbids" );
                }

                var tempFile = Common.AppDataFolder + "/tvdbids/" + this._seriesName;

                var xml = "";

                if ( File.Exists( tempFile ) )
                {
                    if ( ( File.GetLastWriteTime( tempFile ).Ticks/TimeSpan.TicksPerSecond + ( Settings.CacheTimeout ) ) > ( DateTime.Now.Ticks/TimeSpan.TicksPerSecond ) )
                    {
                        xml = File.ReadAllText( tempFile );
                    }
                }

                if ( String.Compare( xml, "", StringComparison.Ordinal ) == 0 )
                {
                    return;
                }

                this.parse_Series_id( xml );
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void parse_Series_id( string xml )
        {
            try
            {
                var document = new XmlDocument();
                document.LoadXml( xml );

                var ids = document.SelectNodes( ".//seriesid" );
                var names = document.SelectNodes( ".//SeriesName" );

                if ( ids == null || names == null )
                {
                    return;
                }

                for ( var x = 0; x < ids.Count; x++ )
                {
                    var xmlname = names[ x ].InnerText.ToLower().Trim();
                    var searchname = this._seriesName.ToLower().Trim();

                    if ( xmlname != searchname )
                    {
                        continue;
                    }

                    int id;
                    int.TryParse( ids[ x ].InnerText.Trim(), out id );

                    if ( id <= -1 )
                    {
                        continue;
                    }

                    this._seriesid = id;
                    break;
                }
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        // ReSharper disable once FunctionComplexityOverflow
        private void parse_series_metadata( XmlNode document )
        {
            try
            {
                var seriesdata = document.SelectSingleNode( ".//Series" );

                if ( seriesdata == null )
                {
                    return;
                }

                this._series.Id = seriesdata.SelectSingleNode( ".//id" )?.InnerText.Trim();
                this._series.Actors = seriesdata.SelectSingleNode( ".//Actors" )?.InnerText.Trim();
                this._series.AirsDayOfWeek = seriesdata.SelectSingleNode( ".//Airs_DayOfWeek" )?.InnerText.Trim();
                this._series.AirsTime = seriesdata.SelectSingleNode( ".//Airs_Time" )?.InnerText.Trim();
                this._series.ContentRating = seriesdata.SelectSingleNode( ".//ContentRating" )?.InnerText.Trim();
                this._series.FirstAired = seriesdata.SelectSingleNode( ".//FirstAired" )?.InnerText.Trim();
                this._series.Genre = seriesdata.SelectSingleNode( ".//Genre" )?.InnerText.Trim();
                this._series.ImdbId = seriesdata.SelectSingleNode( ".//IMDB_ID" )?.InnerText.Trim();
                this._series.Language = seriesdata.SelectSingleNode( ".//Language" )?.InnerText.Trim();
                this._series.Network = seriesdata.SelectSingleNode( ".//NetworkID" )?.InnerText.Trim();
                this._series.NetworkId = seriesdata.SelectSingleNode( ".//Series" )?.InnerText.Trim();
                this._series.Overview = seriesdata.SelectSingleNode( ".//Overview" )?.InnerText.Trim();
                this._series.Rating = seriesdata.SelectSingleNode( ".//Rating" )?.InnerText.Trim();
                this._series.RatingCount = seriesdata.SelectSingleNode( ".//RatingCount" )?.InnerText.Trim();
                this._series.Runtime = seriesdata.SelectSingleNode( ".//Runtime" )?.InnerText.Trim();
                this._series.SeriesId = seriesdata.SelectSingleNode( ".//SeriesID" )?.InnerText.Trim();
                this._series.SeriesName = seriesdata.SelectSingleNode( ".//SeriesName" )?.InnerText.Trim();
                this._series.Status = seriesdata.SelectSingleNode( ".//Status" )?.InnerText.Trim();
                this._series.Added = seriesdata.SelectSingleNode( ".//added" )?.InnerText.Trim();
                this._series.AddedBy = seriesdata.SelectSingleNode( ".//addedBy" )?.InnerText.Trim();
                this._series.Banner = seriesdata.SelectSingleNode( ".//banner" )?.InnerText.Trim();
                this._series.Fanart = seriesdata.SelectSingleNode( ".//fanart" )?.InnerText.Trim();
                this._series.Lastupdated = seriesdata.SelectSingleNode( ".//lastupdated" )?.InnerText.Trim();
                this._series.Poster = seriesdata.SelectSingleNode( ".//poster" )?.InnerText.Trim();
                this._series.TmsWantedOld = seriesdata.SelectSingleNode( ".//tms_wanted_old" )?.InnerText.Trim();
                this._series.Zap2ItId = seriesdata.SelectSingleNode( ".//zap2it_id" )?.InnerText.Trim();
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void parse_series_episodes_metadata( XmlNode document )
        {
            try
            {
                var episodes = document.SelectNodes( ".//Episode" );

                if ( episodes == null )
                {
                    return;
                }

                var currentDate = DateTime.Now;

                foreach ( XmlNode episode in episodes )
                {
                    var epname = episode.SelectSingleNode( ".//EpisodeName" )?.InnerText.Trim();
                    var epseasonnum = episode.SelectSingleNode( ".//SeasonNumber" )?.InnerText.Trim();
                    var epaireddate = episode.SelectSingleNode( ".//FirstAired" )?.InnerText.Trim();
                    var epepisodenum = episode.SelectSingleNode( ".//EpisodeNumber" )?.InnerText.Trim();

                    var airdate = ( epaireddate == "" ) ? Convert.ToDateTime( "2050-01-01" ) : Convert.ToDateTime( epaireddate );

                    if ( epseasonnum == null )
                    {
                        continue;
                    }

                    var epsnum = int.Parse( epseasonnum );

                    if ( epepisodenum == null )
                    {
                        continue;
                    }

                    var epnum = int.Parse( epepisodenum );

                    if ( airdate > currentDate || epsnum <= 0 )
                    {
                        continue;
                    }

                    var season = this._series.get_season( epsnum );
                    var newepisode = new Episode( epnum, epname );
                    season.add_episode( newepisode );
                }
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void get_series_data()
        {
            try
            {
                Utils.LogLines.Enqueue( @"Download " + this._seriesName + @" metadata..." );

                if ( !Directory.Exists( Common.AppDataFolder + "/tvdb/" ) )
                {
                    Directory.CreateDirectory( Common.AppDataFolder + "/tvdb" );
                }

                var tempFile = Common.AppDataFolder + "/tvdb/" + this._seriesid;

                if ( File.Exists( tempFile ) )
                {
                    if ( ( File.GetLastWriteTime( tempFile ).Ticks/TimeSpan.TicksPerSecond + ( Settings.CacheTimeout ) ) > ( DateTime.Now.Ticks/TimeSpan.TicksPerSecond ) )
                    {
                        this._xml = Utils.read_file_from_zip( tempFile, "en.xml" );
                        return;
                    }

                    File.Delete( tempFile );
                }

                var download = Utils.download_file( "http://thetvdb.com/api/" + Settings.TvdbApiKey + "/series/" + this._seriesid + "/all/en.zip", tempFile );

                if ( download == false )
                {
                    return;
                }

                this._xml = Utils.read_file_from_zip( tempFile, "en.xml" );
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }
    }
}