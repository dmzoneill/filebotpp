using System;
using System.IO;

namespace FileBotPP.Tree
{
    internal static class ScannedFileItems
    {
        public static FileItem FromFile( FileInfo file, IItem parent, bool badLocationWhenRoot )
        {
            var shortname = file.Name;
            var extension = "";

            if ( file.Name.Contains( "." ) )
            {
                shortname = file.Name.Substring( 0, file.Name.LastIndexOf( ".", StringComparison.Ordinal ) );
                extension = file.Name.Substring( file.Name.LastIndexOf( ".", StringComparison.Ordinal ) + 1 );
            }

            var item = new FileItem
            {
                FullName = file.Name,
                ShortName = shortname,
                Extension = extension,
                Path = file.FullName,
                Parent = parent
            };

            if ( badLocationWhenRoot && parent == null )
            {
                item.BadLocation = true;
            }

            return item;
        }
    }
}
