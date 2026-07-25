# Comercial Web

Aplicación totalmente nueva para migrar el sistema Java Swing a React + ASP.NET Core Web API. El proyecto Java original no fue modificado. SQL Server es la autoridad del esquema: esta solución no contiene migraciones Code First ni ejecuta `Database.Migrate()`.

## Estado

- Respaldo restaurado como `comercial` en `Z3ME-DESARR-L05\SQLEXPRESS` y verificado con `DBCC CHECKDB`.
- Modelo EF Core generado mediante Database First en `Persistence/Scaffolded`.
- API conectada al contexto generado, con autenticación por cookie, CORS, validaciones, Problem Details y logging.
- React incluye autenticación, panel, DataTables, clientes, proveedores, marcas, productos, compras, facturación, pagos, cartera, inventario, instalaciones y mensualidades.
- Están disponibles CRUD y anulaciones lógicas, creación transaccional de compras/facturas, impresión, generación revisada de mensualidades, anulación de factura y liberación para refacturación.
- El módulo de órdenes de trabajo permanece pendiente porque el respaldo no contiene sus tablas. El inventario es de consulta hasta disponer de reglas oficiales de movimiento/saldo.

## Reglas importantes

- `Genera mensual` busca la última mensualidad del cliente y presenta una vista previa de los períodos faltantes antes de escribir en SQL Server.
- La anulación de una factura marca cabecera/detalle como `ANU` y devuelve sus mensualidades a `PENDIENTE`.
- Para Juan Carlos Quito Rivera se usa tarifa base de `$20` y descuento automático de `$5`, con total final de `$15` por mensualidad.
- El producto heredado `326` representa el servicio de Internet residencial en el detalle de factura.

## Ejecución

La forma más sencilla es ejecutar desde la raíz:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\scripts\iniciar-aplicacion.ps1
```

Se abrirán dos ventanas, una para la API y otra para React, y después el navegador.

Ejecución manual:

1. Ajuste `ConnectionStrings:Comercial` en `src/backend/Comercial.Api/appsettings.json` o mediante `ConnectionStrings__Comercial`.
2. Ejecute `dotnet run --project src/backend/Comercial.Api`.
3. En `src/frontend`, copie `.env.example` a `.env`, ejecute `npm install` y `npm run dev`.
4. Ingrese con un usuario activo de la tabla `usuarios`.

Para regenerar el modelo después de cambios realizados directamente en la base, ejecute `scripts/scaffold-database-first.ps1`. Revise el diff antes de integrar; no use `dotnet ef migrations add` ni `database update`.

La guía detallada está en `docs/ANALISIS-Y-MIGRACION.md`.

## Verificación sin modificar datos

Con la API iniciada, ejecute:

```powershell
$clave = Read-Host "Contraseña" -AsSecureString
.\scripts\probar-aplicacion.ps1 -Usuario "SU_USUARIO" -Clave $clave
```

La prueba valida salud, conexión con SQL Server, autenticación y todos los módulos de consulta. No crea, edita ni anula registros.

## OpenAPI

En desarrollo, el contrato OpenAPI está disponible en `http://localhost:5191/openapi/v1.yaml`. Se mantiene como contrato estático versionado porque la descarga de `Microsoft.AspNetCore.OpenApi` fue bloqueada por el proveedor SSL del equipo. No se debilitó TLS para forzar la instalación.

## Publicación local

Para generar una aplicación única que contiene React y ASP.NET Core:

```powershell
.\scripts\publicar-aplicacion.ps1
```

El paquete se crea en `publish\ComercialWeb` y no contiene la contraseña de SQL Server. Antes de iniciarlo, configure la conexión en la misma consola:

```powershell
$env:ConnectionStrings__Comercial = 'Server=SERVIDOR\INSTANCIA;Database=comercial;Trusted_Connection=True;TrustServerCertificate=True'
.\scripts\iniciar-publicado.ps1
```

La aplicación completa se abre en `http://localhost:5191`; en este modo no es necesario ejecutar `npm run dev`. Por seguridad, el script solo permite publicar dentro de `ComercialWebMigration`.

## Abrir con Visual Studio

1. Abra `ComercialWeb.sln` con Visual Studio 2022.
2. Espere a que Visual Studio restaure la solución.
3. En el Explorador de soluciones, establezca `Comercial.Api` como proyecto de inicio.
4. Seleccione el perfil `http` y presione **F5**.

En modo Debug, Visual Studio compila React automáticamente, lo integra con la API y abre `http://localhost:5191`. Se requiere la carga de trabajo **ASP.NET y desarrollo web**, .NET 9 SDK, Node.js y que `npm install` se haya ejecutado al menos una vez en `src\frontend`.
