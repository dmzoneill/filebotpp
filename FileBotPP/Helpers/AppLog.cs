using System;

namespace FileBotPP.Helpers
{
    internal static class AppLog
    {
        public static void Exception( Exception ex )
        {
            Factory.Instance.LogLines.Enqueue( ex.Message );
            Factory.Instance.LogLines.Enqueue( ex.StackTrace );
        }
    }
}
