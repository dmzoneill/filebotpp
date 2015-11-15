using System;
using System.ComponentModel;
using System.IO;
using System.Xml;
using FileBotPP.Helpers;

namespace FileBotPP.Metadata
{
    internal class TvdbWorker : ITvdbWorker
    {
        private readonly string _seriesName;
        private int _seriesid = -1;
        private ITvdbSeries _tvdbSeries;
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
                var tempFile1 = Factory.Instance.AppDataFolder + "/tvdb/" + this._seriesid;

                var tempFile2 = Factory.Instance.AppDataFolder + "/tvdbids/" + this._seriesName;

                if ( !File.Exists( tempFile1 ) || !File.Exists( tempFile2 ) )
                {
                    return false;
                }

                return ( File.GetLastWriteTime( tempFile1 ).Ticks/TimeSpan.TicksPerSecond + Factory.Instance.Settings.CacheTimeout ) > cticks && ( File.GetLastWriteTime( tempFile2 ).Ticks/TimeSpan.TicksPerSecond + Factory.Instance.Settings.CacheTimeout ) > cticks;
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
                return false;
            }
        }

        public void parse_series_data()
        {
            try
            {
                Factory.Instance.LogLines.Enqueue( @"Parsing " + this._seriesName + @" metadata..." );

                var document = new XmlDocument();
                document.LoadXml( this._xml );
                this._tvdbSeries = new TvdbSeries( this._seriesName );

                this.parse_series_episodes_metadata( document );
                this.parse_series_metadata( document );
                this.download_artwork();

                this._xml = null;
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        public ITvdbSeries get_series()
        {
            return this._tvdbSeries;
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
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void get_series_id()
        {
            try
            {
                if ( !Directory.Exists( Factory.Instance.AppDataFolder + "/tvdbids/" ) )
                {
                    Directory.CreateDirectory( Factory.Instance.AppDataFolder + "/tvdbids" );
                }

                var tempFile = Factory.Instance.AppDataFolder + "/tvdbids/" + this._seriesName;

                string xml;

                if ( File.Exists( tempFile ) )
                {
                    if ( ( File.GetLastWriteTime( tempFile ).Ticks/TimeSpan.TicksPerSecond + ( Factory.Instance.Settings.CacheTimeout ) ) > ( DateTime.Now.Ticks/TimeSpan.TicksPerSecond ) )
                    {
                        xml = File.ReadAllText( tempFile );
                    }
                    else
                    {
                        File.Delete( tempFile );
                        xml = Factory.Instance.Utils.Fetch( "http://thetvdb.com/api/GetSeries.php?seriesname=" + this._seriesName );
                    }
                }
                else
                {
                    xml = Factory.Instance.Utils.Fetch( "http://thetvdb.com/api/GetSeries.php?seriesname=" + this._seriesName );
                }

                Factory.Instance.Utils.write_file( tempFile, xml );

                this.parse_Series_id( xml );
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void get_series_id_local()
        {
            try
            {
                if ( !Directory.Exists( Factory.Instance.AppDataFolder + "/tvdbids/" ) )
                {
                    Directory.CreateDirectory( Factory.Instance.AppDataFolder + "/tvdbids" );
                }

                var tempFile = Factory.Instance.AppDataFolder + "/tvdbids/" + this._seriesName;

                var xml = "";

                if ( File.Exists( tempFile ) )
                {
                    if ( ( File.GetLastWriteTime( tempFile ).Ticks/TimeSpan.TicksPerSecond + ( Factory.Instance.Settings.CacheTimeout ) ) > ( DateTime.Now.Ticks/TimeSpan.TicksPerSecond ) )
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
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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

                this._tvdbSeries.Id = seriesdata.SelectSingleNode( ".//id" )?.InnerText.Trim();
                this._tvdbSeries.Actors = seriesdata.SelectSingleNode( ".//Actors" )?.InnerText.Trim();
                this._tvdbSeries.AirsDayOfWeek = seriesdata.SelectSingleNode( ".//Airs_DayOfWeek" )?.InnerText.Trim();
                this._tvdbSeries.AirsTime = seriesdata.SelectSingleNode( ".//Airs_Time" )?.InnerText.Trim();
                this._tvdbSeries.ContentRating = seriesdata.SelectSingleNode( ".//ContentRating" )?.InnerText.Trim();
                this._tvdbSeries.FirstAired = seriesdata.SelectSingleNode( ".//FirstAired" )?.InnerText.Trim();
                this._tvdbSeries.Genre = seriesdata.SelectSingleNode( ".//Genre" )?.InnerText.Trim();
                this._tvdbSeries.ImdbId = seriesdata.SelectSingleNode( ".//IMDB_ID" )?.InnerText.Trim();
                this._tvdbSeries.Language = seriesdata.SelectSingleNode( ".//Language" )?.InnerText.Trim();
                this._tvdbSeries.Network = seriesdata.SelectSingleNode( ".//NetworkID" )?.InnerText.Trim();
                this._tvdbSeries.NetworkId = seriesdata.SelectSingleNode( ".//Series" )?.InnerText.Trim();
                this._tvdbSeries.Overview = seriesdata.SelectSingleNode( ".//Overview" )?.InnerText.Trim();
                this._tvdbSeries.Rating = seriesdata.SelectSingleNode( ".//Rating" )?.InnerText.Trim();
                this._tvdbSeries.RatingCount = seriesdata.SelectSingleNode( ".//RatingCount" )?.InnerText.Trim();
                this._tvdbSeries.Runtime = seriesdata.SelectSingleNode( ".//Runtime" )?.InnerText.Trim();
                this._tvdbSeries.SeriesId = seriesdata.SelectSingleNode( ".//SeriesID" )?.InnerText.Trim();
                this._tvdbSeries.SeriesName = seriesdata.SelectSingleNode( ".//SeriesName" )?.InnerText.Trim();
                this._tvdbSeries.Status = seriesdata.SelectSingleNode( ".//Status" )?.InnerText.Trim();
                this._tvdbSeries.Added = seriesdata.SelectSingleNode( ".//added" )?.InnerText.Trim();
                this._tvdbSeries.AddedBy = seriesdata.SelectSingleNode( ".//addedBy" )?.InnerText.Trim();
                this._tvdbSeries.Banner = seriesdata.SelectSingleNode( ".//banner" )?.InnerText.Trim();
                this._tvdbSeries.Fanart = seriesdata.SelectSingleNode( ".//fanart" )?.InnerText.Trim();
                this._tvdbSeries.Lastupdated = seriesdata.SelectSingleNode( ".//lastupdated" )?.InnerText.Trim();
                this._tvdbSeries.Poster = seriesdata.SelectSingleNode( ".//poster" )?.InnerText.Trim();
                this._tvdbSeries.TmsWantedOld = seriesdata.SelectSingleNode( ".//tms_wanted_old" )?.InnerText.Trim();
                this._tvdbSeries.Zap2ItId = seriesdata.SelectSingleNode( ".//zap2it_id" )?.InnerText.Trim();
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void download_artwork()
        {
            try
            {
                Tvdb.FileDownloads.Enqueue( new[] {"http://thetvdb.com/banners/_cache/" + this._tvdbSeries.Poster, Factory.Instance.AppDataFolder + "/tvdbartwork/poster/" + this._tvdbSeries.Id + ".jpg"} );
                Tvdb.FileDownloads.Enqueue( new[] {"http://thetvdb.com/banners/_cache/" + this._tvdbSeries.Fanart, Factory.Instance.AppDataFolder + "/tvdbartwork/fanart/" + this._tvdbSeries.Id + ".jpg"} );
                Tvdb.FileDownloads.Enqueue( new[] {"http://thetvdb.com/banners/_cache/" + this._tvdbSeries.Banner, Factory.Instance.AppDataFolder + "/tvdbartwork/banner/" + this._tvdbSeries.Id + ".jpg"} );
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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

                    var season = this._tvdbSeries.get_season( epsnum );
                    var newepisode = new TvdbEpisode( epnum, epname );
                    season.add_episode( newepisode );
                }
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void get_series_data()
        {
            try
            {
                Factory.Instance.LogLines.Enqueue( @"Download " + this._seriesName + @" metadata..." );

                if ( !Directory.Exists( Factory.Instance.AppDataFolder + "/tvdb/" ) )
                {
                    Directory.CreateDirectory( Factory.Instance.AppDataFolder + "/tvdb" );
                }

                var tempFile = Factory.Instance.AppDataFolder + "/tvdb/" + this._seriesid;

                if ( File.Exists( tempFile ) )
                {
                    if ( ( File.GetLastWriteTime( tempFile ).Ticks/TimeSpan.TicksPerSecond + ( Factory.Instance.Settings.CacheTimeout ) ) > ( DateTime.Now.Ticks/TimeSpan.TicksPerSecond ) )
                    {
                        this._xml = Factory.Instance.Utils.read_file_from_zip( tempFile, "en.xml" );
                        return;
                    }

                    File.Delete( tempFile );
                }

                var download = Factory.Instance.Utils.download_file( "http://thetvdb.com/api/" + Factory.Instance.Settings.TvdbApiKey + "/Series/" + this._seriesid + "/all/en.zip", tempFile );

                if ( download == false )
                {
                    return;
                }

                this._xml = Factory.Instance.Utils.read_file_from_zip( tempFile, "en.xml" );
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }
    }
}