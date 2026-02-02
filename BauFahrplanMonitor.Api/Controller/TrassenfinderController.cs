using BauFahrplanMonitor.Trassenfinder.Services;
using Microsoft.AspNetCore.Mvc;

namespace BauFahrplanMonitor.Api.Controller;

[ApiController]
[Route("api/trassenfinder")]
public class TrassenfinderController : ControllerBase {
    private readonly ITrassenfinderService _service;

    public TrassenfinderController(ITrassenfinderService service) {
        this._service = service;
    }

    /// <summary>
    /// Liefert alle verfügbaren Infrastrukturen (z. B. Jahresfahrpläne)
    /// </summary>
    [HttpGet("infrastrukturen")]
    public async Task<IActionResult> GetInfrastrukturen() {
        var result = await this._service.LadeInfrastrukturAsync();
        return Ok(result);
    }

    /// <summary>
    /// Liefert eine einzelne Infrastruktur per ID
    /// </summary>
    [HttpGet("infrastrukturen/{id:long}")]
    public async Task<IActionResult> GetInfrastruktur(long id) {
        var result = await this._service.LadeInfrastrukturAsync(id);
        return Ok(result);
    }
}