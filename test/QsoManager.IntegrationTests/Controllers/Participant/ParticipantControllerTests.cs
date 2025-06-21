using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Application.Queries.Participant;
using QsoManager.IntegrationTests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace QsoManager.IntegrationTests.Controllers.Participant;

[Collection("Integration Tests")]
public class ParticipantControllerTests : BaseIntegrationTest
{
    public ParticipantControllerTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }

    [Fact]
    public async Task GetQrzInfo_WithValidCallSign_ShouldReturnParticipantQrzInfo()
    {
        // Arrange
        var callSign = "F4ABC";

        // Act
        var response = await _client.GetAsync($"/api/Participant/{callSign}/qrz");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var qrzInfo = await response.Content.ReadFromJsonAsync<ParticipantQrzInfoDto>();
        
        Assert.NotNull(qrzInfo);
        Assert.Equal(callSign, qrzInfo.CallSign);
        
        // Sans credentials QRZ, les informations enrichies devraient être null
        Assert.Null(qrzInfo.QrzCallsignInfo);
        Assert.Null(qrzInfo.QrzDxccInfo);
    }    [Fact]
    public async Task GetQrzInfo_WithWhitespaceCallSign_ShouldReturnBadRequest()
    {
        // Arrange
        var callSign = " "; // Espace blanc

        // Act
        var response = await _client.GetAsync($"/api/Participant/{Uri.EscapeDataString(callSign)}/qrz");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetQrzInfo_WithAuthenticatedUserAndQrzCredentials_ShouldReturnEnrichedInfo()
    {
        // Arrange
        var testCallsign = "F4QRZ";
        var participantCallsign = "HB3XAJ"; // Un indicatif connu dans QRZ pour les tests
        var (userId, token) = await CreateAndAuthenticateUserAsync(testCallsign);

        // Vérifier que les credentials QRZ sont disponibles
        var qrzCredentials = QrzSecretsHelper.GetQrzCredentials();
        if (qrzCredentials == null)
        {
            // Skip le test si les credentials QRZ ne sont pas disponibles
            Console.WriteLine("SKIPPED: QRZ credentials not available");
            return;
        }

        var (qrzUsername, qrzPassword) = qrzCredentials.Value;

        // Mettre à jour le profil utilisateur avec des credentials QRZ
        var updateProfileRequest = new
        {
            Email = "test@example.com",
            QrzUsername = qrzUsername,
            QrzPassword = qrzPassword
        };

        var profileResponse = await _client.PutAsJsonAsync("/api/auth/profile", updateProfileRequest);
        profileResponse.EnsureSuccessStatusCode();

        // Act
        var response = await _client.GetAsync($"/api/Participant/{participantCallsign}/qrz");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var qrzInfo = await response.Content.ReadFromJsonAsync<ParticipantQrzInfoDto>();
        
        Assert.NotNull(qrzInfo);
        Assert.Equal(participantCallsign, qrzInfo.CallSign);
        
        // Avec des credentials QRZ valides, nous devrions avoir des informations enrichies
        Assert.NotNull(qrzInfo.QrzCallsignInfo);
        Assert.Equal(participantCallsign, qrzInfo.QrzCallsignInfo.CallSign);
        
        // Les informations DXCC peuvent être présentes selon l'indicatif
        if (qrzInfo.QrzCallsignInfo.Dxcc.HasValue)
        {
            Assert.NotNull(qrzInfo.QrzDxccInfo);
        }
    }
}
