using System;
using System.Collections.Generic;

namespace Comercial.Infrastructure.Persistence.Scaffolded.Entities;

public partial class factura_det
{
    public int fac_numero { get; set; }

    public int pro_codigo { get; set; }

    public decimal fac_cantidad { get; set; }

    public decimal fac_precio { get; set; }

    public string fac_estado { get; set; } = null!;

    public virtual factura_cab fac_numeroNavigation { get; set; } = null!;

    public virtual producto pro_codigoNavigation { get; set; } = null!;
}
