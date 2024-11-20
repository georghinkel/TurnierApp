using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TurnierApp.Models;

namespace TurnierApp.ViewModels
{
    public class TableViewModel : ViewModelBase
    {
        private readonly Table _table;
        private int _index;

        private ICommand _nextCommand;
        private ICommand _previousCommand;

        private string _summary;
        private string _prevSummary;
        private string _nextSummary;

        public TableViewModel(Table table)
        {
            _table = table;

            _nextCommand = new DelegateCommand(Next);
            _previousCommand = new DelegateCommand(Previous);
            Update();
        }

        public ICommand NextCommand => _nextCommand;
        public ICommand PrevCommand => _previousCommand;

        public string Title => _table.Title;

        public void Next()
        {
            _index = Math.Min(_index + 1, _table.Rounds.Length - 1);
            Update();
        }

        public void Previous()
        {
            _index = Math.Max(_index - 1, 0);
            Update();
        }

        private void Update()
        {
            var round = _table.Rounds[_index];
            Summary = CalculateSummary(round);
            PrevSummary = _index > 0 ? "Vorherige: " + CalculateSummary(_table.Rounds[_index - 1]) : null;
            NextSummary = _index < _table.Rounds.Length - 1 ? "Nächste: " + CalculateSummary(_table.Rounds[_index + 1]) : null;
            OnPropertyChanged(nameof(Players));
        }

        private string CalculateSummary(Round round)
        {
            return string.Join(" vs. ", round.Players.Select(p => p.Player.Name));
        }

        public string Summary
        {
            get => _summary;
            private set => Set(ref _summary, value);
        }

        public string PrevSummary
        {
            get => _prevSummary;
            private set => Set(ref _prevSummary, value);
        }

        public string NextSummary
        {
            get => _nextSummary;
            private set => Set(ref _nextSummary, value);
        }

        public PlayerRound[] Players => _table.Rounds[_index].Players;
    }
}
