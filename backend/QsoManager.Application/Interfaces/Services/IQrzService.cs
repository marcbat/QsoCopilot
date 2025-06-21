using QsoManager.Application.DTOs.Services;

namespace QsoManager.Application.Interfaces.Services;

public interface IQrzService
{
    /// <summary>
    /// Lookup callsign information from QRZ.com
    /// </summary>
    /// <param name="callsign">The callsign to lookup</param>
    /// <param name="qrzUsername">QRZ username (optional, for authenticated lookups)</param>
    /// <param name="qrzPassword">QRZ password (optional, for authenticated lookups)</param>
    /// <returns>Callsign information from QRZ or null if not found</returns>
    Task<QrzCallsignInfo?> LookupCallsignAsync(
        string callsign, 
        string? qrzUsername = null, 
        string? qrzPassword = null);

    /// <summary>
    /// Lookup DXCC information from QRZ.com
    /// </summary>
    /// <param name="dxccId">The DXCC ID to lookup</param>
    /// <param name="qrzUsername">QRZ username (required for DXCC lookups)</param>
    /// <param name="qrzPassword">QRZ password (required for DXCC lookups)</param>
    /// <returns>DXCC information from QRZ or null if not found</returns>
    Task<QrzDxccInfo?> LookupDxccAsync(
        int dxccId, 
        string? qrzUsername = null, 
        string? qrzPassword = null);
}
