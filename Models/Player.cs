using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurnierApp.Models
{
    public class Player : ViewModelBase
    {
        private string _name;
        private Group _startGroup;

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        public Group StartGroup
        {
            get => _startGroup;
            set => Set(ref _startGroup, value);
        }
    }
}
