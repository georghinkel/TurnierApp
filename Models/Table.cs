using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurnierApp.Models
{
    public class Table : ViewModelBase
    {
        private string _title;

        public Table(int id, Round[] rounds)
        {
            Id = id;
            Rounds = rounds;

            Title = $"Tisch {Id}";
        }

        public int Id
        {
            get;
        }

        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        public Round[] Rounds 
        {
            get;
        }
    }
}
