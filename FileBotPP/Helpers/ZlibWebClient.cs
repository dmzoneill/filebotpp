using System;
using System.Net;

namespace FileBotPP.Helpers
{
    internal class ZlibWebClient : WebClient
    {
        protected override WebRequest GetWebRequest( Uri address )
        {
            var request = base.GetWebRequest( address ) as HttpWebRequest;

            if ( request == null )
            {
                return null;
            }

            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            return request;
        }
    }
}