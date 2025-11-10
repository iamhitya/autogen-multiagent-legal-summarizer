using System.Text.RegularExpressions;

namespace AutoGen.MultiAgent.Legal.Summarizer.LLM;

public class FallbackLLMClient : ILLMClient
{
    public Task<string> GetCompletionAsync(string prompt, int maxTokens = 400)
    {
        // Super simple heuristic: take lines under "Sections:" and summarize via sentence compression
        var sectionsText = ExtractAfter(prompt, "Sections:");
        var sentences = SplitSentences(sectionsText).Take(12).ToList();
        var keySentences = sentences.Where(s =>
            s.Contains("term", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("terminat", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("liabil", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("govern", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("law", StringComparison.OrdinalIgnoreCase)
        ).ToList();
        if (keySentences.Count < 5)
            keySentences = sentences.Take(6).ToList();

        string Compress(string s)
        {
            s = Regex.Replace(s, "\\(.*?\\)", "");
            s = Regex.Replace(s, "\\s+", " ").Trim();
            return s.TrimEnd('.', ';') + ".";
        }

        var summary = string.Join(" ", keySentences.Select(Compress));
        if (summary.Length > 1200) summary = summary[..1200];
        return Task.FromResult(summary);
    }

    private static string ExtractAfter(string text, string marker)
    {
        var idx = text.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        return idx >= 0 ? text[(idx + marker.Length)..] : text;
    }

    private static IEnumerable<string> SplitSentences(string text)
    {
        var parts = Regex.Split(text, "(?<=[.!?])\\s+");
        foreach (var p in parts)
        {
            var t = p.Trim();
            if (t.Length > 0) yield return t;
        }
    }
}
