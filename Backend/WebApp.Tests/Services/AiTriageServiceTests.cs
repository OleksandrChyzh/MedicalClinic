using System.Net;
using System.Text;
using System.Text.Json;
using BLL.Models.AiTriage;
using FluentAssertions;
using Moq;
using Moq.Protected;
using WebApp.Services;
using Xunit;

namespace WebApp.Tests.Services;

public class AiTriageServiceTests
{
    [Fact]
    public async Task AnalyzeAsync_WhenPythonReturnsSuccess_MapsToResultDto()
    {
        var pythonResponse = new
        {
            is_recognized      = true,
            predicted_specialty = "neurologist",
            predicted_urgency  = "medium",
            confidence         = 0.87f,
            human_message      = "Вам треба звернутися до Невролога. Критичність: Середня."
        };

        var jsonPayload = JsonSerializer.Serialize(pythonResponse);

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://127.0.0.1:8000")
        };

        var mockFactory = new Mock<IHttpClientFactory>();
        mockFactory
            .Setup(f => f.CreateClient("PythonAI"))
            .Returns(httpClient);

        var sut = new AiTriageService(mockFactory.Object);
        var request = new AiTriageRequestDto { SymptomText = "болить голова" };

        var result = await sut.AnalyzeAsync(request);

        result.Should().NotBeNull();
        result.IsRecognized.Should().BeTrue();
        result.PredictedSpecialty.Should().Be("neurologist");
        result.PredictedUrgency.Should().Be("medium");
        result.Confidence.Should().BeApproximately(0.87f, precision: 0.01f);
        result.HumanMessage.Should().Contain("Невролога");

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Post),
            ItExpr.IsAny<CancellationToken>());
    }
}
