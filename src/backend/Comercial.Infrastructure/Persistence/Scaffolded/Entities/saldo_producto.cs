using System;
using System.Collections.Generic;

namespace Comercial.Infrastructure.Persistence.Scaffolded.Entities;

public partial class saldo_producto
{
    public int sal_id { get; set; }

    public int pro_codigo { get; set; }

    public DateTime sal_fecha { get; set; }

    public int mov_id { get; set; }

    public decimal sal_producto { get; set; }

    public string sal_estado { get; set; } = null!;
}
