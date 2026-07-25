# Análisis técnico y migración

## Origen y arquitectura

El sistema inspeccionado es NetBeans/Java 8 Swing con JDBC, formularios, lógica y SQL mezclados, transacciones manuales y reportes Jasper. Se localizaron 12 clases de negocio, 11 formularios y 3 reportes. La aplicación nueva separa React (interfaz), ASP.NET Core Web API (HTTP y seguridad), Application/Domain (casos de uso futuros) e Infrastructure (EF Core Database First).

El proyecto Java ubicado en `C:\Users\marco.quito\Documents\Codex\2026-07-07` se utilizó solo como fuente de análisis y no fue modificado.

## Base restaurada y Database First

`Respaldo_BD_Comercial_19072026.bak` fue restaurado el 19-07-2026 en `Z3ME-DESARR-L05\SQLEXPRESS` como `comercial`. Quedó `ONLINE`, `MULTI_USER`, recuperación `FULL`, y `DBCC CHECKDB` no reportó errores. El scaffolding se ejecutó correctamente y generó 13 entidades y `ScaffoldedComercialContext` dentro de la nueva solución.

Tablas encontradas: `clientes`, `usuarios`, `proveedores`, `marcas`, `productos`, `factura_cab`, `factura_det`, `Compras_cab`, `Compras_det`, `Instalacione`, `mensualidades`, `movimientos` y `saldo_productos`.

Solo existen claves foráneas declaradas para factura→cliente y detalle de factura→factura/producto. Producto→marca, compras→proveedor/producto, instalaciones→cliente y mensualidades→instalación/cliente/factura no están declaradas como FK; esto es un riesgo de integridad heredado. Las fechas de mensualidades y algunas fechas de instalaciones están almacenadas como texto. Los importes de compras usan `decimal(18,0)`, sin centavos.

Para regenerar el modelo después de una modificación controlada del esquema:

```powershell
cd "C:\Users\marco.quito\Documents\Codex\2026-07-19\referenced-chatgpt-conversation-this-is-untrusted\outputs\ComercialWebMigration"
Set-ExecutionPolicy -Scope Process Bypass
.\scripts\scaffold-database-first.ps1
```

No ejecutar `dotnet ef migrations add`, `dotnet ef database update` ni `Database.Migrate()`.

## Mapeo Java → React/.NET

| Java | React/API | Estado |
|---|---|---|
| `frmLogin`, `clsUsuarios` | `/api/auth/login`, sesión y logout | Implementado con cookie segura |
| `frmClientes`, `clsClientes` | `/api/clientes` | Consulta y CRUD con anulación lógica |
| `frmProveedor`, `clsProveedor` | `/api/administracion/proveedores` | Consulta, alta, edición y anulación |
| `frmMarcas`, `frmProductos` | `/api/administracion/marcas|productos` | Consulta, alta, edición y anulación |
| `frmCompras`, `clsCompras` | `/api/documentos/compras` | Consulta y creación transaccional |
| `frmFactura`, `clsFactura` | `/api/documentos/facturas` | Creación, impresión, anulación y refacturación |
| `frmIntalaciones` | `/api/servicios/instalaciones` | Consulta, alta, edición y anulación |
| `frmGeneraMensual`, `clsMensualidades` | `/api/servicios/mensualidades` | Vista previa, generación, historial y mantenimiento |
| `frmOrdenTrabajo` | `/api/modulos/ordenes-trabajo` | Bloqueado: el respaldo no contiene sus tablas |
| Reportes Jasper | HTML imprimible/PDF del navegador | Factura y cartera implementadas |

## Endpoints

- `GET /health` (público)
- `POST /api/auth/login`, `GET /api/auth/sesion`, `POST /api/auth/logout`
- `GET/POST /api/clientes`, `GET/PUT/DELETE /api/clientes/{cedula}`
- `GET /api/modulos/resumen`
- `GET /api/modulos/proveedores|productos|compras|facturas|instalaciones|mensualidades`
- `GET /api/modulos/ordenes-trabajo` devuelve 501 con explicación hasta disponer del esquema
- Documentos: `GET/POST /api/documentos/facturas`, `GET/POST /api/documentos/compras`
- Impresión: `GET /api/documentos/facturas/{numero}/imprimir`
- Pagos: `GET /api/consultas/pagos?cedula=&desde=&hasta=`
- Mensualidades de factura: `GET /api/consultas/pagos/{factura}/mensualidades`
- Cartera: `GET /api/consultas/mensualidades-pendientes`
- Reporte de cartera: `GET /api/consultas/mensualidades-pendientes/imprimir`
- Inventario: `GET /api/consultas/inventario?buscar=`
- Movimientos: `GET /api/consultas/inventario/{producto}/movimientos?desde=&hasta=`
- Servicios: `POST/PUT/DELETE /api/servicios/instalaciones` y `/mensualidades`
- Generación: `POST /api/servicios/mensualidades/previsualizar` y `/generar`
- Historial mensual: `GET /api/servicios/clientes/{cedula}/mensualidades`
- Facturas por cliente: `GET /api/documentos/clientes/{cedula}/facturas`
- Anulación: `POST /api/documentos/facturas/{numero}/anular`

Todos salvo salud y login requieren sesión. CORS admite únicamente los orígenes configurados. Los DTO evitan exponer directamente los nombres heredados; EF parametriza consultas; las excepciones se expresan con Problem Details y las operaciones importantes se registran mediante `ILogger`.

## Reglas preservadas y decisiones

- La eliminación de clientes es lógica con estado `ANU`.
- El identificador del cliente no puede cambiar en una actualización.
- Se validan longitudes, campos obligatorios y formato de correo de acuerdo con el esquema real.
- La base existente controla claves, tipos y relaciones; la aplicación no intenta modificarla.
- El código Java calculaba IVA del 15 %, aunque conserva un comentario del 12 %: debe confirmarse antes de implementar facturación.
- La numeración heredada con `MAX(numero)+1` debe reemplazarse por una estrategia transaccional compatible con el esquema antes de habilitar escrituras.
- Se identificaron referencias Java a `Put_Actualiza_Fecha_Actual_Instalaciones`, `Put_ColocaNumeroFacturaenMensualidad` y `getConsultaClientesPendientesPago`; su existencia y comportamiento deben verificarse directamente en SQL Server antes de reutilizarlos.

## Seguridad, pendientes y riesgos

La tabla heredada `usuarios` guarda contraseñas en texto plano (`varchar(20)`). Para conservar el acceso actual se validan temporalmente sin registrar ni devolver la clave, y la sesión se mantiene en una cookie HttpOnly. Prioridad alta: crear un mecanismo de hash y migración progresiva de contraseñas.

Pendientes principales: tablas o definición oficial de órdenes de trabajo; pruebas de aceptación exhaustivas; confirmación fiscal de IVA, estados y redondeos; datos fiscales/logotipo del emisor; reglas oficiales para escrituras de inventario; y migración de contraseñas heredadas a hash.

### Estado actualizado al 19-07-2026

Ya están implementados los formularios y operaciones de clientes, proveedores, marcas, productos, instalaciones, mensualidades, compras y facturas. Las tablas usan DataTables. Existen consulta de pagos, cartera pendiente, inventario de solo lectura y formatos imprimibles de factura/cartera.

Continúan pendientes antes de producción: confirmar IVA y estados con el responsable del negocio; definir la regla oficial que relaciona compras/facturas con `movimientos` y `saldo_producto`; obtener el esquema faltante de órdenes de trabajo; configurar datos fiscales y logotipo; migrar contraseñas heredadas a hash; ejecutar pruebas de aceptación y preparar despliegue/backup. Existe un contrato OpenAPI estático versionado; la generación dinámica queda pendiente por el bloqueo SSL local de paquetes.

## Cierre funcional

La generación mensual trabaja en dos fases: primero calcula la vista previa sin escribir datos y luego exige confirmación. Toma la mensualidad más reciente del cliente, continúa con su instalación asociada y genera únicamente períodos completos faltantes. Las instalaciones anuladas se excluyen. La anulación de factura se ejecuta en una transacción y libera las mensualidades para refacturación.

Regla específica validada por el usuario: Juan Carlos Quito Rivera utiliza cuota base de 20 dólares aun cuando el dato heredado sea cero, descuento automático de 5 dólares y total final de 15 dólares por mensualidad facturada.

## Seguridad aplicada

- Cookie de sesión `HttpOnly`, `SameSite=Strict`, expiración deslizante de ocho horas y seguridad conforme al protocolo utilizado.
- Límite de cinco intentos de inicio de sesión por IP cada minuto; el exceso responde HTTP 429.
- CORS restringido a los orígenes configurados y con credenciales explícitas.
- Encabezados `nosniff`, `DENY`, política de referente y Content Security Policy.
- Redirección HTTPS habilitada fuera de Development; en desarrollo HTTP no genera advertencias.
- Ejemplo de producción en `appsettings.Production.example.json`, sin credenciales reales.

La deuda crítica sigue siendo `usuarios.usu_pass`, que pertenece al esquema heredado y almacena texto plano. Debe migrarse mediante un cambio coordinado de base y aplicación; no se ejecutará automáticamente mediante Code First.

La base no contiene una entidad independiente de cobros. En la consulta de pagos, la fecha mostrada es `factura_cab.fac_fecha`; una mensualidad se considera registrada/facturada cuando tiene `fac_numero`. No debe interpretarse como conciliación bancaria.
