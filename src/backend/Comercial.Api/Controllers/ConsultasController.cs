using Comercial.Infrastructure.Persistence.Scaffolded;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text;

namespace Comercial.Api.Controllers;

[Authorize,ApiController,Route("api/consultas")]
public sealed class ConsultasController(ScaffoldedComercialContext db):ControllerBase
{
    [HttpGet("mensualidades-pendientes")]
    public async Task<IActionResult> MensualidadesPendientes(string? cedula,CancellationToken ct)
    {
        var query=from m in db.mensualidades.AsNoTracking() join c in db.clientes.AsNoTracking() on m.cli_cedula equals c.cli_cedula where m.men_estado=="PENDIENTE"&&m.fac_numero==null select new{m,c};if(!string.IsNullOrWhiteSpace(cedula))query=query.Where(x=>x.m.cli_cedula.Contains(cedula));
        return Ok(await query.OrderBy(x=>x.m.men_fechadesde).Take(1000).Select(x=>new{codigo=x.m.men_codigo,instalacion=x.m.ins_codigo,cedula=x.m.cli_cedula,cliente=x.c.cli_nombres+" "+x.c.cli_apellidos,telefono=x.c.cli_telefono,desde=x.m.men_fechadesde,hasta=x.m.men_fechahasta,cuota=x.m.men_cuota,estado=x.m.men_estado}).ToListAsync(ct));
    }

    [HttpGet("mensualidades-pendientes/imprimir")]
    public async Task<IActionResult> ImprimirPendientes(CancellationToken ct)
    {
        var datos=await (from m in db.mensualidades.AsNoTracking() join c in db.clientes.AsNoTracking() on m.cli_cedula equals c.cli_cedula where m.men_estado=="PENDIENTE"&&m.fac_numero==null orderby c.cli_apellidos,c.cli_nombres,m.men_fechadesde select new{m,c}).Take(2000).ToListAsync(ct);static string H(object? x)=>WebUtility.HtmlEncode(Convert.ToString(x)??"");var rows=new StringBuilder();decimal total=0;foreach(var x in datos){total+=x.m.men_cuota;rows.Append("<tr><td>").Append(H(x.m.cli_cedula)).Append("</td><td>").Append(H(x.c.cli_apellidos)).Append(' ').Append(H(x.c.cli_nombres)).Append("</td><td>").Append(H(x.c.cli_telefono)).Append("</td><td>").Append(x.m.ins_codigo).Append("</td><td>").Append(H(x.m.men_fechadesde)).Append("</td><td>").Append(H(x.m.men_fechahasta)).Append("</td><td class='num'>").Append(x.m.men_cuota.ToString("N2")).Append("</td></tr>");}
        var html="<!doctype html><html lang='es'><head><meta charset='utf-8'><title>Cartera pendiente</title><style>@page{size:A4 landscape;margin:12mm}body{font:12px Arial;color:#18233b}h1{color:#173d75}header{display:flex;justify-content:space-between;border-bottom:3px solid #173d75}table{width:100%;border-collapse:collapse;margin-top:18px}th{background:#173d75;color:white;padding:8px;text-align:left}td{padding:8px;border-bottom:1px solid #dce3ee}.num{text-align:right}.total{font-size:17px;text-align:right;margin-top:15px}.print{position:fixed;right:16px;top:16px;padding:9px;background:#176bdf;color:white;border:0}@media print{.print{display:none}}</style></head><body><button class='print' onclick='window.print()'>Imprimir / Guardar PDF</button><header><div><h1>Mensualidades pendientes</h1><p>Reporte de cartera</p></div><p>Generado: "+DateTime.Now.ToString("dd/MM/yyyy HH:mm")+"</p></header><table><thead><tr><th>Cédula/RUC</th><th>Cliente</th><th>Teléfono</th><th>Instalación</th><th>Desde</th><th>Hasta</th><th>Cuota</th></tr></thead><tbody>"+rows+"</tbody></table><p class='total'>Total pendiente: <b>"+total.ToString("N2")+"</b></p></body></html>";return Content(html,"text/html; charset=utf-8");
    }

    [HttpGet("inventario")]
    public async Task<IActionResult> Inventario(string? buscar,CancellationToken ct)
    {
        var query=db.productos.AsNoTracking().Where(x=>x.pro_estado!="ANU");
        if(!string.IsNullOrWhiteSpace(buscar))query=query.Where(x=>x.pro_descripcion.Contains(buscar)||x.pro_codigo.ToString().Contains(buscar)||x.pro_imei!.Contains(buscar));
        return Ok(await query.OrderBy(x=>x.pro_descripcion).Take(500).Select(x=>new{codigo=x.pro_codigo,descripcion=x.pro_descripcion,modelo=x.pro_modelo,imei=x.pro_imei,unidad=x.pro_unidad,precio=x.pro_precio,saldo=db.saldo_productos.Where(s=>s.pro_codigo==x.pro_codigo&&s.sal_estado!="ANU").OrderByDescending(s=>s.sal_fecha).ThenByDescending(s=>s.sal_id).Select(s=>(decimal?)s.sal_producto).FirstOrDefault()??0,fechaUltimoSaldo=db.saldo_productos.Where(s=>s.pro_codigo==x.pro_codigo&&s.sal_estado!="ANU").OrderByDescending(s=>s.sal_fecha).ThenByDescending(s=>s.sal_id).Select(s=>(DateTime?)s.sal_fecha).FirstOrDefault()}).ToListAsync(ct));
    }

    [HttpGet("inventario/{productoCodigo:int}/movimientos")]
    public async Task<IActionResult> MovimientosProducto(int productoCodigo,DateTime? desde,DateTime? hasta,CancellationToken ct)
    {
        var query=from s in db.saldo_productos.AsNoTracking() join m in db.movimientos.AsNoTracking() on s.mov_id equals m.mov_id where s.pro_codigo==productoCodigo&&s.sal_estado!="ANU" select new{s,m};
        if(desde.HasValue)query=query.Where(x=>x.s.sal_fecha>=desde.Value.Date);if(hasta.HasValue){var limite=hasta.Value.Date.AddDays(1);query=query.Where(x=>x.s.sal_fecha<limite);}
        return Ok(await query.OrderByDescending(x=>x.s.sal_fecha).Select(x=>new{fecha=x.s.sal_fecha,movimiento=x.m.mov_id,nombre=x.m.mov_nombre,cantidad=x.m.mov_cantidad,costo=x.m.mov_costo,saldo=x.s.sal_producto,observaciones=x.m.mov_observaciones,estado=x.m.mov_estado}).Take(500).ToListAsync(ct));
    }

    [HttpGet("pagos")]
    public async Task<IActionResult> Pagos(string? cedula,DateTime? desde,DateTime? hasta,CancellationToken ct)
    {
        var query=from f in db.factura_cabs.AsNoTracking()
                  join c in db.clientes.AsNoTracking() on f.cli_ciruc equals c.cli_cedula
                  where f.fac_estado!="ANU"
                  select new{f,c};
        if(!string.IsNullOrWhiteSpace(cedula))query=query.Where(x=>x.f.cli_ciruc.Contains(cedula));
        if(desde.HasValue)query=query.Where(x=>x.f.fac_fecha>=desde.Value.Date);
        if(hasta.HasValue){var limite=hasta.Value.Date.AddDays(1);query=query.Where(x=>x.f.fac_fecha<limite);}
        var facturas=await query.OrderByDescending(x=>x.f.fac_fecha).ThenByDescending(x=>x.f.fac_numero).Take(500).Select(x=>new{fechaPago=x.f.fac_fecha,factura=x.f.fac_numero,cedula=x.f.cli_ciruc,cliente=x.c.cli_nombres+" "+x.c.cli_apellidos,subtotal=x.f.fac_subtotal,descuento=x.f.fac_descuento,impuesto=x.f.fac_impuesto,total=x.f.fac_total,estado=x.f.fac_estado,mensualidades=db.mensualidades.Count(m=>m.fac_numero==x.f.fac_numero)}).ToListAsync(ct);
        return Ok(facturas);
    }

    [HttpGet("pagos/{factura:int}/mensualidades")]
    public async Task<IActionResult> MensualidadesFactura(int factura,CancellationToken ct)=>Ok(await db.mensualidades.AsNoTracking().Where(x=>x.fac_numero==factura).OrderBy(x=>x.men_fechadesde).Select(x=>new{codigo=x.men_codigo,instalacion=x.ins_codigo,cedula=x.cli_cedula,desde=x.men_fechadesde,hasta=x.men_fechahasta,cuota=x.men_cuota,estado=x.men_estado}).ToListAsync(ct));
}
