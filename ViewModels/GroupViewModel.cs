using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TurnierApp.Models;
using TurnierApp.Properties;

namespace TurnierApp.ViewModels
{
    public class GroupViewModel : ViewModelBase
    {
        private TournamentGroup _tournament;
        private readonly Group _group;
        private readonly ObservableCollection<TableViewModel> _tables = new ObservableCollection<TableViewModel>();

        public GroupViewModel(Group group, TournamentGroup tournament)
        {
            _tournament = tournament;
            _group = group;

            CompleteGroupCommand = new DelegateCommand(CompleteGroup);
            ReplanCommand = new DelegateCommand(Replan);

            AddTables();
        }

        private void AddTables()
        {
            foreach (var table in _tournament.Tables)
            {
                Tables.Add(new TableViewModel(table));
            }
        }

        public string Name => _group.Name;

        public TournamentGroup Tournament
        {
            get => _tournament;
            private set => Set(ref _tournament, value);
        }

        private void CompleteGroup()
        {
            _group.Tournament.Complete(_group);
        }

        private void Replan()
        {
            _group.Tournament.PlanGroup(_group);
            Tables.Clear();
            AddTables();
        }

        public ICommand ReplanCommand { get; }

        public ICommand CompleteGroupCommand { get; }

        public ObservableCollection<TableViewModel> Tables => _tables;


        public void InitializeTournament()
        {
            Tournament.RankingChanged += Tournament_RankingChanged;
            Tables.Clear();
            foreach (var table in Tournament.Tables)
            {
                Tables.Add(new TableViewModel(table));
            }
        }

        private void Tournament_RankingChanged(object sender, EventArgs e)
        {
            RankingChanged?.Invoke(this, e);
        }

        public event EventHandler RankingChanged;
    }
}
