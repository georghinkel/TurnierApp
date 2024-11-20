using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurnierApp.Models
{
    public class TournamentSettings : ViewModelBase
    {
        private int _tables = 2;
        private int _rounds = 30;
        private int _players;
        private int? _maxPlayersPerTable = 2;
        private bool _morePointsAreBetter = true;
        private Player[] _playerNames;
        private bool _orderMatters = true;
        private int _attempts = 1000;

        public TournamentSettings()
        {
            Players = 12;
        }

        public bool OrderMatters
        {
            get => _orderMatters;
            set => Set(ref _orderMatters, value);
        }

        public int Attempts
        {
            get => _attempts;
            set => Set(ref _attempts, value);
        }

        public int Tables
        {
            get => _tables;
            set => Set(ref _tables, value);
        }

        public int? MaxPlayersPerTable
        {
            get => _maxPlayersPerTable;
            set => Set(ref _maxPlayersPerTable, value);
        }

        public int Players
        {
            get => _players;
            set
            {
                if (Set(ref _players, value))
                {
                    var newPlayers = new Player[value];
                    var copiedPlayers = 0;
                    if (_playerNames != null)
                    {
                        for (int playerIndex = 0; playerIndex < value && playerIndex < _playerNames.Length; playerIndex++)
                        {
                            newPlayers[playerIndex] = _playerNames[playerIndex];
                        }
                        copiedPlayers = _playerNames.Length;
                    }
                    for (int i = copiedPlayers; i < newPlayers.Length; i++)
                    {
                        newPlayers[i] = new Player { Name = $"Spieler {i + 1}" };
                    }
                    PlayerNames = newPlayers;
                }
            }
        }

        public Player[] PlayerNames
        {
            get => _playerNames;
            private set => Set(ref _playerNames, value);
        }

        public int Rounds
        {
            get => _rounds;
            set => Set(ref _rounds, value);
        }

        public bool MorePointsAreBetter
        {
            get => _morePointsAreBetter;
            set => Set(ref _morePointsAreBetter, value);
        }
    }
}
