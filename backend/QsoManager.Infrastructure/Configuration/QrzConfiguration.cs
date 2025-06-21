namespace QsoManager.Infrastructure.Configuration;

public class QrzConfiguration
{
    public const string SectionName = "QRZ";
    
    public string ApiUrl { get; set; } = "https://xmldata.qrz.com/xml/current/";
    
    public QrzTestCredentials? TestCredentials { get; set; }
}

public class QrzTestCredentials
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}
