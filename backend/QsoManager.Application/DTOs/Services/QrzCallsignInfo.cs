namespace QsoManager.Application.DTOs.Services;

public class QrzCallsignInfo
{
    public string CallSign { get; set; } = string.Empty;
    public string? FName { get; set; }
    public string? Name { get; set; }
    public string? Nickname { get; set; }
    public string? NameFmt { get; set; }
    public string? Addr1 { get; set; }
    public string? Addr2 { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? Country { get; set; }
    public string? Land { get; set; }
    public double? Lat { get; set; }
    public double? Lon { get; set; }
    public string? Grid { get; set; }
    public string? County { get; set; }
    public string? Class { get; set; }
    public string? Email { get; set; }
    public string? Url { get; set; }
    public string? QslManager { get; set; }
    public string? TimeZone { get; set; }
    public string? GeoLoc { get; set; }
    public int? CqZone { get; set; }
    public int? ItuZone { get; set; }
    public int? Dxcc { get; set; }
    public string? Image { get; set; }
    public string? Bio { get; set; }
    public string? Eqsl { get; set; }
    public string? Mqsl { get; set; }
    public string? Lotw { get; set; }
    public string? Iota { get; set; }
    public DateTime? FetchedAt { get; set; }
}
