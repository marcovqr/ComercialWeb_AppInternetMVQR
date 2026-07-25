using System;
using System.Collections.Generic;

namespace Comercial.Infrastructure.Persistence.Scaffolded.Entities;

public partial class Compras_det
{
    public int com_numero { get; set; }

    public int pro_codigp { get; set; }

    public decimal? com_cantidad { get; set; }

    public decimal? com_precio { get; set; }

    public string? com_campo1 { get; set; }

    public string? com_estado { get; set; }
}
