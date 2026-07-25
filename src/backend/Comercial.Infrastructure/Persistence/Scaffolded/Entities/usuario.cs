using System;
using System.Collections.Generic;

namespace Comercial.Infrastructure.Persistence.Scaffolded.Entities;

public partial class usuario
{
    public string usu_cedula { get; set; } = null!;

    public string usu_login { get; set; } = null!;

    public string usu_pass { get; set; } = null!;

    public string? usu_obser { get; set; }

    public string usu_estado { get; set; } = null!;
}
