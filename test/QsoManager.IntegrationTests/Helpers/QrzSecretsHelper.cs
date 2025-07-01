using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace QsoManager.IntegrationTests.Helpers;

public static class QrzSecretsHelper
{
    private static IConfiguration? _configuration;
    
    static QrzSecretsHelper()
    {
        var builder = new ConfigurationBuilder()
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .AddEnvironmentVariables();
            
        _configuration = builder.Build();
    }
    
    public static string? GetQrzUsername()
    {
        return _configuration?["QrzCredentials:Username"] ?? 
               Environment.GetEnvironmentVariable("QRZ_TEST_USERNAME");
    }
    
    public static string? GetQrzPassword()
    {
        return _configuration?["QrzCredentials:Password"] ?? 
               Environment.GetEnvironmentVariable("QRZ_TEST_PASSWORD");
    }
    
    public static bool AreQrzCredentialsAvailable()
    {
        var username = GetQrzUsername();
        var password = GetQrzPassword();
        
        return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
    }
    
    public static (string username, string password)? GetQrzCredentials()
    {
        var username = GetQrzUsername();
        var password = GetQrzPassword();
        
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            return null;
            
        return (username, password);
    }
}
