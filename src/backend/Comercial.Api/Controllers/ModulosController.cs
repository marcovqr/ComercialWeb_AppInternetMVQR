using Comercial.Infrastructure.Persistence.Scaffolded;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Comercial.Api.Controllers;

[Authorize, ApiController, Route("api/modulos")]
public sealed class ModulosController(ScaffoldedComercialContext db) : ControllerBase
{
    [HttpGet("resumen")]
    public async Task<object> Resumen(CancellationToken ct) => new { clientes = await db.clientes.CountAsync(x => x.cli_estado != "ANU", ct), productos = await db.productos.CountAsync(x => x.pro_estado != "ANU", ct), instalaciones = await db.Instalaciones.CountAsync(x => x.ins_estado != "ANU", ct), mensualidadesPendientes = await db.mensualidades.CountAsync(x => x.men_estado == "PENDIENTE", ct) };

    [HttpGet("proveedores")]
    public async Task<object> Proveedores(CancellationToken ct) => await db.proveedores.AsNoTracking().Where(x => x.prov_estado != "ANU").OrderBy(x => x.prov_razonsocial).Take(200).Select(x => new { ciRuc = x.prov_ciruc, razonSocial = x.prov_razonsocial, apellidos = x.prov_apellidos, nombres = x.prov_nombres, direccion = x.prov_direccion, telefono = x.prov_telefono, celular = x.prov_celular, email = x.prov_email, formaPago = x.prov_fpago, observaciones = x.prov_observaciones, estado = x.prov_estado }).ToListAsync(ct);

    [HttpGet("productos")]
    public async Task<object> Productos(CancellationToken ct) => await db.productos.AsNoTracking().Where(x => x.pro_estado != "ANU").OrderBy(x => x.pro_descripcion).Take(200).Select(x => new { codigo = x.pro_codigo, descripcion = x.pro_descripcion, marcaCodigo = x.mar_codigo, modelo = x.pro_modelo, imei = x.pro_imei, precio = x.pro_precio, unidad = x.pro_unidad, estado = x.pro_estado }).ToListAsync(ct);

    [HttpGet("facturas")]
    public async Task<object> Facturas(CancellationToken ct) => await db.factura_cabs.AsNoTracking().OrderByDescending(x => x.fac_fecha).Take(200).Select(x => new { numero = x.fac_numero, tipo = x.fac_tipo, cliente = x.cli_ciruc, fecha = x.fac_fecha, subtotal = x.fac_subtotal, descuento = x.fac_descuento, impuesto = x.fac_impuesto, total = x.fac_total, estado = x.fac_estado }).ToListAsync(ct);

    [HttpGet("compras")]
    public async Task<object> Compras(CancellationToken ct) => await db.Compras_cabs.AsNoTracking().OrderByDescending(x => x.com_fecha).Take(200).Select(x => new { numero = x.com_numero, fecha = x.com_fecha, tipo = x.com_tipo, proveedor = x.pro_ciruc, subtotal = x.com_subtotal, descuento = x.com_descuento, impuesto = x.com_impuesto, total = x.com_total, estado = x.com_estado }).ToListAsync(ct);

    [HttpGet("instalaciones")]
    public async Task<object> Instalaciones(CancellationToken ct) => await db.Instalaciones.AsNoTracking().Where(x => x.ins_estado != "ANU").OrderByDescending(x => x.ins_fecha_instalacion).Take(200).Select(x => new { codigo = x.ins_codigo, clienteCedula = x.cli_cedula, fechaInstalacion = x.ins_fecha_instalacion, costoInstalacion = x.ins_costo_instalacion, valorMensual = x.ins_mensual, descripcionAntena = x.ins_descripcion_antena, ipAntenaWan = x.ins_ip_antena_wan, loginAntena = x.ins_login_antena, ipWanRouter = x.ins_ip_wan_router, loginRouter = x.ins_login_router, ipAccessPoint = x.ins_ip_access_point, estado = x.ins_estado, observaciones = x.ins_observaciones }).ToListAsync(ct);

    [HttpGet("mensualidades")]
    public async Task<object> Mensualidades(CancellationToken ct) => await db.mensualidades.AsNoTracking().Where(x => x.men_estado != "ANU").OrderByDescending(x => x.men_codigo).Take(200).Select(x => new { codigo = x.men_codigo, instalacionCodigo = x.ins_codigo, clienteCedula = x.cli_cedula, desde = x.men_fechadesde, hasta = x.men_fechahasta, cuota = x.men_cuota, factura = x.fac_numero, estado = x.men_estado }).ToListAsync(ct);

    [HttpGet("genera-mensual")]
    public IActionResult GeneraMensual() => Ok(new[]{new{generaMensual=true}});

    [HttpGet("ordenes-trabajo")]
    public IActionResult Ordenes() => StatusCode(StatusCodes.Status501NotImplemented, new ProblemDetails { Title = "Módulo pendiente", Detail = "El respaldo recibido no contiene tablas de órdenes de trabajo. No se crearon tablas artificiales porque la migración es Database First." });

    [HttpGet("pagos")]
    public async Task<object> Pagos(CancellationToken ct)=>await (from f in db.factura_cabs.AsNoTracking() join c in db.clientes.AsNoTracking() on f.cli_ciruc equals c.cli_cedula where f.fac_estado!="ANU" orderby f.fac_fecha descending select new{fechaPago=f.fac_fecha,factura=f.fac_numero,cedula=f.cli_ciruc,cliente=c.cli_nombres+" "+c.cli_apellidos,total=f.fac_total,estado=f.fac_estado,mensualidades=db.mensualidades.Count(m=>m.fac_numero==f.fac_numero)}).Take(200).ToListAsync(ct);

    [HttpGet("inventario")]
    public async Task<object> Inventario(CancellationToken ct)=>await db.productos.AsNoTracking().Where(x=>x.pro_estado!="ANU").OrderBy(x=>x.pro_descripcion).Take(500).Select(x=>new{codigo=x.pro_codigo,descripcion=x.pro_descripcion,modelo=x.pro_modelo,imei=x.pro_imei,unidad=x.pro_unidad,precio=x.pro_precio,saldo=db.saldo_productos.Where(s=>s.pro_codigo==x.pro_codigo&&s.sal_estado!="ANU").OrderByDescending(s=>s.sal_fecha).ThenByDescending(s=>s.sal_id).Select(s=>(decimal?)s.sal_producto).FirstOrDefault()??0,fechaUltimoSaldo=db.saldo_productos.Where(s=>s.pro_codigo==x.pro_codigo&&s.sal_estado!="ANU").OrderByDescending(s=>s.sal_fecha).ThenByDescending(s=>s.sal_id).Select(s=>(DateTime?)s.sal_fecha).FirstOrDefault()}).ToListAsync(ct);

    [HttpGet("pendientes")]
    public async Task<object> Pendientes(CancellationToken ct)=>await (from m in db.mensualidades.AsNoTracking() join c in db.clientes.AsNoTracking() on m.cli_cedula equals c.cli_cedula where m.men_estado=="PENDIENTE"&&m.fac_numero==null orderby m.men_fechadesde select new{codigo=m.men_codigo,instalacion=m.ins_codigo,cedula=m.cli_cedula,cliente=c.cli_nombres+" "+c.cli_apellidos,telefono=c.cli_telefono,desde=m.men_fechadesde,hasta=m.men_fechahasta,cuota=m.men_cuota,estado=m.men_estado}).Take(1000).ToListAsync(ct);
}
