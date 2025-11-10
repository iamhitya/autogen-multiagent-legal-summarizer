# AutoGen.MultiAgent.Legal.Summarizer - Legal Document Summarization Agents

## Overview
A minimal .NET 9 console application that demonstrates a multi-agent workflow (inspired by the AutoGen framework) for summarizing lengthy legal documents.

Three specialized agents collaborate:
1. Extractor Agent – Reads a legal document and extracts key sections (e.g., clauses, definitions, termination, liabilities) using simple heuristics.
2. Summarizer Agent – Produces a concise, coherent summary of the extracted content using an LLM. Default backend is Azure OpenAI. If credentials are missing, it falls back to an internal heuristic summarizer.
3. Verifier Agent – Reviews the summary for coverage and clarity. Can request refinement loops until the summary is approved or a max iteration count is reached.

Agents communicate via an in-memory message bus, mimicking inter-agent chat coordination in AutoGen. Each agent posts messages; other agents react until a final approved summary is produced.

## AutoGen for .NET
- Installed package: `Microsoft.AutoGen.Core` (pre-release). The app keeps a lightweight orchestrator compatible with the AutoGen mental model.
- AutoGen Azure/OpenAI connector packages were not available on NuGet at setup time; the app currently uses a small internal Azure/OpenAI client. Swapping to official connectors will be straightforward once published or provided via a private feed.

## Requirements
- .NET 9 SDK
- (Optional) Azure OpenAI or OpenAI credentials

## Configuration
Default backend: Azure OpenAI (model: `gpt-4o-mini`). You can switch to OpenAI via environment variables. If neither is configured, a heuristic summarizer is used.

### Azure OpenAI (default)
Set the following environment variables:
- `AZURE_OPENAI_ENDPOINT` – e.g. `https://your-endpoint.openai.azure.com/`
- `AZURE_OPENAI_KEY` – your Azure OpenAI API key
- `AZURE_OPENAI_DEPLOYMENT` – your deployed model name (e.g. `gpt-4o-mini`)

PowerShell (Windows):
```
$env:AZURE_OPENAI_ENDPOINT="https://your-endpoint.openai.azure.com/"
$env:AZURE_OPENAI_KEY="your_key"
$env:AZURE_OPENAI_DEPLOYMENT="gpt-4o-mini"
```

Bash (Linux/macOS):
```
export AZURE_OPENAI_ENDPOINT="https://your-endpoint.openai.azure.com/"
export AZURE_OPENAI_KEY="your_key"
export AZURE_OPENAI_DEPLOYMENT="gpt-4o-mini"
```

### OpenAI (fallback)
Set the following environment variables:
- `OPENAI_API_KEY` – your OpenAI API key
- `OPENAI_MODEL` – model name (e.g. `gpt-4o-mini`)

PowerShell (Windows):
```
$env:OPENAI_API_KEY="your_key"
$env:OPENAI_MODEL="gpt-4o-mini"
```

Bash (Linux/macOS):
```
export OPENAI_API_KEY="your_key"
export OPENAI_MODEL="gpt-4o-mini"
```

## Running
```
# Build
dotnet build

# Run with a path to a legal document
dotnet run -- ./sample/legal_contract.txt

# Or run without args to use the embedded demo text
dotnet run --
```

## Output
Console output shows the iterative agent conversation and ends with an approved summary. The final approved summary is printed as:
```
===== FINAL APPROVED SUMMARY =====
<summary text>
=================================
```

## Project Structure
```
AutoGen.MultiAgent.Legal.Summarizer/
  Program.cs                Entry point (includes optional AutoGen diagnostics)
  Agents/
    AgentBase.cs            Common base class
    ExtractorAgent.cs       Extracts sections
    SummarizerAgent.cs      Summarizes extracted sections
    VerifierAgent.cs        Verifies and requests improvements
  Chat/
    Message.cs              Message record + types
    ChatCoordinator.cs      Orchestrates agent loop
  LLM/
    ILLMClient.cs           Abstraction for LLM calls
    AzureOpenAILLMClient.cs Azure OpenAI implementation
    OpenAILLMClient.cs      OpenAI implementation
    FallbackLLMClient.cs    Rule-based fallback
  README.md                 Project documentation
```

## Troubleshooting
- No summary or very short summary: ensure the correct environment variables are set for Azure OpenAI or OpenAI.
- AutoGen connectors not found: at the time of writing, Azure/OpenAI connector packages were not available on NuGet. Once available (or via a private feed), replace the in-house LLM clients with official AutoGen connectors and refactor the chat loop into GroupChat.

## Extensibility Ideas
- Replace heuristics with NLP chunking and entity extraction.
- Add vector search over sections and retrieval augmentation.
- Add a `RiskAssessmentAgent` for compliance scoring.
- Persist conversations and summaries.

## Disclaimer
This demo uses simplistic heuristics and is not production-grade legal analysis. Always consult a professional for actual legal review.