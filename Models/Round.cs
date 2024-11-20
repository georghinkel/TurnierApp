using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurnierApp.Models
{
    public class Round
    {
        public bool MoreIsBetter
        {
            get;
        }

        public Table Table { get; }

        public Round(int id, Table table, PlayerRound[] players, bool moreIsBetter)
        {
            Id = id;
            Table = table;
            Players = players;

            for (int p = 0; p < players.Length; p++)
            {
                players[p].ScoreChanged += ScoreChanged;
                players[p].Round = this;
            }
            MoreIsBetter = moreIsBetter;
        }

        private void ScoreChanged(object sender, EventArgs e)
        {
            var playersByScore = (MoreIsBetter ? Players.OrderByDescending(p => p.Score) : Players.OrderBy(p => p.Score)).ToList();
            var lastScore = int.MaxValue;
            var lastRank = 0;
            var noScore = MoreIsBetter ? int.MinValue : int.MaxValue;
            for (int i = 0; i < playersByScore.Count; i++)
            {
                if (playersByScore[i].Score != lastScore)
                {
                    lastRank = i + 1;
                    lastScore = playersByScore[i].Score.GetValueOrDefault(noScore);
                }
                playersByScore[i].PlayerCount = Players.Length;
                playersByScore[i].Rank = lastRank;
            }
        }

        public int Id
        {
            get;
        }

        public string Title => $"Runde {Id + 1}";

        public PlayerRound[] Players
        {
            get;
        }
    }
}
