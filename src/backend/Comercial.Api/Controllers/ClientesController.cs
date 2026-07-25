using System.ComponentModel.DataAnnotations;
using Comercial.Infrastructure.Persistence.Scaffolded;
using Comercial.Infrastructure.Persistence.Scaffolded.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Comercial.Api.Controllers;

[Authorize, ApiController, Route("api/clientes")]
public sealed class ClientesController(ScaffoldedComercialContext db, ILogger<ClientesController> log) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<ClienteResponse>> Get(string? buscar, CancellationToken ct)
    {
        var query = db.clientes.AsNoTracking().Where(x => x.cli_estado != "ANU");
        if (!string.IsNullOrWhiteSpace(buscar))
            query = query.Where(x => x.cli_cedula.Contains(buscar) || x.cli_apellidos.Contains(buscar) || x.cli_nombres.Contains(buscar));
        return await query.OrderBy(x => x.cli_apellidos).ThenBy(x => x.cli_nombres).Take(200)
            .Select(x => Map(x)).ToListAsync(ct);
    }

    [HttpGet("{cedula}")]
    public async Task<ActionResult<ClienteResponse>> One(string cedula, CancellationToken ct)
    {
        var entity = await db.clientes.AsNoTracking().SingleOrDefaultAsync(x => x.cli_cedula == cedula, ct);
        return entity is null ? NotFound() : Map(entity);
    }

    [HttpPost]
    public async Task<ActionResult<ClienteResponse>> Create(ClienteRequest input, CancellationToken ct)
    {
        if (await db.clientes.AnyAsync(x => x.cli_cedula == input.Cedula, ct))
            return Conflict(new ProblemDetails { Title = "El cliente ya existe." });
        var entity = new cliente { cli_cedula = input.Cedula.Trim(), cli_apellidos = input.Apellidos.Trim(), cli_nombres = input.Nombres.Trim(), cli_direccion = input.Direccion.Trim(), cli_telefono = input.Telefono, cli_email = input.Email, cli_obser = input.Observacion, cli_estado = "ACT", cli_fechamod = DateTime.Now };
        db.clientes.Add(entity);
        await db.SaveChangesAsync(ct);
        log.LogInformation("Cliente {Cedula} creado por {Usuario}", entity.cli_cedula, User.Identity?.Name);
        return CreatedAtAction(nameof(One), new { cedula = entity.cli_cedula }, Map(entity));
    }

    [HttpPut("{cedula}")]
    public async Task<IActionResult> Update(string cedula, ClienteRequest input, CancellationToken ct)
    {
        if (!string.Equals(cedula, input.Cedula, StringComparison.OrdinalIgnoreCase)) return BadRequest(new ProblemDetails { Title = "La cédula no puede cambiar." });
        var entity = await db.clientes.FindAsync([cedula], ct);
        if (entity is null) return NotFound();
        entity.cli_apellidos = input.Apellidos.Trim(); entity.cli_nombres = input.Nombres.Trim(); entity.cli_direccion = input.Direccion.Trim();
        entity.cli_telefono = input.Telefono; entity.cli_email = input.Email; entity.cli_obser = input.Observacion; entity.cli_fechamod = DateTime.Now;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{cedula}")]
    public async Task<IActionResult> Delete(string cedula, CancellationToken ct)
    {
        var entity = await db.clientes.FindAsync([cedula], ct);
        if (entity is null) return NotFound();
        entity.cli_estado = "ANU"; entity.cli_fechamod = DateTime.Now;
        await db.SaveChangesAsync(ct);
        log.LogInformation("Cliente {Cedula} anulado por {Usuario}", cedula, User.Identity?.Name);
        return NoContent();
    }

    private static ClienteResponse Map(cliente x) => new(x.cli_cedula, x.cli_apellidos, x.cli_nombres, x.cli_direccion, x.cli_telefono, x.cli_email, x.cli_obser, x.cli_estado, x.cli_fechamod);
}

public sealed record ClienteRequest([Required, StringLength(13, MinimumLength = 10)] string Cedula, [Required, StringLength(100)] string Apellidos, [Required, StringLength(100)] string Nombres, [Required, StringLength(200)] string Direccion, [StringLength(30)] string? Telefono, [EmailAddress, StringLength(100)] string? Email, [StringLength(500)] string? Observacion);
public sealed record ClienteResponse(string Cedula, string Apellidos, string Nombres, string Direccion, string? Telefono, string? Email, string? Observacion, string Estado, DateTime FechaModificacion);
