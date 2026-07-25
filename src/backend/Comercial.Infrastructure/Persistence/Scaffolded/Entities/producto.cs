using System;
using System.Collections.Generic;

namespace Comercial.Infrastructure.Persistence.Scaffolded.Entities;

public partial class producto
{
    public int pro_codigo { get; set; }

    public string pro_descripcion { get; set; } = null!;

    public int mar_codigo { get; set; }

    public string? pro_modelo { get; set; }

    public string? pro_imei { get; set; }

    public string? pro_obser { get; set; }

    public string pro_estado { get; set; } = null!;

    public DateTime pro_fechamod { get; set; }

    public decimal pro_precio { get; set; }

    public string pro_unidad { get; set; } = null!;

    public virtual ICollection<factura_det> factura_dets { get; set; } = new List<factura_det>();
}
