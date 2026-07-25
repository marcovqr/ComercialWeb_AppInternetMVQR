using System;
using System.Collections.Generic;

namespace Comercial.Infrastructure.Persistence.Scaffolded.Entities;

public partial class factura_cab
{
    public int fac_numero { get; set; }

    public string fac_tipo { get; set; } = null!;

    public string cli_ciruc { get; set; } = null!;

    public DateTime fac_fecha { get; set; }

    public decimal fac_subtotal { get; set; }

    public decimal fac_descuento { get; set; }

    public decimal fac_impuesto { get; set; }

    public decimal fac_total { get; set; }

    public string fac_estado { get; set; } = null!;

    public virtual cliente cli_cirucNavigation { get; set; } = null!;

    public virtual ICollection<factura_det> factura_dets { get; set; } = new List<factura_det>();
}
