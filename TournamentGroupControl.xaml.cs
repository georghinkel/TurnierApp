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
using TurnierApp.Models;
using TurnierApp.ViewModels;

namespace TurnierApp
{
    /// <summary>
    /// Interaktionslogik für TournamentGroupControl.xaml
    /// </summary>
    public partial class TournamentGroupControl : UserControl
    {

        public TournamentGroupControl()
        {
            InitializeComponent();

            DataContextChanged += TournamentGroupControl_DataContextChanged;
        }

        private void TournamentGroupControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is GroupViewModel oldGroup)
            {
                oldGroup.RankingChanged -= OnRankingChanged;
            }
            if (e.NewValue is GroupViewModel newGroup)
            {
                newGroup.RankingChanged += OnRankingChanged;
            }
        }

        public bool MoreIsBetter
        {
            get
            {
                if (DataContext is GroupViewModel groupView)
                {
                    return groupView.Tournament.MoreIsBetter;
                }
                return true;
            }
        }

        private void OnRankingChanged(object sender, EventArgs e)
        {
            var collectionView = (CollectionView)CollectionViewSource.GetDefaultView(PlayerList.ItemsSource);
            var main = (MainViewModel)DataContext;
            if (main.Tournament == null) return;
            var scoreSortDirection = MoreIsBetter ? ListSortDirection.Descending : ListSortDirection.Ascending;
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
