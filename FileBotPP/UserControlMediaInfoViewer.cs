using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using FileBotPP.Metadata;

namespace FileBotPP
{
    /// <summary>
    ///     Interaction logic for UserControlMediaInfoViewer.xaml
    /// </summary>
    public partial class UserControlMediaInfoViewer
    {
        private readonly IMediaInfo _mediaInfo;

        public UserControlMediaInfoViewer( IMediaInfo mediaInfo, String filename )
        {
            this._mediaInfo = mediaInfo;
            this.InitializeComponent();
            this.FileLabel.Content = filename;
        }

        private void UserControl_Loaded( object sender, RoutedEventArgs e )
        {
            var mediaFields = typeof (MediaInfo).GetProperties();

            var vidcnt = 0;
            var gencnt = 0;
            var audcnt = 0;
            var txtcnt = 0;

            foreach ( var field in mediaFields )
            {
                AddMediaField( this.GeneralGrid, ref gencnt, "General", field );
                AddMediaField( this.VideoGrid, ref vidcnt, "Video", field );
                AddMediaField( this.AudioGrid, ref audcnt, "Audio", field );
                AddMediaField( this.TextGrid, ref txtcnt, "Text", field );
            }
        }

        private void AddMediaField( Grid grid, ref int rowIndex, string prefix, System.Reflection.PropertyInfo field )
        {
            if ( field.Name.StartsWith( prefix ) == false || field.GetValue( this._mediaInfo ) == null )
            {
                return;
            }

            var fieldname = Regex.Replace( field.Name.Substring( prefix.Length ), "(\\B[A-Z])", " $1" );
            var row = new RowDefinition {Height = GridLength.Auto};
            var labelkey = new TextBlock {Text = fieldname, FontWeight = FontWeights.Bold};
            var labelvalue = new TextBlock {Text = field.GetValue( this._mediaInfo )?.ToString(), TextWrapping = TextWrapping.Wrap};

            grid.RowDefinitions.Add( row );

            Grid.SetRow( labelkey, rowIndex );
            Grid.SetColumn( labelkey, 0 );

            Grid.SetRow( labelvalue, rowIndex );
            Grid.SetColumn( labelvalue, 1 );

            grid.Children.Add( labelkey );
            grid.Children.Add( labelvalue );

            rowIndex++;
        }
    }
}