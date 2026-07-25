using System;
using System.Collections.Generic;

namespace Comercial.Infrastructure.Persistence.Scaffolded.Entities;

public partial class cliente
{
    public string cli_cedula { get; set; } = null!;

    public string cli_apellidos { get; set; } = null!;

    public string cli_nombres { get; set; } = null!;

    public string cli_direccion { get; set; } = null!;

    public string? cli_telefono { get; set; }

    public string? cli_email { get; set; }

    public string? cli_obser { get; set; }

    public string cli_estado { get; set; } = null!;

    public DateTime cli_fechamod { get; set; }

    public virtual ICollection<factura_cab> factura_cabs { get; set; } = new List<factura_cab>();
}
