using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FileBotPP.Helpers;

namespace FileBotPP
{
    /// <summary>
    ///     Interaction logic for WindowAddSeries.xaml
    /// </summary>
    public partial class WindowAddSeries
    {
        public WindowAddSeries()
        {
            this.InitializeComponent();
        }

        private void Grid_Loaded( object sender, RoutedEventArgs e )
        {
            Factory.Instance.AddSeriesName = null;
            this.PopulateComboBox();
        }

        private void PopulateComboBox()
        {
            try
            {
                if ( Factory.Instance.Eztv == null )
                {
                    return;
                }

                var got = Factory.Instance.ItemProvider.Items.Select( item => item.FullName ).ToList();
                var want = new List< string >();

                foreach ( var torrent in Factory.Instance.Eztv.get_torrents().Where( torrent => got.Contains( torrent.Series ) == false ).Where( torrent => want.Contains( torrent.Series ) == false ) )
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
                    var numtorrents = Factory.Instance.Eztv.get_torrents().Count( torrent => String.Compare( torrent.Series, gotitem, StringComparison.Ordinal ) == 0 );

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
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void Button_Click( object sender, RoutedEventArgs e )
        {
            try
            {
                var sn = ( ( ComboBoxItem ) this.ComboBox.SelectedItem ).Tag;
                var stag = sn?.ToString();
                Factory.Instance.AddSeriesName = stag;
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
                Factory.Instance.AddSeriesName = null;
            }

            this.Close();
        }
    }
}