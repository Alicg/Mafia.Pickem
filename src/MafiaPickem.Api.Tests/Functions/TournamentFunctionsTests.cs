using Azure.Core.Serialization;
using FluentAssertions;
using MafiaPickem.Api.Auth;
using MafiaPickem.Api.Data;
using MafiaPickem.Api.Functions;
using MafiaPickem.Api.Models.Domain;
using MafiaPickem.Api.Models.Responses;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using System.Text;
using System.Text.Json;
using DomainMatch = MafiaPickem.Api.Models.Domain.Match;

namespace MafiaPickem.Api.Tests.Functions;

public class TournamentFunctionsTests
{
    private readonly Mock<ITournamentRepository> _mockTournamentRepository;
    private readonly Mock<IMatchRepository> _mockMatchRepository;
    private readonly Mock<IPredictionRepository> _mockPredictionRepository;
    private readonly Mock<ITournamentParticipantRepository> _mockTournamentParticipantRepository;
    private readonly Mock<ITournamentOperatorRepository> _mockTournamentOperatorRepository;
    private readonly Mock<IUserContext> _mockUserContext;
    private readonly Mock<ILogger<TournamentFunctions>> _mockLogger;
    private readonly TournamentFunctions _tournamentFunctions;

    public TournamentFunctionsTests()
    {
        _mockTournamentRepository = new Mock<ITournamentRepository>();
        _mockMatchRepository = new Mock<IMatchRepository>();
        _mockPredictionRepository = new Mock<IPredictionRepository>();
        _mockTournamentParticipantRepository = new Mock<ITournamentParticipantRepository>();
        _mockTournamentOperatorRepository = new Mock<ITournamentOperatorRepository>();
        _mockUserContext = new Mock<IUserContext>();
        _mockLogger = new Mock<ILogger<TournamentFunctions>>();

        _tournamentFunctions = new TournamentFunctions(
            _mockTournamentRepository.Object,
            _mockMatchRepository.Object,
            _mockPredictionRepository.Object,
            _mockTournamentParticipantRepository.Object,
            _mockTournamentOperatorRepository.Object,
            _mockUserContext.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetActiveTournaments_WhenTournamentHiddenForViewer_ShouldExcludeItFromResponse()
    {
        _mockUserContext.SetupGet(x => x.IsAdmin).Returns(false);
        _mockUserContext.SetupGet(x => x.IsRegistered).Returns(false);
        _mockUserContext.SetupGet(x => x.TelegramUsername).Returns((string?)null);

        var visibleTournament = CreateTournament(1, "Visible Cup", true);
        var hiddenTournament = CreateTournament(2, "Hidden Cup", false);

        _mockTournamentRepository.Setup(x => x.GetActiveAsync())
            .ReturnsAsync(new[] { visibleTournament, hiddenTournament });
        _mockMatchRepository.Setup(x => x.GetCurrentMatchByTournamentIdAsync(1))
            .ReturnsAsync((DomainMatch?)null);

        var request = CreateMockHttpRequest();

        var response = await _tournamentFunctions.GetActiveTournamentsHttp(request);
        var payload = await ReadJsonAsync<List<TournamentDto>>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        payload.Should().HaveCount(1);
        payload[0].Id.Should().Be(1);
        payload[0].VisibleOnHomePage.Should().BeTrue();
        _mockMatchRepository.Verify(x => x.GetCurrentMatchByTournamentIdAsync(1), Times.Once);
        _mockMatchRepository.Verify(x => x.GetCurrentMatchByTournamentIdAsync(2), Times.Never);
    }

    [Fact]
    public async Task GetActiveTournaments_WhenTournamentHiddenForAdmin_ShouldIncludeItInResponse()
    {
        _mockUserContext.SetupGet(x => x.IsAdmin).Returns(true);
        _mockUserContext.SetupGet(x => x.IsRegistered).Returns(false);

        var hiddenTournament = CreateTournament(7, "Hidden Cup", false);

        _mockTournamentRepository.Setup(x => x.GetActiveAsync())
            .ReturnsAsync(new[] { hiddenTournament });
        _mockMatchRepository.Setup(x => x.GetCurrentMatchByTournamentIdAsync(7))
            .ReturnsAsync((DomainMatch?)null);
        _mockTournamentOperatorRepository.Setup(x => x.GetByTournamentIdAsync(7))
            .ReturnsAsync(Array.Empty<string>());

        var request = CreateMockHttpRequest();

        var response = await _tournamentFunctions.GetActiveTournamentsHttp(request);
        var payload = await ReadJsonAsync<List<TournamentDto>>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        payload.Should().HaveCount(1);
        payload[0].Id.Should().Be(7);
        payload[0].VisibleOnHomePage.Should().BeFalse();
        payload[0].CanManage.Should().BeTrue();
    }

    private static Tournament CreateTournament(int id, string name, bool visibleOnHomePage)
    {
        return new Tournament
        {
            Id = id,
            Name = name,
            Active = true,
            VisibleOnHomePage = visibleOnHomePage,
            TeamsJson = "[]"
        };
    }

    private static HttpRequestData CreateMockHttpRequest(string body = "{}")
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddOptions();
        serviceCollection.Configure<WorkerOptions>(options =>
        {
            options.Serializer = new JsonObjectSerializer(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });
        });
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var context = new Mock<FunctionContext>();
        context.Setup(c => c.InstanceServices).Returns(serviceProvider);

        var request = new Mock<HttpRequestData>(context.Object);
        var response = new Mock<HttpResponseData>(context.Object);
        response.SetupProperty(r => r.StatusCode);
        response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
        response.Setup(r => r.Body).Returns(new MemoryStream());

        request.Setup(r => r.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(body)));
        request.Setup(r => r.CreateResponse()).Returns(response.Object);

        return request.Object;
    }

    private static async Task<T> ReadJsonAsync<T>(HttpResponseData response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(response.Body, Encoding.UTF8, leaveOpen: true);
        var json = await reader.ReadToEndAsync();

        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        })!;
    }
}