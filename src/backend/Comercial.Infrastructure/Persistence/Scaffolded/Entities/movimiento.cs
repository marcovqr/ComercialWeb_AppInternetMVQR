using System;
using System.Collections.Generic;

namespace Comercial.Infrastructure.Persistence.Scaffolded.Entities;

public partial class movimiento
{
    public int mov_id { get; set; }

    public string mov_nombre { get; set; } = null!;

    public DateTime mov_fecha { get; set; }

    public decimal mov_cantidad { get; set; }

    public decimal mov_costo { get; set; }

    public string? mov_campo1 { get; set; }

    public string? mov_campo2 { get; set; }

    public string? mov_campo3 { get; set; }

    public string? mov_observaciones { get; set; }

    public string mov_estado { get; set; } = null!;
}
