using System;
using System.Text.RegularExpressions;
using FileBotPP.Helpers;

namespace FileBotPP.Metadata
{
    public class Kat : SeriesMetadataFetch< IKatWorker >, IKat
    {
        protected override string LogMessage
        {
            get { return @"Fetching Kat metadata..."; }
        }

        protected override MatchCollection LoadMatches()
        {
            var data = Factory.Instance.Utils.FetchDeCompressed( "https://kat.cr/tv/show/" );

            if ( data == null )
            {
                return null;
            }

            return Regex.Matches( data, "<a class=\"plain\" href=\"(.*?)\">(.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Singleline );
        }

        protected override IKatWorker CreateWorker( Match option )
        {
            return new KatWorker( option.Groups[ 1 ].Value, option.Groups[ 2 ].Value );
        }

        protected override bool IsCached( IKatWorker worker )
        {
            return worker.is_cached();
        }

        protected override void RunWorker( IKatWorker worker )
        {
            worker.Run();
        }

        protected override bool IsWorking( IKatWorker worker )
        {
            return worker.is_working();
        }

        protected override void ReportFetchProgress( double percent )
        {
            Factory.Instance.WindowFileBotPp.set_kat_progress( percent + "%" );
        }

        protected override void ReportFinished()
        {
            Factory.Instance.WindowFileBotPp.set_kat_progress( "100%" );
            Factory.Instance.WindowFileBotPp.set_status_text( "Kat done..." );
        }
    }
}
