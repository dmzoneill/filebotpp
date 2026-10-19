using System.Text.RegularExpressions;
using FileBotPP.Helpers;

namespace FileBotPP.Metadata
{
    internal static class TorrentRows
    {
        public static void AddFromHtml( string html, string namePattern, string series, string imdbId )
        {
            foreach ( Match match in Regex.Matches( html, "<tr.*?>(.*?)</tr>", RegexOptions.IgnoreCase | RegexOptions.Singleline ) )
            {
                var epname = Regex.Match( match.Groups[ 1 ].Value, namePattern );
                var epmagnet = Regex.Match( match.Groups[ 1 ].Value, "\"(magnet:.*?)\"" );

                if ( !epmagnet.Success || !epname.Success )
                {
                    continue;
                }

                var torrent = new Torrent
                {
                    Epname = epname.Groups[ 1 ].Value.Trim(),
                    Magnetlink = epmagnet.Groups[ 1 ].Value.Trim(),
                    Series = series,
                    Imbdid = imdbId
                };
                Factory.Instance.Torrents.Add( torrent );
            }
        }
    }
}
