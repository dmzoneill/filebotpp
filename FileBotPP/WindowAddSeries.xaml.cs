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

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Factory.Instance.AddSeriesName = null;
            this.PopulateComboBox();
        }

        private void PopulateComboBox()
        {
            try
            {
                if (Factory.Instance.Eztv == null)
                {
                    return;
                }

                var got = Factory.Instance.ItemProvider.Items.Select(item => item.FullName).ToList();
                var want = new List<string>();

                foreach (var torrent in Factory.Instance.Torrents.Where(torrent => got.Contains(torrent.Series) == false).Where(torrent => want.Contains(torrent.Series) == false))
                {
                    want.Add(torrent.Series);
                }

                this.ComboBox.Items.Add(SeriesComboItem("Series Names", "#Torrents", null, false));

                foreach (var gotitem in want)
                {
                    var numtorrents = Factory.Instance.Torrents.Count(torrent => String.Compare(torrent.Series, gotitem, StringComparison.Ordinal) == 0);
                    this.ComboBox.Items.Add(SeriesComboItem(gotitem, numtorrents.ToString(), gotitem, true));
                }

                this.ComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Factory.Instance.LogLines.Enqueue(ex.Message);
                Factory.Instance.LogLines.Enqueue(ex.StackTrace);
            }
        }

        private static ComboBoxItem SeriesComboItem(string name, string count, object tag, bool sized)
        {
            var tbname = sized ? new TextBlock { Text = name, Width = 280 } : new TextBlock { Text = name };
            var tbnum = sized ? new TextBlock { Text = count, HorizontalAlignment = HorizontalAlignment.Right } : new TextBlock { Text = count };
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(290) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            grid.Children.Add(tbnum);
            grid.Children.Add(tbname);
            Grid.SetColumn(tbname, 0);
            Grid.SetColumn(tbnum, 1);
            return new ComboBoxItem { Content = grid, Tag = tag };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sn = ((ComboBoxItem)this.ComboBox.SelectedItem).Tag;
                var stag = sn?.ToString();
                Factory.Instance.AddSeriesName = stag;
            }
            catch (Exception ex)
            {
                Factory.Instance.LogLines.Enqueue(ex.Message);
                Factory.Instance.LogLines.Enqueue(ex.StackTrace);
                Factory.Instance.AddSeriesName = null;
            }

            this.Close();
        }
    }
}