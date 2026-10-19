using System;
using System.Net;

namespace FileBotPP.Helpers
{
    internal static class WebProxyConfig
    {
        public static void Apply( Action< IWebProxy > assign )
        {
            if ( string.Compare( Factory.Instance.Settings.ProxyServerHost, "", StringComparison.Ordinal ) != 0 )
            {
                var wp = new WebProxy( Factory.Instance.Settings.ProxyServerHost + ":" + Factory.Instance.Settings.ProxyServerPort, true );
                assign( wp );
            }
            else
            {
                assign( null );
            }
        }
    }
}
