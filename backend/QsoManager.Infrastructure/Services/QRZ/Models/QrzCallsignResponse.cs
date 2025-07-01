using System.Xml.Serialization;

namespace QsoManager.Infrastructure.Services.QRZ.Models;

[XmlRoot("QRZDatabase")]
public class QrzDatabase
{
    [XmlElement("Session")]
    public QrzSession? Session { get; set; }

    [XmlElement("Callsign")]
    public QrzCallsign? Callsign { get; set; }

    [XmlElement("DXCC")]
    public QrzDxcc? Dxcc { get; set; }
}

public class QrzSession
{
    [XmlElement("Key")]
    public string? Key { get; set; }

    [XmlElement("Count")]
    public int? Count { get; set; }

    [XmlElement("SubExp")]
    public string? SubExp { get; set; }

    [XmlElement("GMTime")]
    public string? GMTime { get; set; }

    [XmlElement("Message")]
    public string? Message { get; set; }

    [XmlElement("Error")]
    public string? Error { get; set; }
}

public class QrzCallsign
{
    [XmlElement("call")]
    public string? Call { get; set; }

    [XmlElement("xref")]
    public string? Xref { get; set; }

    [XmlElement("aliases")]
    public string? Aliases { get; set; }

    [XmlElement("dxcc")]
    public int? Dxcc { get; set; }

    [XmlElement("fname")]
    public string? FName { get; set; }

    [XmlElement("name")]
    public string? Name { get; set; }

    [XmlElement("addr1")]
    public string? Addr1 { get; set; }

    [XmlElement("addr2")]
    public string? Addr2 { get; set; }

    [XmlElement("state")]
    public string? State { get; set; }

    [XmlElement("zip")]
    public string? Zip { get; set; }

    [XmlElement("country")]
    public string? Country { get; set; }

    [XmlElement("ccode")]
    public int? CCode { get; set; }

    [XmlElement("lat")]
    public double? Lat { get; set; }

    [XmlElement("lon")]
    public double? Lon { get; set; }

    [XmlElement("grid")]
    public string? Grid { get; set; }

    [XmlElement("county")]
    public string? County { get; set; }

    [XmlElement("fips")]
    public string? Fips { get; set; }

    [XmlElement("land")]
    public string? Land { get; set; }

    [XmlElement("efdate")]
    public string? EffectiveDate { get; set; }

    [XmlElement("expdate")]
    public string? ExpirationDate { get; set; }

    [XmlElement("p_call")]
    public string? PreviousCall { get; set; }

    [XmlElement("class")]
    public string? Class { get; set; }

    [XmlElement("codes")]
    public string? Codes { get; set; }

    [XmlElement("qslmgr")]
    public string? QslManager { get; set; }

    [XmlElement("email")]
    public string? Email { get; set; }

    [XmlElement("url")]
    public string? Url { get; set; }

    [XmlElement("u_views")]
    public int? Views { get; set; }

    [XmlElement("bio")]
    public string? Bio { get; set; }

    [XmlElement("biodate")]
    public string? BioDate { get; set; }

    [XmlElement("image")]
    public string? Image { get; set; }

    [XmlElement("imageinfo")]
    public string? ImageInfo { get; set; }

    [XmlElement("serial")]
    public string? Serial { get; set; }

    [XmlElement("moddate")]
    public string? ModDate { get; set; }

    [XmlElement("MSA")]
    public string? MSA { get; set; }

    [XmlElement("AreaCode")]
    public string? AreaCode { get; set; }

    [XmlElement("TimeZone")]
    public string? TimeZone { get; set; }

    [XmlElement("GMTOffset")]
    public string? GMTOffset { get; set; }

    [XmlElement("DST")]
    public string? DST { get; set; }

    [XmlElement("eqsl")]
    public string? Eqsl { get; set; }

    [XmlElement("mqsl")]
    public string? Mqsl { get; set; }

    [XmlElement("cqzone")]
    public int? CqZone { get; set; }

    [XmlElement("ituzone")]
    public int? ItuZone { get; set; }

    [XmlElement("born")]
    public int? Born { get; set; }

    [XmlElement("user")]
    public string? User { get; set; }

    [XmlElement("lotw")]
    public string? Lotw { get; set; }

    [XmlElement("iota")]
    public string? Iota { get; set; }

    [XmlElement("geoloc")]
    public string? GeoLoc { get; set; }

    [XmlElement("attn")]
    public string? Attn { get; set; }

    [XmlElement("nickname")]
    public string? Nickname { get; set; }

    [XmlElement("name_fmt")]
    public string? NameFmt { get; set; }
}

public class QrzDxcc
{
    [XmlElement("dxcc")]
    public int? Dxcc { get; set; }

    [XmlElement("cc")]
    public string? CountryCode2 { get; set; }

    [XmlElement("ccc")]
    public string? CountryCode3 { get; set; }

    [XmlElement("name")]
    public string? Name { get; set; }

    [XmlElement("continent")]
    public string? Continent { get; set; }

    [XmlElement("ituzone")]
    public int? ItuZone { get; set; }

    [XmlElement("cqzone")]
    public int? CqZone { get; set; }

    [XmlElement("timezone")]
    public string? TimeZone { get; set; }

    [XmlElement("lat")]
    public double? Lat { get; set; }

    [XmlElement("lon")]
    public double? Lon { get; set; }

    [XmlElement("notes")]
    public string? Notes { get; set; }
}
