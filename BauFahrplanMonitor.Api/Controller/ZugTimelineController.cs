using BauFahrplanMonitor.Core.Data.Repositories;
using BauFahrplanMonitor.Core.Dto;
using BauFahrplanMonitor.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace BauFahrplanMonitor.Api.Controller;

[ApiController]
[Route("api/zug")]
public sealed class ZugTimelineController : ControllerBase {

    private readonly ZugTimelineRepository _repo;
    private readonly ZugTimelineService    _service;

    public ZugTimelineController(
        ZugTimelineRepository repo,
        ZugTimelineService    service
    ) {
        _repo    = repo;
        _service = service;
    }

    [HttpGet("{zugNr:int}/timeline")]
    public async Task<ActionResult<ZugTimelineResult>> Get(
        int                  zugNr,
        [FromQuery] DateOnly date,
        CancellationToken    ct
    ) {
        var rows   = await _repo.LoadAsync(zugNr, date, ct);
        var result = _service.Build(zugNr, date, rows);
        return Ok(result);
    }
}

