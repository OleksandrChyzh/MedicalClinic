using System.Net.Http.Json;
using System.Text.Json.Serialization;
using BLL.Interfaces;
using BLL.Models.AiTriage;

namespace WebApp.Services;

public class AiTriageService(IHttpClientFactory httpClientFactory) : IAiTriageService
{
    private const string PythonApiUrl = "http://127.0.0.1:8000/api/ai/analyze";

    public async Task<AiTriageResultDto> AnalyzeAsync(AiTriageRequestDto request)
    {
        var client = httpClientFactory.CreateClient("PythonAI");

        var payload = new { symptom_text = request.SymptomText };

        using var response = await client.PostAsJsonAsync(PythonApiUrl, payload);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PythonAiResponse>()
            ?? throw new InvalidOperationException("Порожня відповідь від AI-сервісу.");

        return new AiTriageResultDto
        {
            IsRecognized = result.IsRecognized,
            PredictedSpecialty = result.PredictedSpecialty,
            PredictedUrgency = result.PredictedUrgency,
            Confidence = result.Confidence,
            HumanMessage = result.HumanMessage
        };
    }

    private sealed class PythonAiResponse
    {
        [JsonPropertyName("is_recognized")]
        public bool IsRecognized { get; set; }

        [JsonPropertyName("predicted_specialty")]
        public string? PredictedSpecialty { get; set; }

        [JsonPropertyName("predicted_urgency")]
        public string? PredictedUrgency { get; set; }

        [JsonPropertyName("confidence")]
        public float Confidence { get; set; }

        [JsonPropertyName("human_message")]
        public string HumanMessage { get; set; } = string.Empty;
    }
}
