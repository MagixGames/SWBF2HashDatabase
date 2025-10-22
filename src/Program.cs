using System.Text;

int Hash(string strToHash, bool lowercase = false)
{
    if (lowercase) strToHash = strToHash.ToLower();
    int hash = 5381;
    for (int i = 0; i < strToHash.Length; i++)
    {
        byte B = (byte)strToHash[i];
        hash = (hash * 33) ^ B;
    }
    return hash;
}
string ToString(int h) => $"0x{h:X8}";

HashSet<int> allHashes = new HashSet<int>();
HashSet<int> unhashed = new HashSet<int>();
HashSet<int> stillHashed = new HashSet<int>();
List<string> strings = new List<string>(); // making it a list so it can be sorted nicely later :D

// Read hashes txt
string text = File.ReadAllText("hashes.txt");
string[] lines = text.Split('\n');
foreach (string line in lines)
{
    string l = line.Trim();
    if (l == string.Empty || !l.StartsWith("0x")) continue; // line is empty or literally isnt a hash
    int hash = int.Parse(l.Substring(2), System.Globalization.NumberStyles.HexNumber);
    allHashes.Add(int.Parse(l.Substring(2), System.Globalization.NumberStyles.HexNumber));
    stillHashed.Add(int.Parse(l.Substring(2), System.Globalization.NumberStyles.HexNumber));
}


// Read strings txt
text = File.ReadAllText("strings.txt");
lines = text.Split('\n');
foreach (string line in lines)
{
    string l = line.Trim();
    int hash = Hash(l);
    if (l == string.Empty) continue; // empty
    if (!allHashes.Contains(hash)) continue; // doesnt equate to a hash
    if (strings.Contains(l)) continue; // already here
    stillHashed.Remove(hash);
    unhashed.Add(hash);
    strings.Add(l);
}
strings.Sort();

// Find collisions
Dictionary<int, List<string>> collisions = new Dictionary<int, List<string>>();
foreach (string s in strings)
{
    int hash = Hash(s);
    if (!collisions.ContainsKey(hash))
    {
        collisions[hash] = new List<string>();
    }
    collisions[hash].Add(s);
}

StringBuilder sbStrings = new StringBuilder();
StringBuilder sbUnhashed = new StringBuilder();
StringBuilder sbStillHashed = new StringBuilder();
StringBuilder sbCollisions = new StringBuilder();
StringBuilder sbCompletion = new StringBuilder();

foreach (var s in strings)
{
    sbStrings.AppendLine(s);
}
foreach (var h in unhashed)
{
    sbUnhashed.AppendLine(ToString(h));
}
foreach (var h in stillHashed)
{
    sbStillHashed.AppendLine(ToString(h));
}
foreach (var h in collisions)
{
    if (h.Value.Count <= 1)
    {
        continue;
    }

    sbCollisions.AppendLine($"Hash: {ToString(h.Key)} [ {string.Join(", ", h.Value.Select(s => $"\"{s}\""))} ]");
}
if (sbCollisions.Length == 0)
{
    sbCollisions.AppendLine("No collisions found :D");
}

sbCompletion.AppendLine($"Total hashes: {allHashes.Count}");
sbCompletion.AppendLine($"Successfully unhashed: {unhashed.Count} ({(((float)unhashed.Count / allHashes.Count) * 100):0.00}%)");
sbCompletion.AppendLine($"Unsolved: {allHashes.Count - unhashed.Count} ({((((float)allHashes.Count - unhashed.Count) / allHashes.Count) * 100):0.00}%)");

File.WriteAllText("strings.txt", sbStrings.ToString());
File.WriteAllText("unhashed.txt", sbUnhashed.ToString());
File.WriteAllText("unsolved.txt", sbStillHashed.ToString());
File.WriteAllText("collisions.txt", sbCollisions.ToString());
File.WriteAllText("result.txt", sbCompletion.ToString());
File.WriteAllText("README.md", sbCompletion.ToString()); // same as result but for github