using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TurnierApp.Models;
using TurnierApp.Properties;

namespace TurnierApp.Services
{
    internal class TournamentSerializer
    {
        public static void Serialize(Tournament tournament, Stream target)
        {
            var saved = new SavedTournament
            {
                MoreIsBetter = tournament.Settings.MorePointsAreBetter,
                Rounds = tournament.Groups.Select(g =>
                {
                    return new SavedTournamentRound
                    {
                        Name = g.Name,
                        Rounds = g.Rounds,
                        Placements = g.Placements.Select(p => new SavedPlacement
                        {
                            Group = p.Group?.Name,
                            Rank = p.Rank,
                        }).ToArray(),
                        Players = g.Plan?.Players?.Select(p => p.Name).ToArray(),
                        Tables = g.Plan?.Tables.Select(t => new SavedTable
                        {
                            Rounds = t.Rounds.Select(r => new SavedRound
                            {
                                Players = r.Players.Select(p => p.Player.Name).ToArray(),
                            }).ToArray()
                        }).ToArray()
                    };
                }).ToArray(),
                Players = tournament.Players.Select(p => p.Name).ToArray(),
            };
            var serializer = new XmlSerializer(typeof(SavedTournament));
            serializer.Serialize(target, saved);
        }

        public static Tournament Deserialize(Stream source)
        {
            var serializer = new XmlSerializer(typeof(SavedTournament));
            var saved = (SavedTournament)serializer.Deserialize(source);

            var tournament = new Tournament();
            var players = new Dictionary<string, Player>();
            if (saved.Players != null)
            {
                foreach (var playerName in saved.Players)
                {
                    var player = new Player { Name = playerName };
                    players.Add(playerName, player);
                }
            }
            tournament.Players = players.Values.ToArray();
            if (saved.Rounds != null)
            {
                foreach (var round in saved.Rounds)
                {
                    tournament.Groups.Add(RestoreGroup(tournament, round, saved.MoreIsBetter, players));
                }
            }
            return tournament;
        }

        private static Group RestoreGroup(Tournament tournament, SavedTournamentRound saved, bool moreIsBetter, Dictionary<string, Player> players)
        {
            var group = new Group(tournament)
            {
                Name = saved.Name,
                Rounds = saved.Rounds,

            };
            if (saved.Players != null)
            {
                group.Plan = RestoreTournamentGroup(saved, moreIsBetter, players);
            }
            return group;
        }

        private static TournamentGroup RestoreTournamentGroup(SavedTournamentRound saved, bool moreIsBetter, Dictionary<string, Player> playersDict)
        {
            var tables = CreateTables(saved.Tables.Length, saved.Rounds);
            var players = CreatePlayers(saved.Players, playersDict);

            List<GroupPlayer>[,] playerAssignments = RestoreAssignments(saved, players);

            var roundsPerPlayer = players.ToDictionary(p => p, p => new List<PlayerRound>());

            PlayerRound AddPlayerRound(GroupPlayer player)
            {
                var round = new PlayerRound(player);
                roundsPerPlayer[player].Add(round);
                return round;
            }

            for (int t = 0; t < saved.Tables.Length; t++)
            {
                for (int r = 0; r < saved.Rounds; r++)
                {
                    tables[t].Rounds[r] = new Round(r, tables[t], playerAssignments[r, t].Select(AddPlayerRound).ToArray(), moreIsBetter);
                }
            }

            foreach (var item in roundsPerPlayer)
            {
                item.Key.Initialize(item.Value.ToArray());
            }

            return new TournamentGroup(saved.Name, tables, players, saved.Rounds, moreIsBetter);
        }

        private static List<GroupPlayer>[,] RestoreAssignments(SavedTournamentRound saved, GroupPlayer[] players)
        {
            var result = new List<GroupPlayer>[saved.Rounds, saved.Tables.Length];
            for (int t = 0; t < saved.Tables.Length; t++)
            {
                var table = saved.Tables[t];
                for (int r = 0; r < saved.Rounds; r++)
                {
                    var round = table.Rounds[r];
                    result[r, t] = round.Players.Select(pName => players.FirstOrDefault(p => string.Equals(p.Name, pName, StringComparison.OrdinalIgnoreCase))).ToList();
                }
            }
            return result;
        }

        private static Table[] CreateTables(int amount, int rounds)
        {
            var tables = new Table[amount];
            for (int i = 0; i < amount; i++)
            {
                tables[i] = new Table(i + 1, new Round[rounds]);
            }
            return tables;
        }

        private static GroupPlayer[] CreatePlayers(string[] names, Dictionary<string, Player> playersLookup)
        {
            var players = new GroupPlayer[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                players[i] = new GroupPlayer(playersLookup[names[i]]);
            }
            return players;
        }
    }
}
