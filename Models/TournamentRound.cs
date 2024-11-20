using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurnierApp.Models
{
    public class TournamentRound
    {
        public TournamentRound(int id, TournamentTable[] tables)
        {
            Id = id;
            Tables = tables;
        }

        public int Id { get; }

        public TournamentTable[] Tables { get; }

        public string Title => $"Runde {Id + 1}";
    }
}
