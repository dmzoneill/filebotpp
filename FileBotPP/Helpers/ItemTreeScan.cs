using System;
using System.Linq;
using FileBotPP.Tree;

namespace FileBotPP.Helpers
{
    public static class ItemTreeScan
    {
        public static void Run( IDirectoryItem directory, IFileItem file, Func< bool > stopped, Action< int > setCount, Action< IItem > processFolder, Action< IFileItem > processFile )
        {
            if ( directory == null && file == null )
            {
                setCount( Factory.Instance.ItemProvider.Count() );

                foreach ( var dir in Factory.Instance.ItemProvider.Items.OfType< IDirectoryItem >().ToArray() )
                {
                    if ( stopped() )
                    {
                        return;
                    }

                    processFolder( dir );
                }
            }

            if ( directory != null )
            {
                if ( stopped() )
                {
                    return;
                }

                setCount( directory.Count );
                processFolder( directory );
            }

            if ( file == null )
            {
                return;
            }

            if ( stopped() )
            {
                return;
            }

            setCount( file.Count );
            processFile( file );
        }

        public static void WalkChildren( IItem directory, Func< bool > stopped, bool honorStop, Action< IItem > processFolder, Action< IFileItem > processFile )
        {
            foreach ( var item in directory.Items.OfType< IDirectoryItem >() )
            {
                if ( honorStop && stopped() )
                {
                    return;
                }

                processFolder( item );
            }

            foreach ( var item in directory.Items.OfType< IFileItem >().Where( item => item.Missing != true ) )
            {
                if ( honorStop && stopped() )
                {
                    return;
                }

                processFile( item );
            }
        }
    }
}
