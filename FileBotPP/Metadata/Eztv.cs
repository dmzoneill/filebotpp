using System.Text.RegularExpressions;
using FileBotPP.Helpers;

namespace FileBotPP.Metadata
{
    public class Eztv : SeriesMetadataFetch< IEztvWorker >, IEztv
    {
        protected override string LogMessage
        {
            get { return @"Fetching EZTV metadata..."; }
        }

        protected override MatchCollection LoadMatches()
        {
            var data = Factory.Instance.Utils.Fetch( "https://eztv.ag" );

            var selectMatch = Regex.Match( data, "<select.*?>(.*?)</select>", RegexOptions.IgnoreCase | RegexOptions.Singleline );
            if ( selectMatch.Success == false )
            {
                return null;
            }

            return Regex.Matches( selectMatch.Value, "<option value=\"(.*?)\">(.*?)</option>", RegexOptions.IgnoreCase | RegexOptions.Singleline );
        }

        protected override IEztvWorker CreateWorker( Match option )
        {
            return new EztvWorker( int.Parse( option.Groups[ 1 ].Value ), option.Groups[ 2 ].Value );
        }

        protected override bool IsCached( IEztvWorker worker )
        {
            return worker.is_cached();
        }

        protected override void RunWorker( IEztvWorker worker )
        {
            worker.Run();
        }

        protected override bool IsWorking( IEztvWorker worker )
        {
            return worker.is_working();
        }

        protected override void ReportFetchProgress( double percent )
        {
            Factory.Instance.WindowFileBotPp.set_eztv_progress( percent + "%" );
        }

        protected override void ReportFinished()
        {
            Factory.Instance.WindowFileBotPp.set_eztv_progress( "100%" );
            Factory.Instance.WindowFileBotPp.set_status_text( "Eztv done..." );
        }
    }
}
