using System;
using System.Collections.Generic;

namespace Comercial.Infrastructure.Persistence.Scaffolded.Entities;

public partial class mensualidade
{
    public int men_codigo { get; set; }

    public int ins_codigo { get; set; }

    public string cli_cedula { get; set; } = null!;

    public string men_fechadesde { get; set; } = null!;

    public string men_fechahasta { get; set; } = null!;

    public decimal men_cuota { get; set; }

    public string men_estado { get; set; } = null!;

    public DateOnly? men_fechamod { get; set; }

    public DateOnly? men_fechacrea { get; set; }

    public int? fac_numero { get; set; }
}
