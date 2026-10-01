using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;

namespace InventorySort
{
    /// <summary>
    /// Reads QuickStackDeposit's slot locks from its locks file (next to the .cfg files).
    /// Format, one line per character: <c>playerId|x,y;x,y;...</c> (playerId = Player.GetPlayerID()).
    /// QuickStackDeposit rewrites the file on every lock toggle, so reading it per sort is always current.
    /// Soft dependency: if the file doesn't exist, no slots are locked.
    /// </summary>
    internal static class QuickStackLocks
    {
        private const string FileName = "dreamscapist.valheim.quickstackdeposit.locks.txt";

        public static HashSet<(int, int)> LoadFor(Player player)
        {
            var locked = new HashSet<(int, int)>();
            string path = Path.Combine(Paths.ConfigPath, FileName);
            if (!File.Exists(path)) return locked;

            // Same key QuickStackDeposit uses: Player.GetPlayerID(), unique per character.
            string id = player.GetPlayerID().ToString();
            try
            {
                foreach (string rawLine in File.ReadAllLines(path))
                {
                    string line = rawLine.Trim();
                    int bar = line.IndexOf('|');
                    if (bar <= 0) continue;
                    if (line.Substring(0, bar).Trim() != id) continue;

                    foreach (string pair in line.Substring(bar + 1).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        string[] xy = pair.Split(',');
                        if (xy.Length == 2 && int.TryParse(xy[0].Trim(), out int x) && int.TryParse(xy[1].Trim(), out int y))
                            locked.Add((x, y));
                    }
                }
            }
            catch (Exception e)
            {
                InventorySortPlugin.Log.LogWarning($"Couldn't read QuickStackDeposit locks: {e.Message}");
                return locked;
            }

            return locked;
        }
    }
}
