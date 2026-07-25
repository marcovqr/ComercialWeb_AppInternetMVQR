using System;
using System.Collections.Generic;

namespace Comercial.Infrastructure.Persistence.Scaffolded.Entities;

public partial class proveedore
{
    public string prov_ciruc { get; set; } = null!;

    public string prov_apellidos { get; set; } = null!;

    public string prov_nombres { get; set; } = null!;

    public string prov_razonsocial { get; set; } = null!;

    public string prov_direccion { get; set; } = null!;

    public string? prov_telefono { get; set; }

    public string? prov_celular { get; set; }

    public string prov_email { get; set; } = null!;

    public string prov_fpago { get; set; } = null!;

    public DateTime? prov_fcreacion { get; set; }

    public DateTime? prov_fmodif { get; set; }

    public string usu_cedula { get; set; } = null!;

    public string? prov_campo4 { get; set; }

    public string? prov_campo5 { get; set; }

    public string? prov_observaciones { get; set; }

    public string prov_estado { get; set; } = null!;
}
