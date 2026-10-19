using System;
using System.IO;

namespace FileBotPP.Helpers
{
    internal static class CacheFiles
    {
        public static void EnsureDirectory( string path )
        {
            if ( Directory.Exists( path ) == false )
            {
                Directory.CreateDirectory( path );
            }
        }

        public static bool IsWithinTimeout( string path )
        {
            if ( File.Exists( path ) == false )
            {
                return false;
            }

            return ( File.GetLastWriteTime( path ).Ticks/TimeSpan.TicksPerSecond + Factory.Instance.Settings.CacheTimeout ) > ( DateTime.Now.Ticks/TimeSpan.TicksPerSecond );
        }

        public static bool IsFresh( string path )
        {
            if ( File.Exists( path ) == false )
            {
                return false;
            }

            if ( IsWithinTimeout( path ) )
            {
                return true;
            }

            File.Delete( path );
            return false;
        }

        public static string ReadIfFresh( string path, bool deleteIfStale )
        {
            if ( File.Exists( path ) == false )
            {
                return null;
            }

            if ( IsWithinTimeout( path ) )
            {
                return File.ReadAllText( path );
            }

            if ( deleteIfStale )
            {
                File.Delete( path );
            }

            return null;
        }
    }
}
