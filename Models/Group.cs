using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TurnierApp.ViewModels;

namespace TurnierApp.Models
{
    public class Group : ViewModelBase
    {
        private string _name;
        private readonly ObservableCollection<PlayerPlacement> _placements = new ObservableCollection<PlayerPlacement>();
        private readonly Tournament _tournament;
        private TournamentGroup _tournamentGroup;
        private bool _isCompleted;
        private int _tables = 1;
        private int _rounds = 15;
        private int? _maxPlayersPerTable = 2;

        public Group(Tournament tournament)
        {
            _tournament = tournament;

            AddPlacementCommand = new DelegateCommand(AddPlacement);
            RemoveCommand = new DelegateCommand(Remove);
        }

        public Tournament Tournament => _tournament;

        public ICommand AddPlacementCommand { get; }

        public ICommand RemoveCommand { get; }

        private void AddPlacement()
        {
            _placements.Add(new PlayerPlacement(this));
        }

        private void Remove()
        {
            _tournament.Groups.Remove(this);
            if (_tournamentGroup != null )
            {
                var thisGroup = _tournament.ActiveGroups.FirstOrDefault(g => g.Tournament == _tournamentGroup);
                if (thisGroup != null)
                {
                    _tournament.ActiveGroups.Remove(thisGroup);
                }
            }
        }

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        public ICollection<PlayerPlacement> Placements => _placements;

        public TournamentGroup Plan
        {
            get => _tournamentGroup;
            set => Set(ref _tournamentGroup, value);
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set => Set(ref _isCompleted, value);
        }

        public bool HasPlacements => Placements.Count > 0;

        public int Tables
        {
            get => _tables;
            set => Set(ref _tables, value);
        }

        public int? MaxPlayersPerTable
        {
            get => _maxPlayersPerTable;
            set => Set(ref _maxPlayersPerTable, value);
        }

        public int Rounds
        {
            get => _rounds;
            set => Set(ref _rounds, value);
        }
    }
}
