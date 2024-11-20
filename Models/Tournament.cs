using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurnierApp.Models
{
    public class Tournament
    {
        private TournamentRound[] _rounds;

        public bool MoreIsBetter { get; }

        public Tournament(Table[] tables, Player[] players, int rounds, bool moreIsBetter)
        {
            Tables = tables;
            Players = players;
            MoreIsBetter = moreIsBetter;
            InitializeRounds(rounds);

            foreach (Player player in players)
            {
                player.ScoreChanged += OnRankingChanged;
            }
        }

        private void OnRankingChanged(object sender, EventArgs e)
        {
            RankingChanged?.Invoke(this, e);
        }

        public event EventHandler RankingChanged;

        public Table[] Tables { get; set; }

        public Player[] Players { get; set; }

        public TournamentRound[] Rounds { get => _rounds; }

        private void InitializeRounds(int roundCount)
        {
            _rounds = new TournamentRound[roundCount];
            for (int i = 0; i < roundCount; i++)
            {
                var tablesForRound = new TournamentTable[Tables.Length];
                for (int t = 0; t < Tables.Length; t++)
                {
                    tablesForRound[t] = new TournamentTable(Tables[t], Tables[t].Rounds[i]);
                }
                _rounds[i] = new TournamentRound(i, tablesForRound);
            }
        }
    }
}
