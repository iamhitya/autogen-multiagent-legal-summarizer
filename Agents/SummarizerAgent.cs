using AutoGen.MultiAgent.Legal.Summarizer.Chat;
using AutoGen.MultiAgent.Legal.Summarizer.LLM;
using System.Text;

namespace AutoGen.MultiAgent.Legal.Summarizer.Agents;

public class SummarizerAgent : AgentBase
{
    private readonly ILLMClient _llm;
    private int _attempt;

    public SummarizerAgent(ILLMClient llm) : base("SummarizerAgent") => _llm = llm;

    public override async Task<IEnumerable<Message>> OnMessageAsync(IReadOnlyList<Message> history)
    {
        // Look for extracted sections or refinement requests
        var extraction = history.LastOrDefault(m => m.Type == MessageTypes.ExtractedSections);
        var refinement = history.LastOrDefault(m => m.Type == MessageTypes.RefinementRequest && m.Sender == "VerifierAgent");

        if (extraction is null) return Enumerable.Empty<Message>();

        bool needNewSummary = history.Count(m => m.Type == MessageTypes.Summary && m.Sender == Name) <= _attempt;
        if (refinement != null) needNewSummary = true;
        if (!needNewSummary) return Enumerable.Empty<Message>();

        _attempt++;
        var prompt = BuildPrompt(extraction.Content, refinement?.Content);
        string summary = await _llm.GetCompletionAsync(prompt, 600);
        var msg = new Message(MessageTypes.Summary, Name, summary, new Dictionary<string, string> { ["attempt"] = _attempt.ToString() });
        Console.WriteLine(msg);
        return new[] { msg };
    }

    private static string BuildPrompt(string sections, string? refinement)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Summarize the following legal contract sections into a concise, neutral summary (150-250 words). Focus on: term length, termination rights, liability caps, governing law, and miscellaneous notes. Avoid adding information not present in the text.");
        if (!string.IsNullOrWhiteSpace(refinement))
        {
            sb.AppendLine("Verifier feedback to address:");
            sb.AppendLine(refinement);
        }
        sb.AppendLine("Sections:");
        sb.AppendLine(sections);
        sb.AppendLine("Summary:");
        return sb.ToString();
    }
}
