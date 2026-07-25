using System.ComponentModel.DataAnnotations;
using System.Data;
using Comercial.Infrastructure.Persistence.Scaffolded;
using Comercial.Infrastructure.Persistence.Scaffolded.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Comercial.Api.Controllers;

[Authorize, ApiController, Route("api/administracion")]
public sealed class AdministracionController(ScaffoldedComercialContext db, ILogger<AdministracionController> log) : ControllerBase
{
    [HttpGet("proveedores/{ciRuc}")]
    public async Task<IActionResult> Proveedor(string ciRuc,CancellationToken ct){var x=await db.proveedores.AsNoTracking().Where(x=>x.prov_ciruc==ciRuc&&x.prov_estado!="ANU").Select(x=>new{ciRuc=x.prov_ciruc,razonSocial=x.prov_razonsocial,nombres=x.prov_nombres,apellidos=x.prov_apellidos,direccion=x.prov_direccion,telefono=x.prov_telefono,celular=x.prov_celular,email=x.prov_email,formaPago=x.prov_fpago}).SingleOrDefaultAsync(ct);return x is null?NotFound():Ok(x);}

    [HttpGet("marcas")]
    public async Task<object> Marcas(CancellationToken ct) => await db.marcas.AsNoTracking().Where(x => x.mar_estado != "ANU").OrderBy(x => x.mar_descripcion).Select(x => new { mar_codigo = x.mar_codigo, descripcion = x.mar_descripcion, modelo = x.mar_modelo, estado = x.mar_estado }).ToListAsync(ct);

    [HttpPost("marcas")]
    public async Task<IActionResult> CrearMarca(MarcaRequest input, CancellationToken ct)
    {
        var entity = new marca { mar_descripcion = input.Descripcion.Trim(), mar_modelo = input.Modelo.Trim(), mar_estado = "ACT" };
        db.marcas.Add(entity); await db.SaveChangesAsync(ct);
        return Created($"/api/administracion/marcas/{entity.mar_codigo}", entity);
    }

    [HttpPut("marcas/{codigo:int}")]
    public async Task<IActionResult> EditarMarca(int codigo, MarcaRequest input, CancellationToken ct)
    { var x = await db.marcas.FindAsync([codigo], ct); if (x is null) return NotFound(); x.mar_descripcion = input.Descripcion.Trim(); x.mar_modelo = input.Modelo.Trim(); await db.SaveChangesAsync(ct); return NoContent(); }

    [HttpDelete("marcas/{codigo:int}")]
    public async Task<IActionResult> AnularMarca(int codigo, CancellationToken ct)
    { var x = await db.marcas.FindAsync([codigo], ct); if (x is null) return NotFound(); x.mar_estado = "ANU"; await db.SaveChangesAsync(ct); return NoContent(); }

    [HttpPost("proveedores")]
    public async Task<IActionResult> CrearProveedor(ProveedorRequest input, CancellationToken ct)
    {
        if (await db.proveedores.AnyAsync(x => x.prov_ciruc == input.CiRuc, ct)) return Conflict(new ProblemDetails { Title = "El proveedor ya existe." });
        var x = new proveedore { prov_ciruc=input.CiRuc.Trim(), prov_apellidos=input.Apellidos.Trim(), prov_nombres=input.Nombres.Trim(), prov_razonsocial=input.RazonSocial.Trim(), prov_direccion=input.Direccion.Trim(), prov_telefono=input.Telefono, prov_celular=input.Celular, prov_email=input.Email.Trim(), prov_fpago=input.FormaPago.Trim(), prov_fcreacion=DateTime.Now, prov_fmodif=DateTime.Now, usu_cedula=User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "", prov_observaciones=input.Observaciones, prov_estado="ACT" };
        db.proveedores.Add(x); await db.SaveChangesAsync(ct); return Created($"/api/administracion/proveedores/{x.prov_ciruc}", new { ciRuc=x.prov_ciruc });
    }

    [HttpPut("proveedores/{ciRuc}")]
    public async Task<IActionResult> EditarProveedor(string ciRuc, ProveedorRequest input, CancellationToken ct)
    { if(ciRuc!=input.CiRuc)return BadRequest(new ProblemDetails{Title="El RUC no puede cambiar."}); var x=await db.proveedores.FindAsync([ciRuc],ct);if(x is null)return NotFound();x.prov_apellidos=input.Apellidos.Trim();x.prov_nombres=input.Nombres.Trim();x.prov_razonsocial=input.RazonSocial.Trim();x.prov_direccion=input.Direccion.Trim();x.prov_telefono=input.Telefono;x.prov_celular=input.Celular;x.prov_email=input.Email.Trim();x.prov_fpago=input.FormaPago.Trim();x.prov_observaciones=input.Observaciones;x.prov_fmodif=DateTime.Now;await db.SaveChangesAsync(ct);return NoContent(); }

    [HttpDelete("proveedores/{ciRuc}")]
    public async Task<IActionResult> AnularProveedor(string ciRuc,CancellationToken ct){var x=await db.proveedores.FindAsync([ciRuc],ct);if(x is null)return NotFound();x.prov_estado="ANU";x.prov_fmodif=DateTime.Now;await db.SaveChangesAsync(ct);return NoContent();}

    [HttpPost("productos")]
    public async Task<IActionResult> CrearProducto(ProductoRequest input,CancellationToken ct)
    {
        if(!await db.marcas.AnyAsync(x=>x.mar_codigo==input.MarcaCodigo&&x.mar_estado!="ANU",ct))return BadRequest(new ProblemDetails{Title="La marca no existe o está anulada."});
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var codigo=(await db.productos.MaxAsync(x=>(int?)x.pro_codigo,ct)??0)+1;
        var x=new producto{pro_codigo=codigo,pro_descripcion=input.Descripcion.Trim(),mar_codigo=input.MarcaCodigo,pro_modelo=input.Modelo,pro_imei=input.Imei,pro_obser=input.Observacion,pro_estado="ACT",pro_fechamod=DateTime.Now,pro_precio=input.Precio,pro_unidad=input.Unidad.Trim()};
        db.productos.Add(x);await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);log.LogInformation("Producto {Codigo} creado",codigo);return Created($"/api/administracion/productos/{codigo}",new{codigo});
    }

    [HttpPut("productos/{codigo:int}")]
    public async Task<IActionResult> EditarProducto(int codigo,ProductoRequest input,CancellationToken ct){var x=await db.productos.FindAsync([codigo],ct);if(x is null)return NotFound();x.pro_descripcion=input.Descripcion.Trim();x.mar_codigo=input.MarcaCodigo;x.pro_modelo=input.Modelo;x.pro_imei=input.Imei;x.pro_obser=input.Observacion;x.pro_precio=input.Precio;x.pro_unidad=input.Unidad.Trim();x.pro_fechamod=DateTime.Now;await db.SaveChangesAsync(ct);return NoContent();}

    [HttpDelete("productos/{codigo:int}")]
    public async Task<IActionResult> AnularProducto(int codigo,CancellationToken ct){var x=await db.productos.FindAsync([codigo],ct);if(x is null)return NotFound();x.pro_estado="ANU";x.pro_fechamod=DateTime.Now;await db.SaveChangesAsync(ct);return NoContent();}
}

public sealed record MarcaRequest([Required,StringLength(50)]string Descripcion,[Required,StringLength(50)]string Modelo);
public sealed record ProveedorRequest([Required,StringLength(13,MinimumLength=10)]string CiRuc,[Required,StringLength(50)]string Apellidos,[Required,StringLength(50)]string Nombres,[Required,StringLength(100)]string RazonSocial,[Required,StringLength(200)]string Direccion,[StringLength(25)]string? Telefono,[StringLength(25)]string? Celular,[Required,EmailAddress,StringLength(50)]string Email,[Required,StringLength(30)]string FormaPago,[StringLength(200)]string? Observaciones);
public sealed record ProductoRequest([Required,StringLength(50)]string Descripcion,int MarcaCodigo,[StringLength(30)]string? Modelo,[StringLength(15)]string? Imei,[StringLength(150)]string? Observacion,[Range(0,double.MaxValue)]decimal Precio,[Required,StringLength(10)]string Unidad);
