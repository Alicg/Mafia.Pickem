using FluentAssertions;
using MafiaPickem.Api.Bot;
using MafiaPickem.Api.Functions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Text;

namespace MafiaPickem.Api.Tests.Functions;

public class BotWebhookFunctionTests
{
    private readonly Mock<ITelegramWebhookValidator> _validatorMock;
    private readonly Mock<ITelegramBotClient> _telegramBotClientMock;
    private readonly BotWebhookFunction _function;

    public BotWebhookFunctionTests()
    {
        _validatorMock = new Mock<ITelegramWebhookValidator>();
        _telegramBotClientMock = new Mock<ITelegramBotClient>();
        _function = new BotWebhookFunction(_validatorMock.Object, _telegramBotClientMock.Object);
    }

    [Fact]
    public void BotWebhookFunction_ShouldRequireValidator()
    {
        // Arrange & Act
        var function = new BotWebhookFunction(_validatorMock.Object, _telegramBotClientMock.Object);

        // Assert
        function.Should().NotBeNull();
    }

    [Fact]
    public async Task HandleWebhook_WithPrivateMessage_ShouldSendMiniAppPrompt()
    {
        var secretToken = "test-secret";
        const long expectedChatId = 123456789;
        const string body = """
            {
              "update_id": 1,
              "message": {
                "message_id": 10,
                "chat": {
                  "id": 123456789,
                  "type": "private"
                },
                "text": "/start"
              }
            }
            """;

        _validatorMock.Setup(v => v.ValidateSecretToken(secretToken)).Returns(true);
        _telegramBotClientMock
            .Setup(x => x.SendMiniAppPromptAsync(expectedChatId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = CreateMockHttpRequest(body, secretToken);

        // Act
        var response = await _function.HandleWebhook(request, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _validatorMock.Verify(v => v.ValidateSecretToken(secretToken), Times.Once);
        _telegramBotClientMock.Verify(
            x => x.SendMiniAppPromptAsync(expectedChatId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleWebhook_WithNonPrivateMessage_ShouldNotSendMiniAppPrompt()
    {
        var secretToken = "test-secret";
        const string body = """
            {
              "update_id": 1,
              "message": {
                "message_id": 10,
                "chat": {
                  "id": -100123456,
                  "type": "group"
                },
                "text": "/start"
              }
            }
            """;

        _validatorMock.Setup(v => v.ValidateSecretToken(secretToken)).Returns(true);

        var request = CreateMockHttpRequest(body, secretToken);

        // Act
        var response = await _function.HandleWebhook(request, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _telegramBotClientMock.Verify(
            x => x.SendMiniAppPromptAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static HttpRequestData CreateMockHttpRequest(string body, string? secretToken)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddOptions();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var context = new Mock<FunctionContext>();
        context.Setup(c => c.InstanceServices).Returns(serviceProvider);

        var request = new Mock<HttpRequestData>(context.Object);
        var response = new Mock<HttpResponseData>(context.Object);
        response.SetupProperty(r => r.StatusCode);
        response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
        response.Setup(r => r.Body).Returns(new MemoryStream());

        request.Setup(r => r.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(body)));
        request.Setup(r => r.Headers).Returns(CreateHeaders(secretToken));
        request.Setup(r => r.CreateResponse()).Returns(response.Object);

        return request.Object;
    }

    private static HttpHeadersCollection CreateHeaders(string? secretToken)
    {
        var headers = new HttpHeadersCollection();
        if (!string.IsNullOrWhiteSpace(secretToken))
        {
            headers.Add("X-Telegram-Bot-Api-Secret-Token", secretToken);
        }

        return headers;
    }
}

