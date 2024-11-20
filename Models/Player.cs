using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurnierApp.Models
{
    [DebuggerDisplay("{Name}")]
    public class Player : ViewModelBase
    {
        private string _name;
        private PlayerRound[] _rounds;
        private double _sumRank;
        private int _sumScore;
        private int _completed;

        public string Name
        {
            get => _name;
            set
            {
                if (Set(ref _name, value) && _rounds != null)
                {
                    foreach (var round in _rounds)
                    {
                        foreach (var otherRound in round.Round.Players)
                        {
                            if (otherRound != round)
                            {
                                otherRound.RaiseSummaryChanged();
                            }
                        }
                    }
                }
            }
        }

        public double SumRank
        {
            get => _sumRank;
            private set => Set(ref _sumRank, value);
        }

        public int Completed
        {
            get => _completed;
            private set => Set(ref _completed, value);
        }

        public int SumScore
        {
            get => _sumScore;
            private set => Set(ref _sumScore, value);
        }

        public void Initialize(PlayerRound[] rounds)
        {
            _rounds = rounds.OrderBy(r => r.Round.Id).ToArray();

            foreach (var round in _rounds)
            {
                round.RankChanged += RankChanged;
                round.ScoreChanged += RankChanged;
            }
        }

        private void RankChanged(object sender, EventArgs e)
        {
            SumRank = Rounds.Sum(CalculateRankScore);
            SumScore = Rounds.Sum(r => r.Score ?? 0);
            ScoreChanged?.Invoke(this, EventArgs.Empty);
            Completed = Rounds.Where(r => r.Score.HasValue).Count();
        }

        public event EventHandler ScoreChanged;

        private double CalculateRankScore(PlayerRound round)
        {
            return (round.PlayerCount - round.Rank) / (round.PlayerCount - 1.0);
        }

        public PlayerRound[] Rounds => _rounds;
    }
}
