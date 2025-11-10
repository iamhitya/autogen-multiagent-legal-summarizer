using AutoGen.MultiAgent.Legal.Summarizer.Chat;
using System.Text.RegularExpressions;

namespace AutoGen.MultiAgent.Legal.Summarizer.Agents;

public class VerifierAgent : AgentBase
{
    private const int MaxRounds = 3;
    private int _round;

    public VerifierAgent() : base("VerifierAgent") { }

    public override Task<IEnumerable<Message>> OnMessageAsync(IReadOnlyList<Message> history)
    {
        var summary = history.LastOrDefault(m => m.Type == MessageTypes.Summary);
        if (summary is null) return Task.FromResult(Enumerable.Empty<Message>());
        if (history.Any(m => m.Type == MessageTypes.FinalApproval)) return Task.FromResult(Enumerable.Empty<Message>());

        _round++;
        if (IsAcceptable(summary.Content) || _round >= MaxRounds)
        {
            var final = new Message(MessageTypes.FinalApproval, Name, "Summary approved.");
            Console.WriteLine(final);
            return Task.FromResult<IEnumerable<Message>>(new[] { final });
        }
        var feedback = BuildFeedback(summary.Content);
        var req = new Message(MessageTypes.RefinementRequest, Name, feedback, new Dictionary<string, string> { ["round"] = _round.ToString() });
        Console.WriteLine(req);
        return Task.FromResult<IEnumerable<Message>>(new[] { req });
    }

    private static bool IsAcceptable(string text)
    {
        var mustHave = new[] { "term", "terminate", "liab", "govern", "law" };
        var lower = text.ToLowerInvariant();
        return mustHave.Count(k => lower.Contains(k)) >= 3 && CountWords(lower) >= 80;
    }

    private static string BuildFeedback(string text)
    {
        var hints = new List<string>();
        if (!Regex.IsMatch(text, "term", RegexOptions.IgnoreCase)) hints.Add("Include the agreement term.");
        if (!Regex.IsMatch(text, "terminat", RegexOptions.IgnoreCase)) hints.Add("Clarify termination rights and notice periods.");
        if (!Regex.IsMatch(text, "liabil|cap", RegexOptions.IgnoreCase)) hints.Add("Mention liability exclusions and caps.");
        if (!Regex.IsMatch(text, "govern|law", RegexOptions.IgnoreCase)) hints.Add("State governing law.");
        if (CountWords(text) < 120) hints.Add("Expand slightly to 150-250 words.");
        if (hints.Count == 0) hints.Add("Improve clarity and ensure coverage of key clauses.");
        return string.Join(" ", hints);
    }

    private static int CountWords(string s)
    {
        // Count alphanumeric word tokens
        return Regex.Matches(s, "[A-Za-z0-9']+").Count;
    }
}
