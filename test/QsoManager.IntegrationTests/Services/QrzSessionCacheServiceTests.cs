using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Infrastructure.Services.QRZ;
using QsoManager.Infrastructure.Services.QRZ.Models;
using Xunit;

namespace QsoManager.IntegrationTests.Services;

[Collection("Integration Tests")]
public class QrzSessionCacheServiceTests : BaseIntegrationTest
{
    public QrzSessionCacheServiceTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }

    [Fact]
    public void CacheSession_ShouldStoreSession()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var cacheService = scope.ServiceProvider.GetRequiredService<IQrzSessionCacheService>();
        
        var username = "testuser";
        var sessionKey = "test-session-key-123";
        var expirationDate = DateTime.UtcNow.AddHours(24);

        // Act
        cacheService.CacheSession(username, sessionKey, expirationDate);
        var cachedSession = cacheService.GetCachedSession(username);

        // Assert
        Assert.NotNull(cachedSession);
        Assert.Equal(sessionKey, cachedSession.SessionKey);
        Assert.Equal(username, cachedSession.Username);
        Assert.True(cachedSession.IsValid);
        Assert.False(cachedSession.IsExpiringSoon());
    }

    [Fact]
    public void GetCachedSession_WithExpiredSession_ShouldReturnNull()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var cacheService = scope.ServiceProvider.GetRequiredService<IQrzSessionCacheService>();
        
        var username = "expireduser";
        var sessionKey = "expired-session-key";
        var expirationDate = DateTime.UtcNow.AddMinutes(-1); // Expiré

        // Act
        cacheService.CacheSession(username, sessionKey, expirationDate);
        var cachedSession = cacheService.GetCachedSession(username);

        // Assert
        Assert.Null(cachedSession);
    }

    [Fact]
    public void GetCachedSession_WithExpiringSoonSession_ShouldReturnSessionButMarkAsExpiringSoon()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var cacheService = scope.ServiceProvider.GetRequiredService<IQrzSessionCacheService>();
        
        var username = "expiringuser";
        var sessionKey = "expiring-session-key";
        var expirationDate = DateTime.UtcNow.AddMinutes(15); // Expire dans 15 minutes

        // Act
        cacheService.CacheSession(username, sessionKey, expirationDate);
        var cachedSession = cacheService.GetCachedSession(username);

        // Assert
        Assert.NotNull(cachedSession);
        Assert.Equal(sessionKey, cachedSession.SessionKey);
        Assert.True(cachedSession.IsValid);
        Assert.True(cachedSession.IsExpiringSoon(30)); // Expire dans moins de 30 minutes
    }

    [Fact]
    public void QrzSessionCache_ProactiveRenewal_ShouldTriggerWhenExpiringSoon()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var cacheService = scope.ServiceProvider.GetRequiredService<IQrzSessionCacheService>();
        
        var username = "renewaluser";
        var sessionKey = "renewal-session-key";
        var expirationDate = DateTime.UtcNow.AddMinutes(25); // Expire dans 25 minutes

        // Act
        cacheService.CacheSession(username, sessionKey, expirationDate);
        var cachedSession = cacheService.GetCachedSession(username);

        // Assert
        Assert.NotNull(cachedSession);
        Assert.True(cachedSession.IsValid);
        Assert.True(cachedSession.IsExpiringSoon(30)); // Devrait déclencher un renouvellement proactif
        Assert.False(cachedSession.IsExpiringSoon(20)); // Mais pas si le seuil est plus bas
    }

    [Fact]
    public void RemoveSession_ShouldRemoveFromCache()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var cacheService = scope.ServiceProvider.GetRequiredService<IQrzSessionCacheService>();
        
        var username = "removeuser";
        var sessionKey = "remove-session-key";
        var expirationDate = DateTime.UtcNow.AddHours(24);

        // Act
        cacheService.CacheSession(username, sessionKey, expirationDate);
        var cachedBefore = cacheService.GetCachedSession(username);
        
        cacheService.RemoveSession(username);
        var cachedAfter = cacheService.GetCachedSession(username);

        // Assert
        Assert.NotNull(cachedBefore);
        Assert.Null(cachedAfter);
    }

    [Fact]
    public void CleanupExpiredSessions_ShouldRemoveOnlyExpiredSessions()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var cacheService = scope.ServiceProvider.GetRequiredService<IQrzSessionCacheService>();
        
        var validUser = "validuser";
        var validSessionKey = "valid-session-key";
        var validExpirationDate = DateTime.UtcNow.AddHours(24);
        
        var expiredUser = "expireduser2";
        var expiredSessionKey = "expired-session-key2";
        var expiredExpirationDate = DateTime.UtcNow.AddMinutes(-5);

        // Act
        cacheService.CacheSession(validUser, validSessionKey, validExpirationDate);
        cacheService.CacheSession(expiredUser, expiredSessionKey, expiredExpirationDate);
        
        cacheService.CleanupExpiredSessions();
        
        var validSessionAfterCleanup = cacheService.GetCachedSession(validUser);
        var expiredSessionAfterCleanup = cacheService.GetCachedSession(expiredUser);

        // Assert
        Assert.NotNull(validSessionAfterCleanup);
        Assert.Null(expiredSessionAfterCleanup);
    }
}
