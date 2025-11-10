using AutoGen.MultiAgent.Legal.Summarizer.Chat;
using System.Text;

namespace AutoGen.MultiAgent.Legal.Summarizer.Agents;

public class ExtractorAgent : AgentBase
{
    private readonly string _document;
    private bool _hasEmitted;

    public ExtractorAgent(string document) : base("ExtractorAgent")
    {
        _document = document ?? string.Empty;
    }

    public override Task<IEnumerable<Message>> OnMessageAsync(IReadOnlyList<Message> history)
    {
        // Run once at start
        if (_hasEmitted) return Task.FromResult(Enumerable.Empty<Message>());
        _hasEmitted = true;

        var sections = ExtractSections(_document);
        var content = string.Join("\n\n", sections.Select(s => $"# {s.Key}\n{s.Value}"));
        var msg = new Message(MessageTypes.ExtractedSections, Name, content);
        Console.WriteLine(msg);
        return Task.FromResult<IEnumerable<Message>>(new[] { msg });
    }

    private static Dictionary<string, string> ExtractSections(string text)
    {
        // Simple heuristic: split by lines, detect uppercase headings and accumulate until next heading
        var lines = text.Replace("\r", string.Empty).Split('\n');
        var sections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string current = "Preamble";
        var sb = new StringBuilder();

        bool IsHeading(string l)
        {
            var t = l.Trim();
            if (t.Length is < 3 or > 80) return false;
            if (!t.All(c => char.IsLetter(c) || char.IsWhiteSpace(c) || c == '-' || c == '&')) return false;
            return t == t.ToUpperInvariant();
        }

        void Commit()
        {
            var val = sb.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(val))
                sections[current] = val;
            sb.Clear();
        }

        foreach (var line in lines)
        {
            if (IsHeading(line))
            {
                Commit();
                current = line.Trim();
            }
            else
            {
                sb.AppendLine(line);
            }
        }
        Commit();

        // Keep only relevant legal-like sections if present
        var preferred = new[] { "DEFINITIONS", "TERM", "TERMINATION", "LIABILITY", "GOVERNING LAW", "MISCELLANEOUS" };
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in preferred)
        {
            if (sections.TryGetValue(p, out var v)) result[p] = v.Trim();
        }
        if (result.Count == 0) return sections; // fallback to all
        return result;
    }
}
