using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FileBotPP.Helpers;
using FileBotPP.Tree;

namespace FileBotPP
{
    /// <summary>
    ///     Interaction logic for AddSeriesWindow.xaml
    /// </summary>
    public partial class AddSeriesWindow
    {
        public AddSeriesWindow()
        {
            this.InitializeComponent();
        }

        private void Grid_Loaded( object sender, RoutedEventArgs e )
        {
            Common.AddSeriesName = null;
            this.PopulateComboBox();
        }

        private void PopulateComboBox()
        {
            try
            {
                if ( Common.Eztv == null )
                {
                    return;
                }

                var got = ItemProvider.Items.Select( item => item.FullName ).ToList();
                var want = new List< string >();

                foreach ( var torrent in Common.Eztv.get_torrents().Where( torrent => got.Contains( torrent.Series ) == false ).Where( torrent => want.Contains( torrent.Series ) == false ) )
                {
                    want.Add( torrent.Series );
                }

                var tbname = new TextBlock {Text = "Series Names"};
                var tbnum = new TextBlock {Text = "#Torrents"};
                var grid = new Grid();
                grid.ColumnDefinitions.Add( new ColumnDefinition {Width = new GridLength( 290 )} );
                grid.ColumnDefinitions.Add( new ColumnDefinition {Width = new GridLength( 60 )} );
                grid.Children.Add( tbnum );
                grid.Children.Add( tbname );
                Grid.SetColumn( tbname, 0 );
                Grid.SetColumn( tbnum, 1 );

                this.ComboBox.Items.Add( new ComboBoxItem {Content = grid, Tag = null} );

                foreach ( var gotitem in want )
                {
                    var numtorrents = Common.Eztv.get_torrents().Count( torrent => String.Compare( torrent.Series, gotitem, StringComparison.Ordinal ) == 0 );

                    tbname = new TextBlock {Text = gotitem, Width = 280};
                    tbnum = new TextBlock {Text = numtorrents.ToString(), HorizontalAlignment = HorizontalAlignment.Right};
                    grid = new Grid();
                    grid.ColumnDefinitions.Add( new ColumnDefinition {Width = new GridLength( 290 )} );
                    grid.ColumnDefinitions.Add( new ColumnDefinition {Width = new GridLength( 60 )} );
                    grid.Children.Add( tbnum );
                    grid.Children.Add( tbname );
                    Grid.SetColumn( tbname, 0 );
                    Grid.SetColumn( tbnum, 1 );

                    this.ComboBox.Items.Add( new ComboBoxItem {Content = grid, Tag = gotitem} );
                }

                this.ComboBox.SelectedIndex = 0;
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void Button_Click( object sender, RoutedEventArgs e )
        {
            try
            {
                var sn = ( ( ComboBoxItem ) this.ComboBox.SelectedItem ).Tag;
                var stag = sn?.ToString();
                Common.AddSeriesName = stag;
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
                Common.AddSeriesName = null;
            }

            this.Close();
        }
    }
}