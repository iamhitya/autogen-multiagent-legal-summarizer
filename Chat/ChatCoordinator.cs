using AutoGen.MultiAgent.Legal.Summarizer.Agents;

namespace AutoGen.MultiAgent.Legal.Summarizer.Chat;

public class ChatCoordinator
{
    private readonly List<AgentBase> _agents;
    private readonly List<Message> _history = new();

    public ChatCoordinator(IEnumerable<AgentBase> agents)
    {
        _agents = agents.ToList();
    }

    public async Task<string> RunAsync(int maxTurns = 12)
    {
        for (int turn = 0; turn < maxTurns; turn++)
        {
            foreach (var agent in _agents)
            {
                var outputs = await agent.OnMessageAsync(_history);
                foreach (var m in outputs)
                {
                    _history.Add(m);
                    Console.WriteLine(m.ToString());
                }

                if (_history.Any(h => h.Type == MessageTypes.FinalApproval))
                {
                    var best = _history.LastOrDefault(h => h.Type == MessageTypes.Summary)?.Content
                               ?? "No summary produced.";
                    return best;
                }
            }
        }
        return _history.LastOrDefault(h => h.Type == MessageTypes.Summary)?.Content
               ?? "No summary produced within turn limit.";
    }
}
