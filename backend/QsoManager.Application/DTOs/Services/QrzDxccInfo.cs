namespace QsoManager.Application.DTOs.Services;

public class QrzDxccInfo
{
    public int Dxcc { get; set; }
    public string? CountryCode2 { get; set; }
    public string? CountryCode3 { get; set; }
    public string? Name { get; set; }
    public string? Continent { get; set; }
    public int? ItuZone { get; set; }
    public int? CqZone { get; set; }
    public string? TimeZone { get; set; }
    public double? Lat { get; set; }
    public double? Lon { get; set; }
    public string? Notes { get; set; }
}
