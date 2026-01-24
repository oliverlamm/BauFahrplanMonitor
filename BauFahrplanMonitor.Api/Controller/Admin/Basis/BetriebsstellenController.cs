using BauFahrplanMonitor.Api.Dto;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Api.Controller.Admin.Basis;

[ApiController]
[Route("api/admin/basis/betriebsstellen")]
public sealed class BetriebsstellenController : ControllerBase {

    private readonly UjBauDbContext _db;

    public BetriebsstellenController(UjBauDbContext db) {
        this._db = db;
    }

    [HttpGet("list")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BetriebsstellenListRowDto>>> List(
        [FromQuery] string? query,
        [FromQuery] string? basis,
        CancellationToken   token) {

        var q =
            from b in this._db.Set<BasisBetriebsstelle>().AsNoTracking()
            join t in this._db.Set<BasisBetriebsstelleTyp>() on b.TypRef equals t.Id
            join r in this._db.Set<BasisRegion>() on b.RegionRef equals r.Id
            join nb in this._db.Set<BasisNetzbezirk>() on b.NetzbezirkRef equals nb.Id
            select new { b, t, r, nb };

        // 🔍 Textsuche (RL100 oder Name)
        if (!string.IsNullOrWhiteSpace(query)) {
            q = q.Where(x =>
                (x.b.Rl100 != null && EF.Functions.ILike(x.b.Rl100, $"%{query}%")) ||
                (x.b.Name  != null && EF.Functions.ILike(x.b.Name, $"%{query}%")));
        }

        // 🔘 Basisdaten-Filter
        q = basis switch {
            "only"    => q.Where(x => x.b.IstBasisDatensatz == true),
            "without" => q.Where(x => x.b.IstBasisDatensatz != true),
            _         => q
        };

        var result = await q
            .OrderBy(x => x.b.Name)
            .Select(x => new BetriebsstellenListRowDto {
                Id       = x.b.Id,
                Rl100    = x.b.Rl100             ?? "",
                Name     = x.b.Name              ?? "",
                IstBasis = x.b.IstBasisDatensatz ?? false
            })
            .ToListAsync(token);

        return Ok(result);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<BetriebsstelleDetailDto>> Get(
        long              id,
        CancellationToken token) {

        var b = await _db.Set<BasisBetriebsstelle>()
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new {
                x.Id,
                x.Rl100,
                x.Name,
                x.Zustand,

                TypId = x.TypRef,
                Typ   = x.TypRefNavigation.Bezeichner,

                RegionId = x.RegionRef,
                Region   = x.RegionRefNavigation.Bezeichner,

                NetzbezirkId = x.NetzbezirkRef,
                Netzbezirk   = x.NetzbezirkRefNavigation.Name,

                IstBasis = x.IstBasisDatensatz
            })
            .SingleOrDefaultAsync(token);

        if (b == null)
            return NotFound();

        var geo = await _db.Set<BasisBetriebsstelle2strecke>()
            .AsNoTracking()
            .Where(x =>
                x.BstRef == id &&
                x.Shape  != null)
            .Select(x => new {
                VzGNr = (int)x.StreckeRefNavigation.VzgNr,
                x.Shape,
                x.KmL,
                x.KmI
            })
            .ToListAsync(token);   // 🔑 HIER SQL zu Ende

        var geoDtos = geo
            .Select(x => {
                var p = x.Shape as NetTopologySuite.Geometries.Point;

                return new BetriebsstelleGeoDto {
                    VzGNr = x.VzGNr,
                    Lon   = p?.X ?? 0,
                    Lat   = p?.Y ?? 0,
                    KmL   = x.KmL,
                    KmI   = x.KmI
                };
            })
            .OrderBy(x => x.VzGNr)
            .ToList();
        
        return Ok(new BetriebsstelleDetailDto {
            Id      = b.Id,
            Rl100   = b.Rl100 ?? "",
            Name    = b.Name  ?? "",
            Zustand = b.Zustand,

            TypId = b.TypId,
            Typ   = b.Typ ?? "",

            RegionId = b.RegionId,
            Region   = b.Region ?? "",

            NetzbezirkId = b.NetzbezirkId,
            Netzbezirk   = b.Netzbezirk ?? "",

            IstBasis = b.IstBasis ?? false,

            Geo = geoDtos
        });
    }
    
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(
        long                    id,
        BetriebsstelleUpdateDto dto,
        CancellationToken       token) {

        var b = await _db.BasisBetriebsstelle
            .SingleOrDefaultAsync(x => x.Id == id, token);

        if (b == null)
            return NotFound();

        b.Name    = dto.Name;
        b.Zustand = dto.Zustand;

        b.TypRef        = dto.TypId;
        b.RegionRef     = dto.RegionId;
        b.NetzbezirkRef = dto.NetzbezirkId;

        b.IstBasisDatensatz = dto.IstBasis;

        await _db.SaveChangesAsync(token);
        return NoContent();
    }
}