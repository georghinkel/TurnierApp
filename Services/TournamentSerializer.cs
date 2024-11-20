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
                MoreIsBetter = tournament.MoreIsBetter,
                Rounds = tournament.Rounds.Length,
                Players = tournament.Players.Select(p => p.Name).ToArray(),
                Tables = tournament.Tables.Select(t => new SavedTable
                {
                    Rounds = t.Rounds.Select(r => new SavedRound
                    {
                        Players = r.Players.Select(p => p.Player.Name).ToArray(),
                    }).ToArray()
                }).ToArray()
            };
            var serializer = new XmlSerializer(typeof(SavedTournament));
            serializer.Serialize(target, saved);
        }

        public static Tournament Deserialize(Stream source)
        {
            var serializer = new XmlSerializer(typeof(SavedTournament));
            var saved = (SavedTournament)serializer.Deserialize(source);

            var tables = CreateTables(saved.Tables.Length, saved.Rounds);
            var players = CreatePlayers(saved.Players);


            List<Player>[,] playerAssignments = RestoreAssignments(saved, players);

            var roundsPerPlayer = players.ToDictionary(p => p, p => new List<PlayerRound>());

            PlayerRound AddPlayerRound(Player player)
            {
                var round = new PlayerRound(player);
                roundsPerPlayer[player].Add(round);
                return round;
            }

            for (int t = 0; t < saved.Tables.Length; t++)
            {   
                for (int r = 0; r < saved.Rounds; r++)
                {
                    tables[t].Rounds[r] = new Round(r, tables[t], playerAssignments[r, t].Select(AddPlayerRound).ToArray(), saved.MoreIsBetter);
                }
            }

            foreach (var item in roundsPerPlayer)
            {
                item.Key.Initialize(item.Value.ToArray());
            }

            return new Tournament(tables, players, saved.Rounds, saved.MoreIsBetter);
        }

        private static List<Player>[,] RestoreAssignments(SavedTournament saved, Player[] players)
        {
            var result = new List<Player>[saved.Rounds, saved.Tables.Length];
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

        private static Player[] CreatePlayers(string[] names)
        {
            var players = new Player[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                players[i] = new Player { Name = names[i] };
            }
            return players;
        }
    }
}
