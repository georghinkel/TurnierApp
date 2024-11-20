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
        private TournamentSettings _settings = new TournamentSettings();
        private TournamentPlanner _planner = new TournamentPlanner();
        private Tournament _tournament;
        private readonly ObservableCollection<TableViewModel> _tables = new ObservableCollection<TableViewModel>();

        private DelegateCommand _createTournamentCommand;
        private DelegateCommand _exportTournamentCommand;
        private DelegateCommand _loadTournamentCommand;
        private DelegateCommand _saveTournamentCommand;

        public MainViewModel()
        {
            _createTournamentCommand = new DelegateCommand(InitializeTournament);
            _exportTournamentCommand = new DelegateCommand(Export);

            _loadTournamentCommand = new DelegateCommand(LoadTournament);
            _saveTournamentCommand = new DelegateCommand(SaveTournament);
        }

        public TournamentSettings Settings => _settings;

        public ICommand CreateTournamentCommand => _createTournamentCommand;

        public ICommand ExportCommand => _exportTournamentCommand;

        public ICommand LoadCommand => _loadTournamentCommand;

        public ICommand SaveCommand => _saveTournamentCommand;

        public Tournament Tournament
        {
            get => _tournament;
            private set => Set(ref _tournament, value);
        }

        public ObservableCollection<TableViewModel> Tables => _tables;

        public void InitializeTournament()
        {
            Tournament = _planner.Plan(Settings, Tournament?.Players ?? Settings.PlayerNames);
            InitTournamentCore();
        }

        private void InitTournamentCore()
        {
            Tournament.RankingChanged += Tournament_RankingChanged;
            Tables.Clear();
            foreach (var table in Tournament.Tables)
            {
                Tables.Add(new TableViewModel(table));
            }
        }

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
                    InitTournamentCore();
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
                    writer.WriteLine("Ergebnisse");
                    writer.WriteLine("==========");
                    writer.WriteLine();
                    var sortedPlayers = Tournament.Players.OrderByDescending(p => p.SumRank);
                    if (Tournament.MoreIsBetter)
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
                    writer.WriteLine("Ergebnisse im Detail");
                    writer.WriteLine("--------------------");
                    writer.WriteLine();
                    foreach (var round in Tournament.Rounds)
                    {
                        foreach (var table in round.Tables)
                        {
                            writer.WriteLine($"* {round.Title} an {table.Title}: {string.Join(", ", table.Round.Players.OrderBy(p => p.Rank).Select(p => $"{p.Rank}. {p.Player.Name} mit {p.Score}"))}");
                        }
                    }
                }
            }
        }

        private void Tournament_RankingChanged(object sender, EventArgs e)
        {
            RankingChanged?.Invoke(this, e);
        }

        public event EventHandler RankingChanged;
    }
}
