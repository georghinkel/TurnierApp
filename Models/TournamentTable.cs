using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurnierApp.Models
{
    public class TournamentTable
    {
        public TournamentTable(Table table, Round round)
        {
            Table = table;
            Round = round;
        }

        public string Title => Table.Title;

        public Table Table { get; }

        public Round Round { get; }
    }
}
