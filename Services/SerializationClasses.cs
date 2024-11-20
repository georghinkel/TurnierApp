using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace TurnierApp.Services
{
    public class SavedTournament
    {
        public bool MoreIsBetter { get; set; }

        public string[] Players { get; set; }

        public int Rounds { get; set; }

        public SavedTable[] Tables { get; set; }
    }

    public class SavedRound
    {
        public string[] Players { get; set; }
    }

    public class SavedTable
    {
        public SavedRound[] Rounds { get; set; }
    }
}
