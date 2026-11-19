using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace IS_AdvokatskeKancelarije.Services
{
    public class OllamaSettings
    {
        public string BaseUrl { get; set; } = "http://localhost:11434";
        public string EmbeddingModel { get; set; } = "bge-m3";
        public string ChatModel { get; set; } = "gemma3:4b";

        // Veličina konteksta (u tokenima) koju model dobija; podrazumevana vrednost Ollame je premala za RAG
        public int ChatContextTokens { get; set; } = 6144;
        public double Temperature { get; set; } = 0.1;
    }

    public record ChatMessage(string Role, string Content);

    /// <summary>Tanak klijent za lokalni Ollama server (embeddings i chat).</summary>
    public class OllamaService
    {
        private readonly HttpClient _http;
        private readonly OllamaSettings _settings;

        public OllamaService(HttpClient http, IOptions<OllamaSettings> settings)
        {
            _http = http;
            _settings = settings.Value;
        }

        public async Task<List<float[]>> EmbedAsync(IReadOnlyList<string> inputs, CancellationToken ct = default)
        {
            var response = await _http.PostAsJsonAsync(
                "/api/embed",
                new { model = _settings.EmbeddingModel, input = inputs },
                ct);

            await EnsureSuccessAsync(response, "embedding", ct);

            var result = await response.Content.ReadFromJsonAsync<EmbedResponse>(cancellationToken: ct);
            if (result == null || result.Embeddings.Count != inputs.Count)
                throw new InvalidOperationException("Ollama returned an unexpected number of embeddings");

            return result.Embeddings;
        }

        /// <summary>Šalje razgovor jezičkom modelu i vraća kompletan odgovor (bez streaminga).</summary>
        public async Task<string> ChatAsync(IReadOnlyList<ChatMessage> messages, string? model = null, CancellationToken ct = default)
        {
            var payload = new
            {
                model = string.IsNullOrWhiteSpace(model) ? _settings.ChatModel : model,
                messages = messages.Select(m => new { role = m.Role, content = m.Content }),
                stream = false,
                options = new { temperature = _settings.Temperature, num_ctx = _settings.ChatContextTokens }
            };

            var response = await _http.PostAsJsonAsync("/api/chat", payload, ct);
            await EnsureSuccessAsync(response, "chat", ct);

            var result = await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken: ct);
            var content = result?.Message?.Content;
            if (string.IsNullOrWhiteSpace(content))
                throw new InvalidOperationException("Ollama returned an empty answer");

            // Neki modeli (npr. Qwen3) vraćaju i <think>...</think> blok sa "razmišljanjem"
            content = Regex.Replace(content, @"<think>.*?</think>", string.Empty, RegexOptions.Singleline).Trim();
            return content;
        }

        private static async Task EnsureSuccessAsync(HttpResponseMessage response, string what, CancellationToken ct)
        {
            if (response.IsSuccessStatusCode)
                return;

            var body = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException(
                $"Ollama {what} request failed ({(int)response.StatusCode}): {body}");
        }

        public class EmbedResponse
        {
            [JsonPropertyName("embeddings")]
            public List<float[]> Embeddings { get; set; } = new();
        }

        public class ChatResponse
        {
            [JsonPropertyName("message")]
            public ChatResponseMessage? Message { get; set; }
        }

        public class ChatResponseMessage
        {
            [JsonPropertyName("content")]
            public string? Content { get; set; }
        }
    }
}
