using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurnierApp.Models;
using TurnierApp.Properties;

namespace TurnierApp.Services
{
    internal class TournamentPlanner
    {
        public Tournament Plan(TournamentSettings settings, Player[] lastPlayers)
        {
            var tables = CreateTables(settings.Tables, settings.Rounds);
            var players = CreatePlayers(settings.Players);

            if (lastPlayers != null)
            {
                TakeOverPlayerNames(settings, lastPlayers, players);
            }

            List<Player>[,] playerAssignments = FindBestAssignments(settings, players, settings.Attempts, settings.OrderMatters);

            var roundsPerPlayer = players.ToDictionary(p => p, p => new List<PlayerRound>());

            PlayerRound AddPlayerRound(Player player)
            {
                var round = new PlayerRound(player);
                roundsPerPlayer[player].Add(round);
                return round;
            }

            for (int t = 0; t < settings.Tables; t++)
            {
                for (int r = 0; r < settings.Rounds; r++)
                {
                    tables[t].Rounds[r] = new Round(r, tables[t], playerAssignments[r, t].Select(AddPlayerRound).ToArray(), settings.MorePointsAreBetter);
                }
            }

            foreach (var item in roundsPerPlayer)
            {
                item.Key.Initialize(item.Value.ToArray());
            }

            return new Tournament(tables, players, settings.Rounds, settings.MorePointsAreBetter);
        }

        private List<Player>[,] FindBestAssignments(TournamentSettings settings, Player[] players, int attempts, bool orderMatters)
        {
            var playerAssignments = CreateAssignments(players, settings);
            while (playerAssignments == null)
            {
                playerAssignments = CreateAssignments(players, settings);
            }
            var score = ScoreAssignment(playerAssignments, players, orderMatters);

            for (int i = 0; i < attempts; i++)
            {
                var alternative = CreateAssignments(players, settings);
                if (orderMatters)
                {
                    OptimizeOrder(alternative, players);
                }
                var alternativeScore = ScoreAssignment(alternative, players, orderMatters);
                if (alternativeScore < score)
                {
                    playerAssignments = alternative;
                    score = alternativeScore;
                }
            }

            if (orderMatters)
            {
                OptimizeOrder(playerAssignments, players);
            }

            return playerAssignments;
        }

        private void OptimizeOrder(List<Player>[,] assignments, Player[] players)
        {
            var indexSums = CalculateAssignmentIndexSums(assignments, players)
                .OrderBy(kv => kv.Value).ToList();
            var average = indexSums.Average(kv => kv.Value);

            while (true)
            {
                var playerWithHighestSum = indexSums[indexSums.Count - 1].Key;
                var playerWithLowestSum = indexSums[0].Key;

                var match = FindMatch(assignments, m =>
                {
                    var hIndex = m.IndexOf(playerWithHighestSum);
                    var lIndex = m.IndexOf(playerWithLowestSum);
                    return hIndex >= 0 && lIndex >= 0 && (lIndex - hIndex >= average - indexSums[0].Value);
                });

                if (match != null)
                {
                    var hIndex = match.IndexOf(playerWithHighestSum);
                    var lIndex = match.IndexOf(playerWithLowestSum);

                    match[lIndex] = playerWithHighestSum;
                    match[hIndex] = playerWithLowestSum;

                    var lSum = indexSums[0].Value + lIndex - hIndex;
                    PropagateLowValue(indexSums, playerWithLowestSum, lSum);
                    var hSum = indexSums[indexSums.Count - 1].Value + hIndex - lIndex;
                    PropagateHighValue(indexSums, playerWithHighestSum, hSum);
                }
                else
                {
                    break;
                }
            }
        }

        private static void PropagateLowValue(List<KeyValuePair<Player, int>> indexSums, Player playerWithLowestSum, int lSum)
        {
            var kv = new KeyValuePair<Player, int>(playerWithLowestSum, lSum);
            var index = 1;
            while (index < indexSums.Count && indexSums[index].Value < lSum)
            {
                indexSums[index - 1] = indexSums[index];
                indexSums[index] = kv;
                index++;
            }
        }

        private static void PropagateHighValue(List<KeyValuePair<Player, int>> indexSums, Player playerWithHighestSum, int hSum)
        {
            var kv = new KeyValuePair<Player, int>(playerWithHighestSum, hSum);
            var index = indexSums.Count - 2;
            while (index >= 0 && indexSums[index].Value > hSum)
            {
                indexSums[index + 1] = indexSums[index];
                indexSums[index] = kv;
                index--;
            }
        }

        private List<Player> FindMatch(List<Player>[,] assignments, Predicate<List<Player>> predicate)
        {
            for (int i = 0; i <= assignments.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= assignments.GetUpperBound(1); j++)
                {
                    if (predicate.Invoke(assignments[i,j]))
                    {
                        return assignments[i, j];
                    }
                }
            }
            return null;
        }

        private int ScoreAssignment(List<Player>[,] assignments, Player[] players, bool orderMatters)
        {
            var maxOppositions = 0;
            for (int p1 = 0; p1 < players.Length; p1++)
            {
                for (int p2 = p1 + 1; p2 < players.Length; p2++)
                {
                    var oppositions = assignments.Cast<List<Player>>().Count(round => round.Contains(players[p1]) && round.Contains(players[p2]));
                    maxOppositions = Math.Max(maxOppositions, oppositions);
                }
            }

            if (orderMatters)
            {
                var assignmentIndices = CalculateAssignmentIndexSums(assignments, players);
                var average = assignmentIndices.Values.Average();
                return maxOppositions + (int)(assignmentIndices.Values.Sum(i =>
                {
                    var diff = average - i;
                    return diff * diff;
                }));
            }

            return maxOppositions;
        }

        private static Dictionary<Player, int> CalculateAssignmentIndexSums(List<Player>[,] assignments, Player[] players)
        {
            var assignmentIndices = new Dictionary<Player, int>();
            for (int p1 = 0; p1 < players.Length; p1++)
            {
                var player = players[p1];
                var assignmentIndexSum = 0;
                foreach (var assignmentList in assignments)
                {
                    var index = assignmentList.IndexOf(player);
                    if (index != -1) assignmentIndexSum += index;
                }
                assignmentIndices.Add(player, assignmentIndexSum);
            }

            return assignmentIndices;
        }

        private static void TakeOverPlayerNames(TournamentSettings settings, Player[] lastPlayers, Player[] players)
        {
            for (int p = 0; p < settings.Players && p < lastPlayers.Length; p++)
            {
                players[p].Name = lastPlayers[p].Name;
            }
        }

        private List<Player>[,] CreateAssignments(Player[] players, TournamentSettings settings)
        {
            var maxPlayersPerRound = settings.Players;
            var overFill = 0;
            if (settings.MaxPlayersPerTable.HasValue)
            {
                maxPlayersPerRound = settings.Tables * settings.MaxPlayersPerTable.Value;
                overFill = maxPlayersPerRound * settings.Rounds % settings.Players;
                var skipRounds = overFill / maxPlayersPerRound;
                if (skipRounds > 0)
                {
                    settings.Rounds -= skipRounds;
                    overFill -= skipRounds * maxPlayersPerRound;
                }
            }
            var idealPlayersPerTable = maxPlayersPerRound / settings.Tables;
            var leftovers = maxPlayersPerRound % settings.Tables;

            var nonIdealPlaced = new HashSet<Player>();
            var canPlayedAgainst = players.ToDictionary(p => p, p => new List<Player>(players));
            foreach (var player in players)
            {
                canPlayedAgainst[player].Remove(player);
            }

            var random = new Random();
            var playersToAssign = new List<Player>(players.Length);

            Player PickRandomPlayer(IEnumerable<Player> playerBase)
            {
                if (!playerBase.Any()) return null;
                var index = random.Next(playerBase.Count());
                return playerBase.ElementAt(index);
            }

            var playerAssignments = new List<Player>[settings.Rounds, settings.Tables];

            void MarkPlayingAgainst(Player player1, Player player2)
            {
                var possibleOpponents = canPlayedAgainst[player1];
                possibleOpponents.Remove(player2);
                if (possibleOpponents.Count < idealPlayersPerTable)
                {
                    possibleOpponents.Clear();
                    possibleOpponents.AddRange(players);
                    possibleOpponents.Remove(player1);
                }
            }

            playersToAssign.AddRange(players);
            var playersInRound = new List<Player>();

            for (int r = 0; r < settings.Rounds; r++)
            {
                playersInRound.Clear();
                playersInRound.AddRange(players);
                for (int t = 0; t < settings.Tables; t++)
                {
                    IEnumerable<Player> playersForSeat = playersToAssign.Intersect(playersInRound);
                    var playersAtTable = new List<Player>();
                    for (int p = 0; p < idealPlayersPerTable; p++)
                    {
                        if (playersToAssign.Count <= p)
                        {
                            if (overFill > 0 && r == settings.Rounds - 1)
                            {
                                // no more player assignments
                                break;
                            }
                            // all players to assign have been used before
                            playersForSeat = playersForSeat.Except(playersToAssign.ToArray());
                            playersToAssign.AddRange(players);
                        }
                        var player = PickRandomPlayer(playersForSeat);
                        if (player == null) return null;
                        playersAtTable.Add(player);
                        playersForSeat = playersForSeat.Intersect(canPlayedAgainst[player]);
                        if (!playersForSeat.Any())
                        {
                            playersForSeat = playersToAssign.Intersect(playersInRound).Except(playersAtTable);
                        }
                        playersInRound.RemoveAll(playersAtTable.Contains);
                    }
                    if (t < leftovers)
                    {
                        var lastPlayer = PickRandomPlayer(playersForSeat);
                        if (lastPlayer == null) return null;
                        playersAtTable.Add(lastPlayer);
                    }
                    playerAssignments[r, t] = playersAtTable;
                    foreach (var playerAtTable in playersAtTable)
                    {
                        playersToAssign.Remove(playerAtTable);
                    }
                }

                // we have an assignment for all tables for this round
                for (int t = 0; t < settings.Tables; t++)
                {
                    for (int i = 0; i < playerAssignments[r, t].Count; i++)
                    {
                        for (int j = 0; j < i; j++)
                        {
                            MarkPlayingAgainst(playerAssignments[r, t][i], playerAssignments[r, t][j]);
                            MarkPlayingAgainst(playerAssignments[r, t][j], playerAssignments[r, t][i]);
                        }
                    }
                }
            }

            return playerAssignments;
        }

        private Table[] CreateTables(int amount, int rounds)
        {
            var tables = new Table[amount];
            for (int i = 0; i < amount; i++)
            {
                tables[i] = new Table(i + 1, new Round[rounds]);
            }
            return tables;
        }

        private Player[] CreatePlayers(int amount)
        {
            var players = new Player[amount];
            for (int i = 0; i < amount; i++)
            {
                players[i] = new Player { Name = "Spieler " + (i + 1) };
            }
            return players;
        }
    }
}
