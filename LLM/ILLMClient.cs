namespace AutoGen.MultiAgent.Legal.Summarizer.LLM;

public interface ILLMClient
{
    Task<string> GetCompletionAsync(string prompt, int maxTokens = 400);
}

public static class LLMFactory
{
    public static ILLMClient Create()
    {
        // Priority: Azure OpenAI -> OpenAI -> fallback
        var azureEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        var azureKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY");
        var azureDeployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT");
        if (!string.IsNullOrWhiteSpace(azureEndpoint) && !string.IsNullOrWhiteSpace(azureKey) && !string.IsNullOrWhiteSpace(azureDeployment))
        {
            return new AzureOpenAILLMClient(azureEndpoint!, azureKey!, azureDeployment!);
        }

        var openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        var openAiModel = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4o-mini";
        if (!string.IsNullOrWhiteSpace(openAiKey))
        {
            return new OpenAILLMClient(openAiKey!, openAiModel);
        }

        return new FallbackLLMClient();
    }
}
