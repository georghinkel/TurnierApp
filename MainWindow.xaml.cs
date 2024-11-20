using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TurnierApp.ViewModels;

namespace TurnierApp
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainViewModel_RankingChanged(object sender, EventArgs e)
        {
            var collectionView = (CollectionView)CollectionViewSource.GetDefaultView(PlayerList.ItemsSource);
            var main = (MainViewModel)DataContext;
            if (main.Tournament == null) return;
            var scoreSortDirection = main.Tournament.MoreIsBetter ? ListSortDirection.Descending : ListSortDirection.Ascending;
            if (collectionView.SortDescriptions.Count == 0)
            {
                collectionView.SortDescriptions.Add(new SortDescription("SumRank", ListSortDirection.Descending));
                collectionView.SortDescriptions.Add(new SortDescription("SumScore", scoreSortDirection));
            }
            else
            {
                var sortDesc = collectionView.SortDescriptions[1];
                if (sortDesc.Direction != scoreSortDirection)
                {
                    collectionView.SortDescriptions.RemoveAt(1);
                    collectionView.SortDescriptions.Add(new SortDescription("SumScore", scoreSortDirection));
                }
            }
            collectionView.Refresh();
        }
    }
}
