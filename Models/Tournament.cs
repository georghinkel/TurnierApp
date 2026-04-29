using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurnierApp.Services;
using TurnierApp.ViewModels;

namespace TurnierApp.Models
{
    public class Tournament : ViewModelBase
    {
        private readonly ObservableCollection<Group> _groups = new ObservableCollection<Group>();
        private readonly ObservableCollection<GroupViewModel> _activeGroups = new ObservableCollection<GroupViewModel>();
        private readonly ObservableCollection<TournamentGroup> _completedTournaments = new ObservableCollection<TournamentGroup>();
        private Player[] _players;
        private readonly TournamentSettings _settings = new TournamentSettings();
        private readonly TournamentPlanner _planner = new TournamentPlanner();

        public ICollection<Group> Groups => _groups;

        public ICollection<GroupViewModel> ActiveGroups => _activeGroups;

        public ICollection<TournamentGroup> CompletedGroups => _completedTournaments;

        public TournamentSettings Settings => _settings;

        public Player[] Players
        {
            get => _players;
            set => Set(ref _players, value);
        }

        public void AssignStartGroups()
        {
            var startGroups = Groups.Where(g => !g.HasPlacements).ToList();
            int[] counts = InitializeCounts(startGroups);

            var players = Players.Where(p => p.StartGroup == null).ToList();
            var random = new Random();
            foreach (var player in players)
            {
                GetMinCount(counts, out var min, out var minCount);
                var index = GetIndexOf(counts, min, random.Next(minCount));
                if (index >= 0)
                {
                    counts[index]++;
                    player.StartGroup = startGroups[index];
                }
            }
        }

        public void StartTournament()
        {
            _activeGroups.Clear();
            AssignStartGroups();

            foreach (var group in Groups)
            {
                if (group.HasPlacements)
                {
                    group.Plan = null;
                }
                else 
                {
                    var players = Players.Where(p => p.StartGroup == group)
                        .Select(p => new GroupPlayer(p))
                        .ToArray();

                    group.Plan = _planner.Plan(group, Settings, players);
                    _activeGroups.Add(new GroupViewModel(group, group.Plan));
                }
            }
        }

        public void PlanGroup(Group group)
        {
            if (group.HasPlacements)
            {
                var players = new List<GroupPlayer>();
                foreach (var placement in group.Placements)
                {
                    var playersInOrder = placement.Group.Plan.Players.OrderByDescending(p => p.SumRank);
                    if (_settings.MorePointsAreBetter)
                    {
                        playersInOrder = playersInOrder.ThenByDescending(p => p.SumScore);
                    }else
                    {
                        playersInOrder = playersInOrder.ThenBy(p => p.SumScore);
                    }
                    var player = playersInOrder.ElementAtOrDefault(placement.Rank - 1);
                    if (player != null)
                    {
                        players.Add(new GroupPlayer(player.Player));
                    }
                }
                group.Plan = _planner.Plan(group, Settings, players.ToArray());
            }
        }

        public void StartGroup(Group group)
        {
            if (group.Plan != null)
            {
                _activeGroups.Add(new GroupViewModel(group, group.Plan));
            }
        }

        private int[] InitializeCounts(List<Group> startGroups)
        {
            var counts = new int[startGroups.Count];
            foreach (var player in Players)
            {
                if (player.StartGroup != null)
                {
                    var index = startGroups.IndexOf(player.StartGroup);
                    if (index >= 0)
                    {
                        counts[index]++;
                    }
                }
            }

            return counts;
        }

        private int GetIndexOf(int[] counts, int data, int index)
        {
            var dataIndex = index + 1;
            for (int i = 0; i < counts.Length; i++)
            {
                if (counts[i] == data)
                {
                    dataIndex--;
                    if (dataIndex == 0)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private void GetMinCount(int[] counts, out int min, out int minCount)
        {
            min = int.MaxValue;
            minCount = 0;
            for (int i = 0; i < counts.Length; i++)
            {
                if (counts[i] < min)
                {
                    min = counts[i];
                    minCount = 1;
                }
                else if (counts[i] == min)
                {
                    minCount++;
                }
            }
        }

        public void Complete(Group group)
        {
            if (group?.Plan == null)
            {
                throw new InvalidOperationException("Gruppe muss erst gestartet sein");
            }

            group.IsCompleted = true;
            var viewModel = ActiveGroups.FirstOrDefault(gv => gv.Tournament == group.Plan);
            if (viewModel != null)
            {
                ActiveGroups.Remove(viewModel);
            }

            foreach (var enabledGroup in Groups.Where(g => g.HasPlacements))
            {
                if (enabledGroup.Plan == null && enabledGroup.Placements.All(pl => pl.Group.IsCompleted))
                {
                    PlanGroup(enabledGroup);
                    StartGroup(enabledGroup);
                }
            }

            if (!_completedTournaments.Contains(group.Plan))
            {
                _completedTournaments.Add(group.Plan);
            }
        }
    }
}
