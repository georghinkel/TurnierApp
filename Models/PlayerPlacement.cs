using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TurnierApp.ViewModels;

namespace TurnierApp.Models
{
    public class PlayerPlacement : ViewModelBase
    {
        private Group _group;
        private int _rank;

        public PlayerPlacement(Group parent)
        {
            Parent = parent;

            RemoveCommand = new DelegateCommand(Delete);
        }

        public ICommand RemoveCommand { get; }

        private void Delete()
        {
            Parent.Placements.Remove(this);
        }

        public Group Parent { get; }

        public ICollection<Group> AllGroups => Parent.Tournament.Groups;

        public Group Group
        {
            get => _group;
            set => Set(ref _group, value);
        }

        public int Rank
        {
            get => _rank;
            set => Set(ref _rank, value);
        }
    }
}
