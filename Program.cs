using AutoGen.MultiAgent.Legal.Summarizer.Agents;
using AutoGen.MultiAgent.Legal.Summarizer.Chat;
using AutoGen.MultiAgent.Legal.Summarizer.LLM;

// Entry point
var filePath = args.FirstOrDefault();
string documentText;
if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
{
    documentText = await File.ReadAllTextAsync(filePath);
}
else
{
    documentText = SampleDocument.Text; // fallback demo
}

// Choose LLM implementation based on env vars
ILLMClient llmClient = LLMFactory.Create();

var extractor = new ExtractorAgent(documentText);
var summarizer = new SummarizerAgent(llmClient);
var verifier = new VerifierAgent();

var coordinator = new ChatCoordinator(new List<AgentBase> { extractor, summarizer, verifier });
var final = await coordinator.RunAsync();

Console.WriteLine("===== FINAL APPROVED SUMMARY =====");
Console.WriteLine(final);
Console.WriteLine("=================================");

internal static class SampleDocument
{
    public const string Text =
    @"
        MASTER SERVICES AGREEMENT
        DEFINITIONS
        ""Services"" means the professional services described in Statements of Work.
        ""Confidential Information"" means any proprietary or confidential information disclosed.

        TERM
        This Agreement commences on the Effective Date and continues for one (1) year unless terminated earlier.

        TERMINATION
        Either party may terminate this Agreement for material breach upon thirty (30) days written notice if such breach is not cured.

        LIABILITY
        In no event shall either party be liable for indirect, incidental, or consequential damages. The total aggregate liability shall not exceed the fees paid in the twelve (12) months preceding the claim.

        GOVERNING LAW
        This Agreement shall be governed by and construed in accordance with the laws of the State of New York without regard to conflict of law principles.

        MISCELLANEOUS
        No waiver of any provision shall be deemed a waiver of any other provision. If any provision is held unenforceable the remainder shall remain in full force.
    ";
}
