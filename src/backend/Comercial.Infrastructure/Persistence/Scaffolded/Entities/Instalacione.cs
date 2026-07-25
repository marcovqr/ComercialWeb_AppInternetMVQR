using System;
using System.Collections.Generic;

namespace Comercial.Infrastructure.Persistence.Scaffolded.Entities;

public partial class Instalacione
{
    public int ins_codigo { get; set; }

    public DateTime? ins_fecha_instalacion { get; set; }

    public string? cli_cedula { get; set; }

    public double? ins_costo_instalacion { get; set; }

    public double? ins_mensual { get; set; }

    public DateTime? ins_fecha_actual { get; set; }

    public double? ins_dias_servicio { get; set; }

    public string? ins_password_wifi { get; set; }

    public string? ins_ip_antena_wan { get; set; }

    public string? ins_login_antena { get; set; }

    public string? ins_password_antena { get; set; }

    public string? ins_ip_wan_router { get; set; }

    public string? ins_login_router { get; set; }

    public string? ins_password_router { get; set; }

    public string? ins_ip_access_point { get; set; }

    public string? ins_estado { get; set; }

    public string? ins_descripcion_antena { get; set; }

    public string? ins_observaciones { get; set; }

    public string? ins_fecha_creacion { get; set; }

    public string? ins_fecha_mod { get; set; }
}
