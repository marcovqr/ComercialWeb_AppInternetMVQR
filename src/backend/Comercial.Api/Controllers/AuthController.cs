using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Comercial.Infrastructure.Persistence.Scaffolded;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;

namespace Comercial.Api.Controllers;

[ApiController, Route("api/auth")]
public sealed class AuthController(ScaffoldedComercialContext db, ILogger<AuthController> log) : ControllerBase
{
    [AllowAnonymous, EnableRateLimiting("login"), HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest input, CancellationToken ct)
    {
        var user = await db.usuarios.AsNoTracking().SingleOrDefaultAsync(x => x.usu_login == input.Usuario && x.usu_estado != "ANU", ct);
        if (user is null || user.usu_pass != input.Clave)
        {
            log.LogWarning("Intento de acceso fallido para {Usuario}", input.Usuario);
            return Unauthorized(new ProblemDetails { Title = "Usuario o contraseña incorrectos." });
        }
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.usu_cedula), new Claim(ClaimTypes.Name, user.usu_login)], CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        return Ok(new { usuario = user.usu_login, cedula = user.usu_cedula });
    }

    [Authorize, HttpGet("sesion")]
    public IActionResult Session() => Ok(new { usuario = User.Identity?.Name });

    [Authorize, HttpPost("logout")]
    public async Task<IActionResult> Logout() { await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); return NoContent(); }
}

public sealed record LoginRequest([Required, StringLength(10)] string Usuario, [Required, StringLength(20)] string Clave);
