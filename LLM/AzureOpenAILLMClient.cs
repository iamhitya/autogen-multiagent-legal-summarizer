using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AutoGen.MultiAgent.Legal.Summarizer.LLM;

public class AzureOpenAILLMClient : ILLMClient
{
    private readonly HttpClient _http = new();
    private readonly string _endpoint;
    private readonly string _key;
    private readonly string _deployment;

    public AzureOpenAILLMClient(string endpoint, string key, string deployment)
    {
        _endpoint = endpoint.TrimEnd('/');
        _key = key;
        _deployment = deployment;
    }

    public async Task<string> GetCompletionAsync(string prompt, int maxTokens = 400)
    {
        // Uses ChatCompletions API (2024-02-15-preview or similar)
        var url = $"{_endpoint}/openai/deployments/{_deployment}/chat/completions?api-version=2024-02-15-preview";
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Add("api-key", _key);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var body = new
        {
            messages = new[] { new { role = "user", content = prompt } },
            temperature = 0.2,
            max_tokens = maxTokens
        };
        req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        using var res = await _http.SendAsync(req);
        res.EnsureSuccessStatusCode();
        using var stream = await res.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();
        return content ?? string.Empty;
    }
}
