namespace AutoGen.MultiAgent.Legal.Summarizer.Chat;

public static class MessageTypes
{
    public const string ExtractedSections = "ExtractedSections";
    public const string Summary = "Summary";
    public const string RefinementRequest = "RefinementRequest";
    public const string FinalApproval = "FinalApproval";
    public const string Info = "Info";
    public const string Error = "Error";
}

public record Message(
    string Type,
    string Sender,
    string Content,
    IReadOnlyDictionary<string, string>? Meta = null
)
{
    public override string ToString()
        => $"[{Type}] {Sender}: {Content}";
}
