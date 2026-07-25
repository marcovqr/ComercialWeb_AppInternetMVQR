using System.ComponentModel.DataAnnotations;
using System.Data;
using Comercial.Infrastructure.Persistence.Scaffolded;
using Comercial.Infrastructure.Persistence.Scaffolded.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text;

namespace Comercial.Api.Controllers;

[Authorize, ApiController, Route("api/documentos")]
public sealed class DocumentosController(ScaffoldedComercialContext db, ILogger<DocumentosController> log) : ControllerBase
{
    private const decimal Iva = 0.15m;

    [HttpGet("siguiente-numero/{tipo}")]
    public async Task<IActionResult> SiguienteNumero(string tipo,CancellationToken ct)
    {
        if(tipo.Equals("facturas",StringComparison.OrdinalIgnoreCase))return Ok(new{numero=(await db.factura_cabs.MaxAsync(x=>(int?)x.fac_numero,ct)??0)+1,tipo="FA"});
        if(tipo.Equals("compras",StringComparison.OrdinalIgnoreCase))return Ok(new{numero=(await db.Compras_cabs.MaxAsync(x=>(int?)x.com_numero,ct)??0)+1,tipo="COMPRA"});
        return NotFound();
    }

    [HttpGet("facturas/{numero:int}")]
    public async Task<IActionResult> Factura(int numero,CancellationToken ct)
    {
        var cab=await db.factura_cabs.AsNoTracking().Where(x=>x.fac_numero==numero).Select(x=>new{numero=x.fac_numero,tipo=x.fac_tipo,clienteCedula=x.cli_ciruc,fecha=x.fac_fecha,subtotal=x.fac_subtotal,descuento=x.fac_descuento,impuesto=x.fac_impuesto,total=x.fac_total,estado=x.fac_estado}).SingleOrDefaultAsync(ct);if(cab is null)return NotFound();
        var detalles=await db.factura_dets.AsNoTracking().Where(x=>x.fac_numero==numero).OrderBy(x=>x.fac_linea).Select(x=>new{linea=x.fac_linea,productoCodigo=x.pro_codigo,mensualidadCodigo=x.men_codigo,desde=x.fac_desde,hasta=x.fac_hasta,descripcion=x.pro_codigoNavigation.pro_descripcion,cantidad=x.fac_cantidad,precio=x.fac_precio,total=x.fac_cantidad*x.fac_precio,estado=x.fac_estado}).ToListAsync(ct);return Ok(new{cabecera=cab,detalles});
    }

#if false
    [HttpGet("facturas/{numero:int}/imprimir")]
    public async Task<IActionResult> ImprimirFactura(int numero,CancellationToken ct)
    {
        var f=await db.factura_cabs.AsNoTracking().Include(x=>x.cli_cirucNavigation).SingleOrDefaultAsync(x=>x.fac_numero==numero,ct);if(f is null)return NotFound();
        var detalles=await db.factura_dets.AsNoTracking().Where(x=>x.fac_numero==numero).OrderBy(x=>x.fac_linea).Select(x=>new{x.fac_linea,x.pro_codigo,x.men_codigo,x.fac_desde,x.fac_hasta,x.pro_codigoNavigation.pro_descripcion,x.pro_codigoNavigation.pro_modelo,x.pro_codigoNavigation.pro_unidad,x.fac_cantidad,x.fac_precio}).ToListAsync(ct);
        var mensualidades=await db.mensualidades.AsNoTracking().Where(x=>x.fac_numero==numero).OrderBy(x=>x.men_fechadesde).ThenBy(x=>x.men_codigo).Select(x=>new{x.men_codigo,x.ins_codigo,x.men_fechadesde,x.men_fechahasta,x.men_cuota}).ToListAsync(ct);
        static string H(object? value)=>WebUtility.HtmlEncode(Convert.ToString(value)??"");
        var rows=new StringBuilder();foreach(var d in detalles)rows.Append($"<tr><td>{d.pro_codigo}</td><td>{H(d.pro_descripcion)} {H(d.pro_modelo)}</td><td>{H(d.pro_unidad)}</td><td class='num'>{d.fac_cantidad:N2}</td><td class='num'>{d.fac_precio:N2}</td><td class='num'>{d.fac_cantidad*d.fac_precio:N2}</td></tr>");
        var c=f.cli_cirucNavigation;var html=$$"""
<!doctype html><html lang="es"><head><meta charset="utf-8"><title>Factura {{f.fac_numero}}</title><style>@page{size:A4 landscape;margin:12mm}*{box-sizing:border-box}body{font:13px Arial;color:#15233d;margin:0}.sheet{max-width:1080px;margin:auto}.top{display:flex;justify-content:space-between;border-bottom:3px solid #173d75;padding-bottom:12px}.brand h1{margin:0;color:#173d75}.doc{text-align:right}.doc strong{font-size:27px;color:#173d75}.client{display:grid;grid-template-columns:1fr 1fr;gap:7px 30px;border:1px solid #b9c7dc;border-radius:7px;padding:13px;margin:15px 0}.client b{display:inline-block;min-width:90px}table{width:100%;border-collapse:collapse}th{background:#173d75;color:#fff;padding:9px;text-align:left}td{padding:9px;border-bottom:1px solid #dce3ee}.num{text-align:right}.summary{margin:14px 0 0 auto;width:330px}.summary div{display:flex;justify-content:space-between;padding:6px 10px}.summary .total{background:#173d75;color:#fff;font-size:18px;border-radius:5px}.note{margin-top:28px;color:#62718b;font-size:11px}.print{position:fixed;right:18px;top:18px;padding:10px 18px;background:#176bdf;color:#fff;border:0;border-radius:6px}@media print{.print{display:none}}</style></head><body><button class="print" onclick="window.print()">Imprimir / Guardar PDF</button><main class="sheet"><header class="top"><div class="brand"><h1>SISTEMA COMERCIAL</h1><div>Comprobante de venta</div></div><div class="doc"><div>{{H(f.fac_tipo)}}</div><strong>N.º {{f.fac_numero}}</strong><div>{{f.fac_fecha:dd/MM/yyyy HH:mm}}</div></div></header><section class="client"><div><b>Cédula/RUC:</b>{{H(c.cli_cedula)}}</div><div><b>Cliente:</b>{{H(c.cli_nombres)}} {{H(c.cli_apellidos)}}</div><div><b>Dirección:</b>{{H(c.cli_direccion)}}</div><div><b>Teléfono:</b>{{H(c.cli_telefono)}}</div><div><b>Correo:</b>{{H(c.cli_email)}}</div><div><b>Estado:</b>{{H(f.fac_estado)}}</div></section><table><thead><tr><th>Código</th><th>Descripción</th><th>Unidad</th><th class="num">Cantidad</th><th class="num">P. unitario</th><th class="num">Total</th></tr></thead><tbody>{{rows}}</tbody></table><section class="summary"><div><span>Subtotal</span><b>{{f.fac_subtotal:N2}}</b></div><div><span>Descuento</span><b>{{f.fac_descuento:N2}}</b></div><div><span>IVA</span><b>{{f.fac_impuesto:N2}}</b></div><div class="total"><span>Total</span><b>{{f.fac_total:N2}}</b></div></section><p class="note">Documento generado por Comercial Web. La razón social, RUC, dirección fiscal y logotipo del emisor deben configurarse antes de usarlo como comprobante tributario oficial.</p></main></body></html>
""";
        return Content(html,"text/html; charset=utf-8");
    }

#endif
    [HttpGet("facturas/{numero:int}/imprimir")]
    public async Task<IActionResult> ImprimirFacturaHtml(int numero,CancellationToken ct)
    {
        var f=await db.factura_cabs.AsNoTracking().Include(x=>x.cli_cirucNavigation).SingleOrDefaultAsync(x=>x.fac_numero==numero,ct);if(f is null)return NotFound();
        var detalles=await db.factura_dets.AsNoTracking().Where(x=>x.fac_numero==numero).OrderBy(x=>x.fac_linea).Select(x=>new{x.fac_linea,x.pro_codigo,x.men_codigo,x.fac_desde,x.fac_hasta,x.pro_codigoNavigation.pro_descripcion,x.pro_codigoNavigation.pro_modelo,x.pro_codigoNavigation.pro_unidad,x.fac_cantidad,x.fac_precio}).ToListAsync(ct);
        var mensualidades=await db.mensualidades.AsNoTracking().Where(x=>x.fac_numero==numero).OrderBy(x=>x.men_fechadesde).ThenBy(x=>x.men_codigo).Select(x=>new{x.men_codigo,x.ins_codigo,x.men_fechadesde,x.men_fechahasta,x.men_cuota}).ToListAsync(ct);
        static string H(object? value)=>WebUtility.HtmlEncode(Convert.ToString(value)??"");
        var body=new StringBuilder();body.Append("<!doctype html><html lang='es'><head><meta charset='utf-8'><title>Factura ").Append(f.fac_numero).Append("</title><link rel='stylesheet' href='/factura-print.css'></head><body><button class='print' onclick='window.print()'>Imprimir / Guardar PDF</button><main class='sheet'>");
        body.Append("<header class='top'><div class='brand'><h1>SISTEMA COMERCIAL</h1><div>Comprobante de venta</div></div><div class='doc'><div>").Append(H(f.fac_tipo)).Append("</div><strong>N.º ").Append(f.fac_numero).Append("</strong><div>").Append(f.fac_fecha.ToString("dd/MM/yyyy HH:mm")).Append("</div></div></header>");
        var c=f.cli_cirucNavigation;body.Append("<section class='client'><div><b>Cédula/RUC:</b>").Append(H(c.cli_cedula)).Append("</div><div><b>Cliente:</b>").Append(H(c.cli_nombres)).Append(' ').Append(H(c.cli_apellidos)).Append("</div><div><b>Dirección:</b>").Append(H(c.cli_direccion)).Append("</div><div><b>Teléfono:</b>").Append(H(c.cli_telefono)).Append("</div><div><b>Correo:</b>").Append(H(c.cli_email)).Append("</div><div><b>Estado:</b>").Append(H(f.fac_estado)).Append("</div></section><table><thead><tr><th>Código</th><th>Descripción</th><th>Desde</th><th>Hasta</th><th>Unidad</th><th>Cantidad</th><th>P. unitario</th><th>Total</th></tr></thead><tbody>");
        if(detalles.Any(x=>x.fac_desde!=null&&x.fac_hasta!=null))
        {
            foreach(var d in detalles)body.Append("<tr><td>").Append(d.fac_linea).Append("</td><td>").Append(H(d.pro_descripcion)).Append("</td><td>").Append(d.fac_desde?.ToString("yyyy-MM-dd")).Append("</td><td>").Append(d.fac_hasta?.ToString("yyyy-MM-dd")).Append("</td><td>").Append(H(d.pro_unidad)).Append("</td><td class='num'>").Append(d.fac_cantidad.ToString("N2")).Append("</td><td class='num'>").Append(d.fac_precio.ToString("N2")).Append("</td><td class='num'>").Append((d.fac_cantidad*d.fac_precio).ToString("N2")).Append("</td></tr>");
        }
        else if(mensualidades.Count>0)
        {
            foreach(var m in mensualidades)
            {
                var valorSinIva=decimal.Round(m.men_cuota/(1+Iva),4);
                body.Append("<tr><td>").Append(m.men_codigo).Append("</td><td>Internet residencial · Instalación ").Append(m.ins_codigo).Append("</td><td>").Append(H(m.men_fechadesde)).Append("</td><td>").Append(H(m.men_fechahasta)).Append("</td><td>MES</td><td class='num'>1,00</td><td class='num'>").Append(valorSinIva.ToString("N2")).Append("</td><td class='num'>").Append(valorSinIva.ToString("N2")).Append("</td></tr>");
            }
        }
        else
        {
            foreach(var d in detalles)body.Append("<tr><td>").Append(d.pro_codigo).Append("</td><td>").Append(H(d.pro_descripcion)).Append(' ').Append(H(d.pro_modelo)).Append("</td><td>—</td><td>—</td><td>").Append(H(d.pro_unidad)).Append("</td><td class='num'>").Append(d.fac_cantidad.ToString("N2")).Append("</td><td class='num'>").Append(d.fac_precio.ToString("N2")).Append("</td><td class='num'>").Append((d.fac_cantidad*d.fac_precio).ToString("N2")).Append("</td></tr>");
        }
        body.Append("</tbody></table><section class='summary'><div><span>Subtotal</span><b>").Append(f.fac_subtotal.ToString("N2")).Append("</b></div><div><span>Descuento</span><b>").Append(f.fac_descuento.ToString("N2")).Append("</b></div><div><span>IVA</span><b>").Append(f.fac_impuesto.ToString("N2")).Append("</b></div><div class='total'><span>Total</span><b>").Append(f.fac_total.ToString("N2")).Append("</b></div></section><p class='note'>Configure razón social, RUC, dirección fiscal y logotipo del emisor antes de usarlo como comprobante tributario oficial.</p></main></body></html>");
        return Content(body.ToString(),"text/html; charset=utf-8");
    }

    [AllowAnonymous,HttpGet("/factura-print.css")]
    public IActionResult EstiloFactura()=>Content("@page{size:A4 landscape;margin:12mm}*{box-sizing:border-box}body{font:13px Arial;color:#15233d}.sheet{max-width:1080px;margin:auto}.top{display:flex;justify-content:space-between;border-bottom:3px solid #173d75;padding-bottom:12px}.brand h1{margin:0;color:#173d75}.doc{text-align:right;padding-right:165px}.doc strong{font-size:27px}.client{display:grid;grid-template-columns:1fr 1fr;gap:7px 30px;border:1px solid #b9c7dc;padding:13px;margin:15px 0}table{width:100%;border-collapse:collapse}th{background:#173d75;color:white;padding:9px;text-align:left}td{padding:9px;border-bottom:1px solid #dce3ee}.num{text-align:right}.summary{margin:14px 0 0 auto;width:330px}.summary div{display:flex;justify-content:space-between;padding:6px 10px}.summary .total{background:#173d75;color:white;font-size:18px}.note{margin-top:28px;color:#62718b;font-size:11px}.print{position:fixed;right:18px;top:18px;padding:10px 18px;background:#176bdf;color:white;border:0;border-radius:6px;z-index:10}@media print{.doc{padding-right:0}.print{display:none}}","text/css; charset=utf-8");

    [HttpPost("facturas")]
    public async Task<IActionResult> CrearFactura(FacturaRequest input,CancellationToken ct)
    {
        var cliente=await db.clientes.AsNoTracking().SingleOrDefaultAsync(x=>x.cli_cedula==input.ClienteCedula&&x.cli_estado!="ANU",ct);if(cliente is null)return BadRequest(new ProblemDetails{Title="El cliente no existe o está anulado."});
        if(input.MensualidadCodigos.Count==0)return BadRequest(new ProblemDetails{Title="Seleccione al menos una mensualidad pendiente del cliente."});
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var mensualidades=await db.mensualidades.Where(x=>input.MensualidadCodigos.Contains(x.men_codigo)).ToListAsync(ct);
        if(mensualidades.Count!=input.MensualidadCodigos.Distinct().Count()||mensualidades.Any(x=>x.cli_cedula!=input.ClienteCedula||x.fac_numero!=null||x.men_estado!="PENDIENTE"))return Conflict(new ProblemDetails{Title="Una mensualidad no pertenece al cliente, ya fue facturada o no está pendiente."});
        const int productoMensualidad=326;
        if(!await db.productos.AnyAsync(x=>x.pro_codigo==productoMensualidad&&x.pro_estado!="ANU",ct))return BadRequest(new ProblemDetails{Title="No existe el producto 326 usado por el sistema original para facturar Internet residencial."});
        var numero=(await db.factura_cabs.MaxAsync(x=>(int?)x.fac_numero,ct)??0)+1;
        var nombre=$"{cliente.cli_nombres} {cliente.cli_apellidos}".ToUpperInvariant();var tarifaEspecial=nombre.Contains("JUAN")&&nombre.Contains("CARLOS")&&nombre.Contains("QUITO")&&nombre.Contains("RIVERA");
        if(tarifaEspecial)foreach(var mensualidad in mensualidades.Where(x=>x.men_cuota<=0))mensualidad.men_cuota=20m;
        var totalMensualidades=mensualidades.Sum(x=>x.men_cuota);var descuento=tarifaEspecial?5m:decimal.Round(input.Descuento,4);if(descuento>totalMensualidades)return BadRequest(new ProblemDetails{Title="El descuento no puede superar el valor de las mensualidades."});var subtotal=decimal.Round(totalMensualidades/(1+Iva),4);var baseConDescuento=decimal.Round((totalMensualidades-descuento)/(1+Iva),4);var impuesto=decimal.Round(baseConDescuento*Iva,4);var total=baseConDescuento+impuesto;
        var mensualidadesOrdenadas=mensualidades.OrderBy(x=>x.men_fechadesde).ThenBy(x=>x.men_codigo).ToList();
        var detallesFactura=new List<factura_det>(mensualidadesOrdenadas.Count);
        var subtotalAsignado=0m;
        for(var index=0;index<mensualidadesOrdenadas.Count;index++)
        {
            var m=mensualidadesOrdenadas[index];
            if(!DateOnly.TryParse(m.men_fechadesde,out var desde)||!DateOnly.TryParse(m.men_fechahasta,out var hasta))
                return BadRequest(new ProblemDetails{Title=$"La mensualidad {m.men_codigo} tiene un período inválido."});
            var precio=index==mensualidadesOrdenadas.Count-1
                ? subtotal-subtotalAsignado
                : decimal.Round(m.men_cuota/(1+Iva),4);
            subtotalAsignado+=precio;
            detallesFactura.Add(new factura_det{fac_numero=numero,fac_linea=index+1,pro_codigo=productoMensualidad,men_codigo=m.men_codigo,fac_desde=desde,fac_hasta=hasta,fac_cantidad=1,fac_precio=precio,fac_estado="ACT"});
        }
        db.factura_cabs.Add(new factura_cab{fac_numero=numero,fac_tipo=input.Tipo.Trim(),cli_ciruc=input.ClienteCedula.Trim(),fac_fecha=input.Fecha,fac_subtotal=subtotal,fac_descuento=descuento,fac_impuesto=impuesto,fac_total=total,fac_estado="ACT"});
        db.factura_dets.AddRange(detallesFactura);
        foreach(var m in mensualidades){m.fac_numero=numero;m.men_estado="FACTURADO";m.men_fechamod=DateOnly.FromDateTime(DateTime.Now);}
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);log.LogInformation("Factura {Numero} creada por {Usuario}",numero,User.Identity?.Name);return Created($"/api/documentos/facturas/{numero}",new{numero,subtotal,descuento,impuesto,total});
    }

    [HttpGet("clientes/{cedula}/mensualidades-pendientes")]
    public async Task<IActionResult> MensualidadesPendientes(string cedula,CancellationToken ct)
    {
        var cliente=await db.clientes.AsNoTracking().SingleOrDefaultAsync(x=>x.cli_cedula==cedula,ct);var nombre=cliente is null?"":$"{cliente.cli_nombres} {cliente.cli_apellidos}".ToUpperInvariant();var tarifaEspecial=nombre.Contains("JUAN")&&nombre.Contains("CARLOS")&&nombre.Contains("QUITO")&&nombre.Contains("RIVERA");
        var rows=await db.mensualidades.AsNoTracking().Where(x=>x.cli_cedula==cedula&&x.men_estado=="PENDIENTE"&&x.fac_numero==null).OrderBy(x=>x.men_fechadesde).Select(x=>new{codigo=x.men_codigo,instalacionCodigo=x.ins_codigo,desde=x.men_fechadesde,hasta=x.men_fechahasta,cuota=tarifaEspecial&&x.men_cuota<=0?20m:x.men_cuota}).ToListAsync(ct);
        return Ok(rows);
    }

    [HttpGet("clientes/{cedula}/facturas")]
    public async Task<IActionResult> FacturasCliente(string cedula,CancellationToken ct)
    {
        var rows=await db.factura_cabs.AsNoTracking().Where(x=>x.cli_ciruc==cedula).OrderByDescending(x=>x.fac_fecha).ThenByDescending(x=>x.fac_numero).Select(x=>new{numero=x.fac_numero,tipo=x.fac_tipo,fecha=x.fac_fecha,subtotal=x.fac_subtotal,descuento=x.fac_descuento,impuesto=x.fac_impuesto,total=x.fac_total,estado=x.fac_estado}).ToListAsync(ct);
        return Ok(rows);
    }

    [HttpPost("facturas/{numero:int}/anular")]
    public async Task<IActionResult> AnularFactura(int numero,CancellationToken ct)
    {
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var factura=await db.factura_cabs.SingleOrDefaultAsync(x=>x.fac_numero==numero,ct);
        if(factura is null)return NotFound(new ProblemDetails{Title="La factura no existe."});
        if(factura.fac_estado=="ANU")return Conflict(new ProblemDetails{Title="La factura ya está anulada."});
        factura.fac_estado="ANU";
        var detalles=await db.factura_dets.Where(x=>x.fac_numero==numero).ToListAsync(ct);
        foreach(var detalle in detalles)detalle.fac_estado="ANU";
        var mensualidades=await db.mensualidades.Where(x=>x.fac_numero==numero).ToListAsync(ct);
        foreach(var mensualidad in mensualidades){mensualidad.fac_numero=null;mensualidad.men_estado="PENDIENTE";mensualidad.men_fechamod=DateOnly.FromDateTime(DateTime.Now);}
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
        log.LogWarning("Factura {Numero} anulada por {Usuario}; {Cantidad} mensualidades liberadas",numero,User.Identity?.Name,mensualidades.Count);
        return Ok(new{numero,mensualidadesLiberadas=mensualidades.Count,mensaje="Factura anulada. Las mensualidades están disponibles para volver a facturar."});
    }

    [HttpGet("compras/{numero:int}")]
    public async Task<IActionResult> Compra(int numero,CancellationToken ct){var cab=await db.Compras_cabs.AsNoTracking().SingleOrDefaultAsync(x=>x.com_numero==numero,ct);if(cab is null)return NotFound();var detalles=await db.Compras_dets.AsNoTracking().Where(x=>x.com_numero==numero).Select(x=>new{productoCodigo=x.pro_codigp,cantidad=x.com_cantidad,precio=x.com_precio,estado=x.com_estado}).ToListAsync(ct);return Ok(new{cabecera=cab,detalles});}

    [HttpPost("compras")]
    public async Task<IActionResult> CrearCompra(CompraRequest input,CancellationToken ct)
    {
        if(input.Detalles.Count==0)return BadRequest(new ProblemDetails{Title="La compra debe contener al menos un detalle."});if(input.Detalles.GroupBy(x=>x.ProductoCodigo).Any(x=>x.Count()>1))return BadRequest(new ProblemDetails{Title="No repita un producto; acumule su cantidad."});if(!await db.proveedores.AnyAsync(x=>x.prov_ciruc==input.ProveedorCiRuc&&x.prov_estado!="ANU",ct))return BadRequest(new ProblemDetails{Title="El proveedor no existe o está anulado."});
        var codigos=input.Detalles.Select(x=>x.ProductoCodigo).ToList();if(await db.productos.CountAsync(x=>codigos.Contains(x.pro_codigo)&&x.pro_estado!="ANU",ct)!=codigos.Count)return BadRequest(new ProblemDetails{Title="Uno o más productos no existen o están anulados."});
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);var numero=(await db.Compras_cabs.MaxAsync(x=>(int?)x.com_numero,ct)??0)+1;var subtotal=input.Detalles.Sum(x=>decimal.Round(x.Cantidad*x.Precio,0));var descuento=decimal.Round(input.Descuento,0);if(descuento>subtotal)return BadRequest(new ProblemDetails{Title="El descuento no puede superar el subtotal."});var impuesto=decimal.Round((subtotal-descuento)*Iva,0);var total=subtotal-descuento+impuesto;
        db.Compras_cabs.Add(new Compras_cab{com_numero=numero,com_fecha=input.Fecha,com_tipo=input.Tipo,pro_ciruc=input.ProveedorCiRuc.Trim(),com_subtotal=subtotal,com_descuento=descuento,com_impuesto=impuesto,com_total=total,com_estado="ACT"});db.Compras_dets.AddRange(input.Detalles.Select(x=>new Compras_det{com_numero=numero,pro_codigp=x.ProductoCodigo,com_cantidad=decimal.Round(x.Cantidad,0),com_precio=decimal.Round(x.Precio,0),com_estado="ACT"}));await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);log.LogInformation("Compra {Numero} creada por {Usuario}",numero,User.Identity?.Name);return Created($"/api/documentos/compras/{numero}",new{numero,subtotal,descuento,impuesto,total,advertencia="La base almacena importes de compra sin decimales."});
    }
}

public sealed record DocumentoDetalleRequest(int ProductoCodigo,[Range(typeof(decimal),"0.0001","999999999")]decimal Cantidad,[Range(typeof(decimal),"0","999999999")]decimal Precio);
public sealed record FacturaRequest([Required,StringLength(2)]string Tipo,[Required,StringLength(13)]string ClienteCedula,DateTime Fecha,[Range(typeof(decimal),"0","999999999")]decimal Descuento,List<DocumentoDetalleRequest> Detalles,List<int> MensualidadCodigos);
public sealed record CompraRequest(int? Tipo,[Required,StringLength(13)]string ProveedorCiRuc,DateTime Fecha,[Range(typeof(decimal),"0","999999999")]decimal Descuento,List<DocumentoDetalleRequest> Detalles);
