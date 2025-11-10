using AutoGen.MultiAgent.Legal.Summarizer.Chat;

namespace AutoGen.MultiAgent.Legal.Summarizer.Agents;

public abstract class AgentBase
{
    public string Name { get; }
    protected AgentBase(string name) => Name = name;

    public abstract Task<IEnumerable<Message>> OnMessageAsync(IReadOnlyList<Message> history);
}
