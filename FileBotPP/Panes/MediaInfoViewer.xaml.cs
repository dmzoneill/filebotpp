using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using FileBotPP.Metadata;
using FileBotPP.Metadata.Interfaces;

namespace FileBotPP.Panes
{
    /// <summary>
    ///     Interaction logic for MediaInfoViewer.xaml
    /// </summary>
    public partial class MediaInfoViewer
    {
        private readonly IMediaInfo _mediaInfo;

        public MediaInfoViewer( IMediaInfo mediaInfo, String filename )
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
                if ( field.Name.StartsWith( "General" ) )
                {
                    if ( field.GetValue( this._mediaInfo ) == null )
                    {
                        continue;
                    }

                    var fieldname = Regex.Replace( field.Name.Substring( 7 ), "(\\B[A-Z])", " $1" );
                    var row = new RowDefinition {Height = GridLength.Auto};
                    var labelkey = new TextBlock {Text = fieldname, FontWeight = FontWeights.Bold};
                    var labelvalue = new TextBlock {Text = field.GetValue( this._mediaInfo )?.ToString(), TextWrapping = TextWrapping.Wrap};

                    this.GeneralGrid.RowDefinitions.Add( row );

                    Grid.SetRow( labelkey, gencnt );
                    Grid.SetColumn( labelkey, 0 );

                    Grid.SetRow( labelvalue, gencnt );
                    Grid.SetColumn( labelvalue, 1 );

                    this.GeneralGrid.Children.Add( labelkey );
                    this.GeneralGrid.Children.Add( labelvalue );

                    gencnt++;
                }
                if ( field.Name.StartsWith( "Video" ) )
                {
                    if ( field.GetValue( this._mediaInfo ) == null )
                    {
                        continue;
                    }

                    var fieldname = Regex.Replace( field.Name.Substring( 5 ), "(\\B[A-Z])", " $1" );
                    var row = new RowDefinition {Height = GridLength.Auto};
                    var labelkey = new TextBlock {Text = fieldname, FontWeight = FontWeights.Bold};
                    var labelvalue = new TextBlock {Text = field.GetValue( this._mediaInfo )?.ToString(), TextWrapping = TextWrapping.Wrap};

                    this.VideoGrid.RowDefinitions.Add( row );

                    Grid.SetRow( labelkey, vidcnt );
                    Grid.SetColumn( labelkey, 0 );

                    Grid.SetRow( labelvalue, vidcnt );
                    Grid.SetColumn( labelvalue, 1 );

                    this.VideoGrid.Children.Add( labelkey );
                    this.VideoGrid.Children.Add( labelvalue );

                    vidcnt++;
                }
                if ( field.Name.StartsWith( "Audio" ) )
                {
                    if ( field.GetValue( this._mediaInfo ) == null )
                    {
                        continue;
                    }

                    var fieldname = Regex.Replace( field.Name.Substring( 5 ), "(\\B[A-Z])", " $1" );
                    var row = new RowDefinition {Height = GridLength.Auto};
                    var labelkey = new TextBlock {Text = fieldname, FontWeight = FontWeights.Bold};
                    var labelvalue = new TextBlock {Text = field.GetValue( this._mediaInfo )?.ToString(), TextWrapping = TextWrapping.Wrap};

                    this.AudioGrid.RowDefinitions.Add( row );

                    Grid.SetRow( labelkey, audcnt );
                    Grid.SetColumn( labelkey, 0 );

                    Grid.SetRow( labelvalue, audcnt );
                    Grid.SetColumn( labelvalue, 1 );

                    this.AudioGrid.Children.Add( labelkey );
                    this.AudioGrid.Children.Add( labelvalue );

                    audcnt++;
                }
                if ( field.Name.StartsWith( "Text" ) )
                {
                    if ( field.GetValue( this._mediaInfo ) == null )
                    {
                        continue;
                    }

                    var fieldname = Regex.Replace( field.Name.Substring( 4 ), "(\\B[A-Z])", " $1" );
                    var row = new RowDefinition {Height = GridLength.Auto};
                    var labelkey = new TextBlock {Text = fieldname, FontWeight = FontWeights.Bold};
                    var labelvalue = new TextBlock {Text = field.GetValue( this._mediaInfo )?.ToString(), TextWrapping = TextWrapping.Wrap};

                    this.TextGrid.RowDefinitions.Add( row );

                    Grid.SetRow( labelkey, txtcnt );
                    Grid.SetColumn( labelkey, 0 );

                    Grid.SetRow( labelvalue, txtcnt );
                    Grid.SetColumn( labelvalue, 1 );

                    this.TextGrid.Children.Add( labelkey );
                    this.TextGrid.Children.Add( labelvalue );

                    txtcnt++;
                }
            }
        }
    }
}