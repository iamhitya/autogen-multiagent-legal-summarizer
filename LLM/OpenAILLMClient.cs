using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AutoGen.MultiAgent.Legal.Summarizer.LLM;

public class OpenAILLMClient : ILLMClient
{
    private readonly HttpClient _http = new();
    private readonly string _apiKey;
    private readonly string _model;

    public OpenAILLMClient(string apiKey, string model)
    {
        _apiKey = apiKey;
        _model = model;
    }

    public async Task<string> GetCompletionAsync(string prompt, int maxTokens = 400)
    {
        var url = "https://api.openai.com/v1/chat/completions";
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var body = new
        {
            model = _model,
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
