using BauFahrplanMonitor.Api.Dto;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Api.Controller.Admin.Basis;

[ApiController]
[Route("api/admin/basis/lookups")]
public sealed class BasisLookupsController : ControllerBase {

    private readonly UjBauDbContext _db;

    public BasisLookupsController(UjBauDbContext db) {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<BasisLookupsDto>> Get(
        CancellationToken token) {

        // -----------------------------
        // Zustand (distinct aus Betriebsstellen)
        // -----------------------------
        var zustaende = await _db.Set<BasisBetriebsstelle>()
            .AsNoTracking()
            .Where(x => x.Zustand != null)
            .Select(x => x.Zustand!)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(token);

        // -----------------------------
        // Typen
        // -----------------------------
        var typen = await _db.Set<BasisBetriebsstelleTyp>()
            .AsNoTracking()
            .OrderBy(x => x.Bezeichner)
            .Select(x => new LookupItemDto {
                Id   = x.Id,
                Name = x.Bezeichner!
            })
            .ToListAsync(token);

        // -----------------------------
        // Regionen
        // -----------------------------
        var regionen = await _db.Set<BasisRegion>()
            .AsNoTracking()
            .OrderBy(x => x.Bezeichner)
            .Select(x => new LookupItemDto {
                Id   = x.Id,
                Name = x.Bezeichner!
            })
            .ToListAsync(token);

        // -----------------------------
        // Netzbezirke
        // -----------------------------
        var netzbezirke = await _db.Set<BasisNetzbezirk>()
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new LookupItemDto {
                Id   = x.Id,
                Name = x.Name!
            })
            .ToListAsync(token);

        return Ok(new BasisLookupsDto {
            Zustaende   = zustaende,
            Typen       = typen,
            Regionen    = regionen,
            Netzbezirke = netzbezirke
        });
    }
}
