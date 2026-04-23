using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TurnierApp.Models;
using TurnierApp.Services;

namespace TurnierApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly TournamentSettings _settings = new TournamentSettings();
        private Tournament _tournament = new Tournament();

        private readonly DelegateCommand _createTournamentCommand;
        private readonly DelegateCommand _exportTournamentCommand;
        private readonly DelegateCommand _loadTournamentCommand;
        private readonly DelegateCommand _saveTournamentCommand;
        private readonly DelegateCommand _addGroupCommand;

        public MainViewModel()
        {
            _createTournamentCommand = new DelegateCommand(InitializeTournament);
            _exportTournamentCommand = new DelegateCommand(Export);

            _loadTournamentCommand = new DelegateCommand(LoadTournament);
            _saveTournamentCommand = new DelegateCommand(SaveTournament);

            _addGroupCommand = new DelegateCommand(AddGroup);
        }

        private void AddGroup()
        {
            Tournament.Groups.Add(new Group(Tournament) { Name = $"Gruppe {Tournament.Groups.Count + 1}" });
        }

        public Tournament Tournament
        {
            get => _tournament;
            set => Set(ref _tournament, value);
        }

        public void InitializeTournament()
        {
            Tournament.Players = _settings.PlayerNames;
            Tournament.StartTournament();
        }

        public TournamentSettings Settings => _settings;

        public ICommand CreateTournamentCommand => _createTournamentCommand;

        public ICommand ExportCommand => _exportTournamentCommand;

        public ICommand LoadCommand => _loadTournamentCommand;

        public ICommand SaveCommand => _saveTournamentCommand;

        public ICommand AddGroupCommand => _addGroupCommand;

        public void LoadTournament()
        {
            var ofd = new OpenFileDialog()
            {
                Title = "Von wo soll der Turnierplan geladen werden?",
                Filter = "Turnierdaten|*.td"
            };
            if (ofd.ShowDialog().GetValueOrDefault(false))
            {
                using (var fs = File.OpenRead(ofd.FileName))
                {
                    Tournament = TournamentSerializer.Deserialize(fs);
                }
            }
        }

        public void SaveTournament()
        {
            var sfd = new SaveFileDialog()
            {
                Title = "Wo soll der Turnierplan abgespeichert werden?",
                Filter = "Turnierdaten|*.td"
            };
            if (sfd.ShowDialog().GetValueOrDefault(false))
            {
                using (var fs = File.Create(sfd.FileName))
                {
                    TournamentSerializer.Serialize(Tournament, fs);
                }
            }
        }

        public void Export()
        {
            var sfd = new SaveFileDialog()
            {
                Title = "Wo sollen die Ergebnisse gespeichert werden?",
                Filter = "Markdown Dateien|*.md"
            };
            if (Tournament != null && sfd.ShowDialog().GetValueOrDefault())
            {
                using (var writer = new StreamWriter(sfd.FileName))
                {
                    writer.WriteLine("# Ergebnisse");
                    writer.WriteLine();
                    writer.WriteLine();
                    foreach (var group in Tournament.Groups)
                    {
                        if (group.Plan != null)
                        {
                            writer.WriteLine($"## {group.Name}");
                            writer.WriteLine();
                            WriteTournamentGroup(writer, group.Plan);
                            writer.WriteLine();
                            writer.WriteLine();
                        }
                    }
                }
            }
        }

        private void WriteTournamentGroup(StreamWriter writer, TournamentGroup tournament)
        {
            var sortedPlayers = tournament.Players.OrderByDescending(p => p.SumRank);
            if (tournament.MoreIsBetter)
            {
                sortedPlayers = sortedPlayers.ThenByDescending(p => p.SumScore);
            }
            else
            {
                sortedPlayers = sortedPlayers.ThenBy(p => p.SumScore);
            }

            var rank = 1;
            foreach (var player in sortedPlayers)
            {
                writer.WriteLine($"{rank}. {player.Name} mit {player.SumRank} Punkten ({player.SumScore} gesamt)");
            }

            writer.WriteLine();
            writer.WriteLine("### Ergebnisse im Detail");
            writer.WriteLine();
            foreach (var round in tournament.Rounds)
            {
                foreach (var table in round.Tables)
                {
                    writer.WriteLine($"* {round.Title} an {table.Title}: {string.Join(", ", table.Round.Players.OrderBy(p => p.Rank).Select(p => $"{p.Rank}. {p.Player.Name} mit {p.Score}"))}");
                }
            }
        }
    }
}
