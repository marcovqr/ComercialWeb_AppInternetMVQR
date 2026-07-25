using System;
using System.Collections.Generic;

namespace Comercial.Infrastructure.Persistence.Scaffolded.Entities;

public partial class marca
{
    public int mar_codigo { get; set; }

    public string mar_descripcion { get; set; } = null!;

    public string mar_modelo { get; set; } = null!;

    public string mar_estado { get; set; } = null!;
}
