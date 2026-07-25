using System;
using System.Collections.Generic;

namespace Comercial.Infrastructure.Persistence.Scaffolded.Entities;

public partial class Compras_cab
{
    public int com_numero { get; set; }

    public DateTime com_fecha { get; set; }

    public int? com_tipo { get; set; }

    public string pro_ciruc { get; set; } = null!;

    public decimal com_subtotal { get; set; }

    public decimal com_descuento { get; set; }

    public decimal com_impuesto { get; set; }

    public decimal com_total { get; set; }

    public string? com_total_letras { get; set; }

    public string? com_campo1 { get; set; }

    public string? com_campo2 { get; set; }

    public string? com_estado { get; set; }
}
