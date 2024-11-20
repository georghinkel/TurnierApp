using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TurnierApp.Models
{
    public class PlayerRound : ViewModelBase
    {
        private int? _score;
        private int _rank;
        public event EventHandler ScoreChanged;
        public event EventHandler RankChanged;

        public PlayerRound(Player player)
        {
            Player = player;
        }

        public Player Player { get; }

        public int PlayerCount { get; set; }

        public Round Round { get; set; }

        public string Summary => $"{Round.Title} an {Round.Table.Title} gegen {string.Join(", ", Round.Players.Except(Enumerable.Repeat(this, 1)).Select(pr => pr.Player.Name))}";

        public void RaiseSummaryChanged()
        {
           OnPropertyChanged(nameof(Summary));
        }

        public int Rank
        {
            get => _rank;
            set
            {
                if (Set(ref _rank, value)) RankChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int? Score 
        {
            get => _score;
            set
            {
                if (Set(ref _score, value)) ScoreChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
