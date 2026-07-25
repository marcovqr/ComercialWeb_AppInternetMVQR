using System.ComponentModel.DataAnnotations;
using System.Data;
using Comercial.Infrastructure.Persistence.Scaffolded;
using Comercial.Infrastructure.Persistence.Scaffolded.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Comercial.Api.Controllers;

[Authorize, ApiController, Route("api/servicios")]
public sealed class ServiciosController(ScaffoldedComercialContext db, ILogger<ServiciosController> log) : ControllerBase
{
    [HttpPost("instalaciones")]
    public async Task<IActionResult> CrearInstalacion(InstalacionRequest input, CancellationToken ct)
    {
        if (!await db.clientes.AnyAsync(x => x.cli_cedula == input.ClienteCedula && x.cli_estado != "ANU", ct))
            return BadRequest(new ProblemDetails { Title = "El cliente no existe o está anulado." });
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var codigo = (await db.Instalaciones.MaxAsync(x => (int?)x.ins_codigo, ct) ?? 0) + 1;
        var hoy = DateTime.Now;
        var x = new Instalacione { ins_codigo = codigo, cli_cedula = input.ClienteCedula.Trim(), ins_fecha_instalacion = input.FechaInstalacion, ins_costo_instalacion = input.CostoInstalacion, ins_mensual = input.ValorMensual, ins_fecha_actual = hoy, ins_dias_servicio = Math.Max(0, (hoy.Date - input.FechaInstalacion.Date).TotalDays), ins_password_wifi = input.PasswordWifi, ins_ip_antena_wan = input.IpAntenaWan, ins_login_antena = input.LoginAntena, ins_password_antena = input.PasswordAntena, ins_ip_wan_router = input.IpWanRouter, ins_login_router = input.LoginRouter, ins_password_router = input.PasswordRouter, ins_ip_access_point = input.IpAccessPoint, ins_estado = "ACT", ins_descripcion_antena = input.DescripcionAntena, ins_observaciones = input.Observaciones, ins_fecha_creacion = hoy.ToString("yyyy-MM-dd HH:mm:ss"), ins_fecha_mod = hoy.ToString("yyyy-MM-dd HH:mm:ss") };
        db.Instalaciones.Add(x); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        log.LogInformation("Instalación {Codigo} creada para {Cliente}", codigo, input.ClienteCedula);
        return Created($"/api/servicios/instalaciones/{codigo}", new { codigo });
    }

    [HttpPut("instalaciones/{codigo:int}")]
    public async Task<IActionResult> EditarInstalacion(int codigo, InstalacionRequest input, CancellationToken ct)
    {
        var x = await db.Instalaciones.FindAsync([codigo], ct); if (x is null) return NotFound();
        x.cli_cedula=input.ClienteCedula.Trim();x.ins_fecha_instalacion=input.FechaInstalacion;x.ins_costo_instalacion=input.CostoInstalacion;x.ins_mensual=input.ValorMensual;if(!string.IsNullOrWhiteSpace(input.PasswordWifi))x.ins_password_wifi=input.PasswordWifi;x.ins_ip_antena_wan=input.IpAntenaWan;x.ins_login_antena=input.LoginAntena;if(!string.IsNullOrWhiteSpace(input.PasswordAntena))x.ins_password_antena=input.PasswordAntena;x.ins_ip_wan_router=input.IpWanRouter;x.ins_login_router=input.LoginRouter;if(!string.IsNullOrWhiteSpace(input.PasswordRouter))x.ins_password_router=input.PasswordRouter;x.ins_ip_access_point=input.IpAccessPoint;x.ins_descripcion_antena=input.DescripcionAntena;x.ins_observaciones=input.Observaciones;x.ins_fecha_mod=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        await db.SaveChangesAsync(ct); return NoContent();
    }

    [HttpDelete("instalaciones/{codigo:int}")]
    public async Task<IActionResult> AnularInstalacion(int codigo,CancellationToken ct){var x=await db.Instalaciones.FindAsync([codigo],ct);if(x is null)return NotFound();x.ins_estado="ANU";x.ins_fecha_mod=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");await db.SaveChangesAsync(ct);return NoContent();}

    [HttpPost("mensualidades")]
    public async Task<IActionResult> CrearMensualidad(MensualidadRequest input,CancellationToken ct)
    {
        if(input.Hasta<=input.Desde)return BadRequest(new ProblemDetails{Title="La fecha hasta debe ser posterior a la fecha desde."});
        var instalacion=await db.Instalaciones.AsNoTracking().SingleOrDefaultAsync(x=>x.ins_codigo==input.InstalacionCodigo&&x.ins_estado!="ANU",ct);
        if(instalacion is null)return BadRequest(new ProblemDetails{Title="La instalación no existe o está anulada."});
        var desde=input.Desde.ToString("yyyy-MM-dd");var hasta=input.Hasta.ToString("yyyy-MM-dd");
        if(await db.mensualidades.AnyAsync(x=>x.ins_codigo==input.InstalacionCodigo&&x.men_fechadesde==desde&&x.men_fechahasta==hasta,ct))return Conflict(new ProblemDetails{Title="La mensualidad de ese periodo ya existe."});
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);var hoy=DateOnly.FromDateTime(DateTime.Now);
        var nueva=new mensualidade{ins_codigo=input.InstalacionCodigo,cli_cedula=instalacion.cli_cedula??input.ClienteCedula,men_fechadesde=desde,men_fechahasta=hasta,men_cuota=input.Cuota,men_estado="PENDIENTE",men_fechacrea=hoy,men_fechamod=hoy,fac_numero=null};db.mensualidades.Add(nueva);await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return Created($"/api/servicios/mensualidades/{nueva.men_codigo}",new{codigo=nueva.men_codigo});
    }

    [HttpPost("mensualidades/generar")]
    public async Task<IActionResult> GenerarMensualidades(GenerarMensualidadesRequest input,CancellationToken ct)
    {
        var cedula=input.ClienteCedula.Trim();
        var cliente=await db.clientes.AsNoTracking().SingleOrDefaultAsync(x=>x.cli_cedula==cedula&&x.cli_estado!="ANU",ct);
        if(cliente is null)return BadRequest(new ProblemDetails{Title="El cliente no existe o está anulado."});
        var tarifaEspecial=EsJuanCarlosQuitoRivera(cliente);
        var ultimaDelCliente=await db.mensualidades.AsNoTracking().Where(x=>x.cli_cedula==cedula).OrderByDescending(x=>x.men_fechahasta).ThenByDescending(x=>x.men_codigo).FirstOrDefaultAsync(ct);
        var consultaInstalaciones=db.Instalaciones.Where(x=>x.cli_cedula==cedula&&(x.ins_estado==null||x.ins_estado!="ANU")&&x.ins_fecha_instalacion!=null&&(x.ins_mensual>0||tarifaEspecial));
        var instalaciones=ultimaDelCliente is null?await consultaInstalaciones.OrderByDescending(x=>x.ins_fecha_instalacion).ThenByDescending(x=>x.ins_codigo).Take(1).ToListAsync(ct):await consultaInstalaciones.Where(x=>x.ins_codigo==ultimaDelCliente.ins_codigo).ToListAsync(ct);
        if(instalaciones.Count==0)return BadRequest(new ProblemDetails{Title="El cliente no tiene instalaciones activas."});
        var hasta=input.Hasta??DateOnly.FromDateTime(DateTime.Today);
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var hoy=DateOnly.FromDateTime(DateTime.Today);var creadas=new List<object>();
        foreach(var instalacion in instalaciones)
        {
            var periodos=await db.mensualidades.AsNoTracking().Where(x=>x.ins_codigo==instalacion.ins_codigo).Select(x=>x.men_fechahasta).ToListAsync(ct);
            var ultima=periodos.Select(x=>DateOnly.TryParse(x,out var fecha)?fecha:(DateOnly?)null).Where(x=>x.HasValue).Max();
            var desde=ultima??DateOnly.FromDateTime(instalacion.ins_fecha_instalacion!.Value);
            var cuota=tarifaEspecial&&(!instalacion.ins_mensual.HasValue||instalacion.ins_mensual<=0)?20d:instalacion.ins_mensual!.Value;
            while(desde.AddMonths(1)<=hasta){var fin=desde.AddMonths(1);var desdeTexto=desde.ToString("yyyy-MM-dd");var finTexto=fin.ToString("yyyy-MM-dd");var existe=await db.mensualidades.AnyAsync(x=>x.ins_codigo==instalacion.ins_codigo&&x.men_fechadesde==desdeTexto&&x.men_fechahasta==finTexto,ct);if(!existe){db.mensualidades.Add(new mensualidade{ins_codigo=instalacion.ins_codigo,cli_cedula=cedula,men_fechadesde=desdeTexto,men_fechahasta=finTexto,men_cuota=Convert.ToDecimal(cuota),men_estado="PENDIENTE",men_fechacrea=hoy,men_fechamod=hoy,fac_numero=null});creadas.Add(new{instalacion=instalacion.ins_codigo,desde=desdeTexto,hasta=finTexto,cuota});}desde=fin;}
        }
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
        log.LogInformation("Generadas {Cantidad} mensualidades para {Cliente} por {Usuario}",creadas.Count,cedula,User.Identity?.Name);
        return Ok(new{cantidad=creadas.Count,mensualidades=creadas,mensaje=creadas.Count==0?"Las mensualidades ya están generadas hasta la fecha indicada.":$"Se generaron {creadas.Count} mensualidades."});
    }

    [HttpPost("mensualidades/previsualizar")]
    public async Task<IActionResult> PrevisualizarMensualidades(GenerarMensualidadesRequest input,CancellationToken ct)
    {
        var cedula=input.ClienteCedula.Trim();var hasta=input.Hasta??DateOnly.FromDateTime(DateTime.Today);
        var cliente=await db.clientes.AsNoTracking().SingleOrDefaultAsync(x=>x.cli_cedula==cedula&&x.cli_estado!="ANU",ct);if(cliente is null)return BadRequest(new ProblemDetails{Title="El cliente no existe o está anulado."});
        var tarifaEspecial=EsJuanCarlosQuitoRivera(cliente);
        var ultimaDelCliente=await db.mensualidades.AsNoTracking().Where(x=>x.cli_cedula==cedula).OrderByDescending(x=>x.men_fechahasta).ThenByDescending(x=>x.men_codigo).FirstOrDefaultAsync(ct);
        var consultaInstalaciones=db.Instalaciones.AsNoTracking().Where(x=>x.cli_cedula==cedula&&(x.ins_estado==null||x.ins_estado!="ANU")&&x.ins_fecha_instalacion!=null&&(x.ins_mensual>0||tarifaEspecial));
        var instalaciones=ultimaDelCliente is null?await consultaInstalaciones.OrderByDescending(x=>x.ins_fecha_instalacion).ThenByDescending(x=>x.ins_codigo).Take(1).ToListAsync(ct):await consultaInstalaciones.Where(x=>x.ins_codigo==ultimaDelCliente.ins_codigo).ToListAsync(ct);
        if(instalaciones.Count==0)return BadRequest(new ProblemDetails{Title="La instalación de la última mensualidad está anulada o no tiene una cuota válida."});
        var resultado=new List<object>();
        foreach(var instalacion in instalaciones)
        {
            var periodos=await db.mensualidades.AsNoTracking().Where(x=>x.ins_codigo==instalacion.ins_codigo).Select(x=>x.men_fechahasta).ToListAsync(ct);
            var ultima=periodos.Select(x=>DateOnly.TryParse(x,out var fecha)?fecha:(DateOnly?)null).Where(x=>x.HasValue).Max();
            var desde=ultima??DateOnly.FromDateTime(instalacion.ins_fecha_instalacion!.Value);
            var cuota=tarifaEspecial&&(!instalacion.ins_mensual.HasValue||instalacion.ins_mensual<=0)?20d:instalacion.ins_mensual!.Value;
            while(desde.AddMonths(1)<=hasta){var fin=desde.AddMonths(1);var desdeTexto=desde.ToString("yyyy-MM-dd");var finTexto=fin.ToString("yyyy-MM-dd");var existe=await db.mensualidades.AnyAsync(x=>x.ins_codigo==instalacion.ins_codigo&&x.men_fechadesde==desdeTexto&&x.men_fechahasta==finTexto,ct);if(!existe)resultado.Add(new{instalacionCodigo=instalacion.ins_codigo,desde=desdeTexto,hasta=finTexto,cuota});desde=fin;}
        }
        return Ok(new{cantidad=resultado.Count,mensualidades=resultado,mensaje=resultado.Count==0?"No existen mensualidades nuevas para crear.":$"Revise las {resultado.Count} mensualidades antes de confirmar."});
    }

    [HttpGet("clientes/{cedula}/mensualidades")]
    public async Task<IActionResult> HistorialMensualidades(string cedula,CancellationToken ct)
    {
        var rows=await db.mensualidades.AsNoTracking().Where(x=>x.cli_cedula==cedula).OrderByDescending(x=>x.men_fechadesde).ThenByDescending(x=>x.men_codigo).Select(x=>new{codigo=x.men_codigo,instalacionCodigo=x.ins_codigo,clienteCedula=x.cli_cedula,desde=x.men_fechadesde,hasta=x.men_fechahasta,cuota=x.men_cuota,factura=x.fac_numero,estado=x.men_estado}).ToListAsync(ct);
        return Ok(rows);
    }

    [HttpPost("mensualidades/deshacer-generacion-hoy")]
    public async Task<IActionResult> DeshacerGeneracionHoy(DeshacerMensualidadesRequest input,CancellationToken ct)
    {
        var cedula=input.ClienteCedula.Trim();var hoy=DateOnly.FromDateTime(DateTime.Today);
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var activas=await db.Instalaciones.AsNoTracking().Where(x=>x.cli_cedula==cedula&&(x.ins_estado==null||x.ins_estado!="ANU")&&x.ins_mensual>0).Select(x=>x.ins_codigo).ToListAsync(ct);
        var candidatas=await db.mensualidades.Where(x=>x.cli_cedula==cedula&&x.men_fechacrea==hoy&&x.men_estado=="PENDIENTE"&&x.fac_numero==null).ToListAsync(ct);
        var conservar=candidatas.Where(x=>activas.Contains(x.ins_codigo)).GroupBy(x=>x.ins_codigo).Select(g=>g.OrderByDescending(x=>x.men_fechahasta).ThenByDescending(x=>x.men_codigo).First().men_codigo).ToHashSet();
        var eliminar=candidatas.Where(x=>!conservar.Contains(x.men_codigo)).ToList();
        if(eliminar.Count>0)db.mensualidades.RemoveRange(eliminar);
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
        log.LogWarning("Reversión de generación mensual para {Cliente} por {Usuario}: {Eliminadas} eliminadas, {Conservadas} conservadas",cedula,User.Identity?.Name,eliminar.Count,conservar.Count);
        return Ok(new{eliminadas=eliminar.Count,conservadas=conservar.Count,mensaje=$"Se eliminaron {eliminar.Count} mensualidades generadas de más y se conservaron {conservar.Count}."});
    }

    [HttpDelete("mensualidades/{codigo:int}")]
    public async Task<IActionResult> AnularMensualidad(int codigo,CancellationToken ct){var x=await db.mensualidades.FindAsync([codigo],ct);if(x is null)return NotFound();if(x.fac_numero is not null)return Conflict(new ProblemDetails{Title="No se puede anular una mensualidad ya facturada."});x.men_estado="ANU";x.men_fechamod=DateOnly.FromDateTime(DateTime.Now);await db.SaveChangesAsync(ct);return NoContent();}

    [HttpPut("mensualidades/{codigo:int}")]
    public async Task<IActionResult> EditarMensualidad(int codigo,MensualidadRequest input,CancellationToken ct){var x=await db.mensualidades.FindAsync([codigo],ct);if(x is null)return NotFound();if(x.fac_numero is not null)return Conflict(new ProblemDetails{Title="No se puede editar una mensualidad ya facturada."});if(input.Hasta<=input.Desde)return BadRequest(new ProblemDetails{Title="La fecha hasta debe ser posterior a la fecha desde."});x.ins_codigo=input.InstalacionCodigo;x.cli_cedula=input.ClienteCedula.Trim();x.men_fechadesde=input.Desde.ToString("yyyy-MM-dd");x.men_fechahasta=input.Hasta.ToString("yyyy-MM-dd");x.men_cuota=input.Cuota;x.men_fechamod=DateOnly.FromDateTime(DateTime.Now);await db.SaveChangesAsync(ct);return NoContent();}

    private static bool EsJuanCarlosQuitoRivera(cliente x)
    {
        var nombre=$"{x.cli_nombres} {x.cli_apellidos}".ToUpperInvariant();
        return nombre.Contains("JUAN")&&nombre.Contains("CARLOS")&&nombre.Contains("QUITO")&&nombre.Contains("RIVERA");
    }
}

public sealed record InstalacionRequest([Required,StringLength(13)]string ClienteCedula,DateTime FechaInstalacion,[Range(0,double.MaxValue)]double CostoInstalacion,[Range(0,double.MaxValue)]double ValorMensual,[StringLength(255)]string? PasswordWifi,[StringLength(255)]string? IpAntenaWan,[StringLength(255)]string? LoginAntena,[StringLength(255)]string? PasswordAntena,[StringLength(255)]string? IpWanRouter,[StringLength(50)]string? LoginRouter,[StringLength(255)]string? PasswordRouter,[StringLength(255)]string? IpAccessPoint,[StringLength(255)]string? DescripcionAntena,[StringLength(255)]string? Observaciones);
public sealed record MensualidadRequest(int InstalacionCodigo,[Required,StringLength(13)]string ClienteCedula,DateOnly Desde,DateOnly Hasta,[Range(0,double.MaxValue)]decimal Cuota);
public sealed record GenerarMensualidadesRequest([Required,StringLength(13,MinimumLength=10)]string ClienteCedula,DateOnly? Hasta);
public sealed record DeshacerMensualidadesRequest([Required,StringLength(13,MinimumLength=10)]string ClienteCedula);
