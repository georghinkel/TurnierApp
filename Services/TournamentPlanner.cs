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
        public TournamentGroup Plan(Group group, TournamentSettings settings, GroupPlayer[] players)
        {
            var tables = CreateTables(group.Tables, group.Rounds);

            List<GroupPlayer>[,] playerAssignments = FindBestAssignments(group, players, settings.Attempts, settings.OrderMatters);

            var roundsPerPlayer = players.ToDictionary(p => p, p => new List<PlayerRound>());

            PlayerRound AddPlayerRound(GroupPlayer player)
            {
                var round = new PlayerRound(player);
                roundsPerPlayer[player].Add(round);
                return round;
            }

            for (int t = 0; t < group.Tables; t++)
            {
                for (int r = 0; r < group.Rounds; r++)
                {
                    tables[t].Rounds[r] = new Round(r, tables[t], playerAssignments[r, t].Select(AddPlayerRound).ToArray(), settings.MorePointsAreBetter);
                }
            }

            foreach (var item in roundsPerPlayer)
            {
                item.Key.Initialize(item.Value.ToArray());
            }

            return new TournamentGroup(group.Name, tables, players, group.Rounds, settings.MorePointsAreBetter);
        }

        private List<GroupPlayer>[,] FindBestAssignments(Group settings, GroupPlayer[] players, int attempts, bool orderMatters)
        {
            var playerAssignments = CreateAssignments(players, settings);
            while (playerAssignments == null)
            {
                playerAssignments = CreateAssignments(players, settings);
            }
            var score = ScoreAssignment(playerAssignments, players);

            for (int i = 0; i < attempts; i++)
            {
                var alternative = CreateAssignments(players, settings);
                if (orderMatters)
                {
                    OptimizeOrder(alternative, players);
                }
                var alternativeScore = ScoreAssignment(alternative, players);
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

        private void OptimizeOrder(List<GroupPlayer>[,] assignments, GroupPlayer[] players)
        {
            var indexSums = CalculateAssignmentIndexSums(assignments, players)
                .OrderBy(kv => kv.Value).ToList();
            var average = indexSums.Average(kv => kv.Value);

            var attempt = 1;

            while (attempt < 100)
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
                attempt++;
            }
        }

        private static void PropagateLowValue(List<KeyValuePair<GroupPlayer, int>> indexSums, GroupPlayer playerWithLowestSum, int lSum)
        {
            var kv = new KeyValuePair<GroupPlayer, int>(playerWithLowestSum, lSum);
            var index = 1;
            while (index < indexSums.Count && indexSums[index].Value < lSum)
            {
                indexSums[index - 1] = indexSums[index];
                indexSums[index] = kv;
                index++;
            }
        }

        private static void PropagateHighValue(List<KeyValuePair<GroupPlayer, int>> indexSums, GroupPlayer playerWithHighestSum, int hSum)
        {
            var kv = new KeyValuePair<GroupPlayer, int>(playerWithHighestSum, hSum);
            var index = indexSums.Count - 2;
            while (index >= 0 && indexSums[index].Value > hSum)
            {
                indexSums[index + 1] = indexSums[index];
                indexSums[index] = kv;
                index--;
            }
        }

        private List<GroupPlayer> FindMatch(List<GroupPlayer>[,] assignments, Predicate<List<GroupPlayer>> predicate)
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

        private int ScoreAssignment(List<GroupPlayer>[,] assignments, GroupPlayer[] players)
        {
            var maxOppositions = 0;
            for (int p1 = 0; p1 < players.Length; p1++)
            {
                for (int p2 = p1 + 1; p2 < players.Length; p2++)
                {
                    var oppositions = assignments.Cast<List<GroupPlayer>>().Count(round => round.Contains(players[p1]) && round.Contains(players[p2]));
                    maxOppositions = Math.Max(maxOppositions, oppositions);
                }
            }

            return maxOppositions;
        }

        private static Dictionary<GroupPlayer, int> CalculateAssignmentIndexSums(List<GroupPlayer>[,] assignments, GroupPlayer[] players)
        {
            var assignmentIndices = new Dictionary<GroupPlayer, int>();
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

        private List<GroupPlayer>[,] CreateAssignments(GroupPlayer[] players, Group settings)
        {
            var maxPlayersPerRound = players.Length;
            var overFill = 0;
            if (settings.MaxPlayersPerTable.HasValue)
            {
                maxPlayersPerRound = settings.Tables * settings.MaxPlayersPerTable.Value;
                overFill = maxPlayersPerRound * settings.Rounds % players.Length;
                var skipRounds = overFill / maxPlayersPerRound;
                if (skipRounds > 0)
                {
                    settings.Rounds -= skipRounds;
                    overFill -= skipRounds * maxPlayersPerRound;
                }
            }
            var idealPlayersPerTable = maxPlayersPerRound / settings.Tables;
            var leftovers = maxPlayersPerRound % settings.Tables;

            var nonIdealPlaced = new HashSet<GroupPlayer>();
            var canPlayedAgainst = players.ToDictionary(p => p, p => new List<GroupPlayer>(players));
            foreach (var player in players)
            {
                canPlayedAgainst[player].Remove(player);
            }

            var random = new Random();
            var playersToAssign = new List<GroupPlayer>(players.Length);

            GroupPlayer PickRandomPlayer(IEnumerable<GroupPlayer> playerBase)
            {
                if (!playerBase.Any()) return null;
                var index = random.Next(playerBase.Count());
                return playerBase.ElementAt(index);
            }

            var playerAssignments = new List<GroupPlayer>[settings.Rounds, settings.Tables];

            void MarkPlayingAgainst(GroupPlayer player1, GroupPlayer player2)
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
            var playersInRound = new List<GroupPlayer>();

            for (int r = 0; r < settings.Rounds; r++)
            {
                playersInRound.Clear();
                playersInRound.AddRange(players);
                for (int t = 0; t < settings.Tables; t++)
                {
                    IEnumerable<GroupPlayer> playersForSeat = playersToAssign.Intersect(playersInRound);
                    var playersAtTable = new List<GroupPlayer>();
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
    }
}
