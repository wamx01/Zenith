using MundoVs.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using MundoVs.Infrastructure.Data;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Services;
using MundoVs.Core.Entities;
using MundoVs.Core.Entities.Auth;
using MundoVs.Core.Models;
using MundoVs.Infrastructure.Repositories;
using MundoVs.Core.Security;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.FileProviders;
using QuestPDF.Infrastructure;
using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Zenith.Contracts.Asistencia;

var builder = WebApplication.CreateBuilder(args);
var defaultCulture = new CultureInfo("es-MX");
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

var configuredPort = builder.Configuration["PORT"];
if (int.TryParse(configuredPort, out var port) && port > 0)
{
    builder.WebHost.UseUrls($"http://+:{port}");
}
else
{
    var configuredUrls = builder.Configuration["URLS"]
        ?? builder.Configuration["ASPNETCORE_URLS"]
        ?? builder.Configuration["Hosting:Urls"];
    if (!string.IsNullOrWhiteSpace(configuredUrls))
    {
        builder.WebHost.UseUrls(configuredUrls);
    }
}

var cookieSecurePolicy = ParseCookieSecurePolicy(builder.Configuration["Auth:CookieSecurePolicy"]);
var cookieSameSite = ParseSameSiteMode(builder.Configuration["Auth:SameSite"]);
var useHttpsRedirection = builder.Configuration.GetValue("Auth:UseHttpsRedirection", true);
var applyMigrationsOnStartup = builder.Configuration.GetValue("Database:ApplyMigrationsOnStartup", false);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configurar MariaDB con Pomelo
var connectionString = GetRequiredConnectionString(builder.Configuration);
var serverVersion = new MariaDbServerVersion(new Version(10, 11));

builder.Services.AddDbContext<CrmDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

// Factory para contextos independientes (evita concurrencia en Blazor Server)
builder.Services.AddDbContextFactory<CrmDbContext>(options =>
    options.UseMySql(connectionString, serverVersion), ServiceLifetime.Scoped);

// Registrar repositorios
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IPedidoSerigrafiaRepository, PedidoSerigrafiaRepository>();
builder.Services.AddScoped<IProductoClienteRepository, ProductoClienteRepository>();
builder.Services.AddScoped<IPedidoSeguimientoRepository, PedidoSeguimientoRepository>();
builder.Services.AddScoped<IPresupuestoProductoRepository, PresupuestoProductoRepository>();
builder.Services.AddScoped<IAppConfigRepository, AppConfigRepository>();
builder.Services.AddScoped<INominaCalculator, NominaCalculator>();
builder.Services.AddScoped<INominaLegalPolicyService, NominaLegalPolicyService>();
builder.Services.AddScoped<INominaPdfService, NominaPdfService>();
builder.Services.AddScoped<INominaReciboBuilder, NominaReciboBuilder>();
builder.Services.AddScoped<INominaResumenBuilder, NominaResumenBuilder>();
builder.Services.AddScoped<INominaSatCatalogInitializer, NominaSatCatalogInitializer>();
builder.Services.AddScoped<IRrhhAsistenciaProcessor, RrhhAsistenciaProcessor>();
builder.Services.AddScoped<IRrhhAsistenciaCorreccionAdvisor, RrhhAsistenciaCorreccionAdvisor>();
builder.Services.AddScoped<IRrhhTiempoExtraResolutionService, RrhhTiempoExtraResolutionService>();
builder.Services.AddScoped<IRrhhPrenominaSnapshotService, RrhhPrenominaSnapshotService>();
builder.Services.AddScoped<IRrhhMarcacionIngestionService, RrhhMarcacionIngestionService>();
builder.Services.AddScoped<IRrhhMarcacionZonaHorariaService, RrhhMarcacionZonaHorariaService>();
builder.Services.AddScoped<CodigoNegocioService>();
builder.Services.AddScoped<INotaEntregaPdfService, NotaEntregaPdfService>();
builder.Services.AddScoped<ModuloStateService>();
builder.Services.AddMemoryCache();

builder.Services.AddScoped<IEmpresaContext, EmpresaContext>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IModuloAccesoService, ModuloAccesoService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<ITenantFileStorageService, TenantFileStorageService>();
builder.Services.AddHttpClient<IFacturacionFiscalService, FacturapiFacturacionFiscalService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestMethod
        | HttpLoggingFields.RequestPath
        | HttpLoggingFields.ResponseStatusCode
        | HttpLoggingFields.Duration;
});
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "mundovs.auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = cookieSameSite;
        options.Cookie.SecurePolicy = cookieSecurePolicy;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                if (IsJsonEndpoint(context.Request.Path))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = context =>
            {
                if (IsJsonEndpoint(context.Request.Path))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(defaultCulture);
    options.SupportedCultures = [defaultCulture];
    options.SupportedUICultures = [defaultCulture];
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Auth: registrar DESPUÉS de AddRazorComponents para sobreescribir ServerAuthenticationStateProvider
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());

var app = builder.Build();
QuestPDF.Settings.License = LicenseType.Community;

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

    if (applyMigrationsOnStartup)
    {
        await db.Database.MigrateAsync();
    }
    else
    {
        logger.LogInformation("EF Core migrations on startup are disabled by configuration.");
    }

    var satCatalogInitializer = scope.ServiceProvider.GetRequiredService<INominaSatCatalogInitializer>();
    await satCatalogInitializer.SeedAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseRequestLocalization();

app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        var headers = context.Response.Headers;
        headers.TryAdd(HeaderNames.XContentTypeOptions, "nosniff");
        headers.TryAdd(HeaderNames.XFrameOptions, "DENY");
        headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
        headers.TryAdd("Permissions-Policy", "camera=(), microphone=(), geolocation=()");

        if (!app.Environment.IsDevelopment())
        {
            headers.TryAdd(HeaderNames.ContentSecurityPolicy,
                "default-src 'self'; img-src 'self' data:; style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; font-src 'self' https://cdn.jsdelivr.net data:; script-src 'self' 'unsafe-inline'; connect-src 'self' https: wss:; frame-ancestors 'none'; base-uri 'self'; form-action 'self'");
        }

        return Task.CompletedTask;
    });

    await next();
});

app.UseHttpLogging();
if (useHttpsRedirection)
{
    app.UseHttpsRedirection();
}
var uploadsRoot = Path.Combine(app.Environment.WebRootPath, "uploads");
Directory.CreateDirectory(uploadsRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsRoot),
    RequestPath = "/uploads"
});
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapGet("/tenant-files/{empresaId:guid}/{**storagePath}", async (Guid empresaId, string storagePath, ITenantFileStorageService tenantFileStorage, CancellationToken cancellationToken) =>
{
    var file = await tenantFileStorage.OpenReadAsync(empresaId, storagePath, cancellationToken);
    return file is null
        ? Results.NotFound()
        : Results.File(file.Content, file.ContentType, enableRangeProcessing: true);
}).RequireAuthorization();

app.MapPost("/auth/session/login", async (HttpContext context, IAuthService authService, IModuloAccesoService moduloAccesoService, AuthLoginRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        return Results.BadRequest(new AuthSessionResponse(false, "Captura correo electrónico y contraseña."));

    var usuario = await authService.LoginAsync(request.Email.Trim(), request.Password);
    if (usuario == null)
        return Results.BadRequest(new AuthSessionResponse(false, "Credenciales incorrectas, usuario inactivo o empresa sin acceso vigente."));

    var principal = await CreatePrincipalAsync(usuario, authService, moduloAccesoService);
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, CreateAuthenticationProperties());

    return Results.Ok(new AuthSessionResponse(true, RedirectUrl: usuario.RequiereCambioPassword ? "/auth/cambiar-password-inicial" : "/"));
}).AllowAnonymous();

app.MapPost("/auth/session/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok(new AuthSessionResponse(true));
}).AllowAnonymous();

app.MapPost("/auth/session/refresh", async (HttpContext context, IAuthService authService, IModuloAccesoService moduloAccesoService, IDbContextFactory<CrmDbContext> dbFactory) =>
{
    var userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!Guid.TryParse(userIdValue, out var userId))
        return Results.Unauthorized();

    await using var db = await dbFactory.CreateDbContextAsync();
    var usuario = await db.Usuarios
        .IgnoreQueryFilters()
        .Include(u => u.TipoUsuario)
        .Include(u => u.Empresa)
        .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

    if (usuario == null)
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.Unauthorized();
    }

    var principal = await CreatePrincipalAsync(usuario, authService, moduloAccesoService);
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, CreateAuthenticationProperties());

    return Results.Ok(new AuthSessionResponse(true));
}).RequireAuthorization();

app.MapPost("/api/rrhh/marcaciones/sync", async (HttpContext httpContext, CrmDbContext db, IConfiguration configuration, IRrhhAsistenciaProcessor rrhhAsistenciaProcessor, IRrhhMarcacionIngestionService rrhhMarcacionIngestionService, MarcacionSyncBatchDto batch) =>
{
    if (!TryAuthenticateAgentRequest(httpContext, configuration, batch.EmpresaId, null, out _))
    {
        return Results.Unauthorized();
    }

    var ingestionResult = await rrhhMarcacionIngestionService.IngerirLoteAsync(db, batch, httpContext.RequestAborted);

    if (ingestionResult.IsSuccess)
    {
        await rrhhAsistenciaProcessor.ProcesarMarcacionesPendientesAsync(db, batch.EmpresaId, batch.ChecadorId, httpContext.RequestAborted);
    }

    return ingestionResult.StatusCode switch
    {
        StatusCodes.Status400BadRequest => Results.BadRequest(ingestionResult.Response),
        StatusCodes.Status404NotFound => Results.NotFound(ingestionResult.Response),
        _ => Results.Ok(ingestionResult.Response)
    };
}).AllowAnonymous();

app.MapPost("/api/rrhh/asistencias/reprocesar", async (HttpContext httpContext, CrmDbContext db, IRrhhAsistenciaProcessor rrhhAsistenciaProcessor, RrhhAsistenciaReprocesoRequest request) =>
{
    if (request.EmpresaId == Guid.Empty)
    {
        return Results.BadRequest("EmpresaId es requerido.");
    }

    if (!(httpContext.User.Identity?.IsAuthenticated ?? false))
    {
        return Results.Unauthorized();
    }

    var puedeEditar = httpContext.User.HasClaim("Capacidad", "empleados.editar")
        || httpContext.User.HasClaim("Capacidad", "nominas.editar");

    if (!puedeEditar)
    {
        return Results.Forbid();
    }

    if (request.FechaHasta < request.FechaDesde)
    {
        return Results.BadRequest("La fecha final no puede ser menor a la fecha inicial.");
    }

    var rangoDias = request.FechaHasta.DayNumber - request.FechaDesde.DayNumber + 1;
    var maxDiasReproceso = Math.Max(1, httpContext.RequestServices.GetRequiredService<IConfiguration>().GetValue<int?>("Asistencia:ReprocesoMaxDays") ?? 31);
    if (rangoDias > maxDiasReproceso)
    {
        return Results.BadRequest($"El rango máximo permitido para reproceso es de {maxDiasReproceso} día(s).");
    }

    var grupos = await rrhhAsistenciaProcessor.ReprocesarRangoAsync(
        db,
        request.EmpresaId,
        request.FechaDesde,
        request.FechaHasta,
        request.EmpleadoId,
        httpContext.RequestAborted);

    db.RrhhLogsChecador.Add(new RrhhLogChecador
    {
        Id = Guid.NewGuid(),
        EmpresaId = request.EmpresaId,
        FechaUtc = DateTime.UtcNow,
        Nivel = "Information",
        Mensaje = "Se ejecutó un reproceso histórico de asistencias.",
        Detalle = $"usuario={httpContext.User.Identity?.Name ?? "desconocido"};desde={request.FechaDesde:yyyy-MM-dd};hasta={request.FechaHasta:yyyy-MM-dd};empleado={request.EmpleadoId};grupos={grupos};motivo={LimitarTexto(request.Motivo, 300, "sin motivo")}",
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    });

    await db.SaveChangesAsync(httpContext.RequestAborted);

    return Results.Ok(new
    {
        EmpresaId = request.EmpresaId,
        request.FechaDesde,
        request.FechaHasta,
        request.EmpleadoId,
        GruposProcesados = grupos,
        Mensaje = "Reproceso de asistencias completado."
    });
});

app.MapGet("/api/rrhh/agentes/configuracion", async (HttpContext httpContext, CrmDbContext db, IConfiguration configuration, Guid empresaId, string? nombreAgente) =>
{
    if (empresaId == Guid.Empty)
    {
        return Results.BadRequest("EmpresaId es requerido.");
    }

    if (!TryAuthenticateAgentRequest(httpContext, configuration, empresaId, nombreAgente, out var agentConfig))
    {
        return Results.Unauthorized();
    }

    var checadores = await db.RrhhChecadores
        .AsNoTracking()
        .Where(c => c.EmpresaId == empresaId && c.IsActive)
        .OrderBy(c => c.Nombre)
        .Select(c => new ChecadorConfigDto
        {
            Id = c.Id,
            EmpresaId = c.EmpresaId,
            Nombre = c.Nombre,
            NumeroSerie = c.NumeroSerie,
            Ip = c.Ip,
            Puerto = c.Puerto,
            NumeroMaquina = c.NumeroMaquina,
            Ubicacion = c.Ubicacion,
            ZonaHoraria = c.ZonaHoraria,
            UltimaSincronizacionUtc = c.UltimaSincronizacionUtc,
            UltimoEventoLeido = c.UltimoEventoLeido,
            Activo = c.IsActive
        })
        .ToListAsync();

    var intervaloSegundos = Math.Max(5, agentConfig?.IntervaloSegundos ?? configuration.GetValue<int?>("Asistencia:IntervaloSegundos") ?? 60);

    return Results.Ok(new AgenteConfiguracionDto
    {
        EmpresaId = empresaId,
        NombreAgente = string.IsNullOrWhiteSpace(nombreAgente) ? "Zenith Asistencia Worker" : nombreAgente.Trim(),
        IntervaloSegundos = intervaloSegundos,
        PermitirLecturaUsuarios = agentConfig?.PermitirLecturaUsuarios ?? false,
        ModoDiagnostico = agentConfig?.ModoDiagnostico ?? false,
        Checadores = checadores
    });
}).AllowAnonymous();

app.MapPost("/api/rrhh/agentes/heartbeat", async (HttpContext httpContext, CrmDbContext db, IConfiguration configuration, AgenteHeartbeatDto heartbeat) =>
{
    if (heartbeat.EmpresaId == Guid.Empty)
    {
        return Results.BadRequest("EmpresaId es requerido.");
    }

    if (!TryAuthenticateAgentRequest(httpContext, configuration, heartbeat.EmpresaId, heartbeat.NombreAgente, out _))
    {
        return Results.Unauthorized();
    }

    var estadoAgente = await UpsertEstadoAgenteAsync(db, heartbeat.EmpresaId, heartbeat.NombreAgente, httpContext.RequestAborted);
    estadoAgente.Hostname = LimitarTexto(heartbeat.Hostname, 120);
    estadoAgente.Version = LimitarTexto(heartbeat.Version, 40);
    estadoAgente.UltimoHeartbeatUtc = DateTime.UtcNow;
    estadoAgente.UltimaEjecucionUtc = heartbeat.UltimaEjecucionUtc;
    estadoAgente.MarcacionesLeidas = heartbeat.MarcacionesLeidas;
    estadoAgente.MarcacionesEnviadas = heartbeat.MarcacionesEnviadas;
    estadoAgente.UltimoError = LimitarTexto(heartbeat.ErroresRecientes, 500);
    estadoAgente.UpdatedAt = DateTime.UtcNow;

    db.RrhhLogsChecador.Add(new RrhhLogChecador
    {
        Id = Guid.NewGuid(),
        EmpresaId = heartbeat.EmpresaId,
        FechaUtc = DateTime.UtcNow,
        Nivel = "Information",
        Mensaje = $"Heartbeat recibido de {heartbeat.NombreAgente}.",
        Detalle = $"host={heartbeat.Hostname};version={heartbeat.Version};ultima={heartbeat.UltimaEjecucionUtc:O};leidas={heartbeat.MarcacionesLeidas};enviadas={heartbeat.MarcacionesEnviadas};errores={heartbeat.ErroresRecientes}",
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    });

    await db.SaveChangesAsync();
    return Results.Ok();
}).AllowAnonymous();

app.MapPost("/api/rrhh/agentes/logs", async (HttpContext httpContext, CrmDbContext db, IConfiguration configuration, AgenteLogDto logEntry) =>
{
    if (logEntry.EmpresaId == Guid.Empty)
    {
        return Results.BadRequest("EmpresaId es requerido.");
    }

    if (!TryAuthenticateAgentRequest(httpContext, configuration, logEntry.EmpresaId, logEntry.NombreAgente, out _))
    {
        return Results.Unauthorized();
    }

    var estadoAgente = await UpsertEstadoAgenteAsync(db, logEntry.EmpresaId, logEntry.NombreAgente, httpContext.RequestAborted);
    estadoAgente.Hostname = LimitarTexto(logEntry.Hostname, 120) ?? estadoAgente.Hostname;
    estadoAgente.UltimoLogNivel = LimitarTexto(logEntry.Nivel, 20, "Information");
    estadoAgente.UltimoLogMensaje = LimitarTexto(logEntry.Mensaje, 500, "Log de agente");
    estadoAgente.UltimoLogDetalle = LimitarTexto(logEntry.Detalle, 2000);
    estadoAgente.UltimoLogUtc = logEntry.FechaUtc == default ? DateTime.UtcNow : logEntry.FechaUtc;
    if (string.Equals(logEntry.Nivel, "Error", StringComparison.OrdinalIgnoreCase))
    {
        estadoAgente.UltimoError = estadoAgente.UltimoLogMensaje;
    }

    estadoAgente.UpdatedAt = DateTime.UtcNow;

    if (logEntry.ChecadorId.HasValue)
    {
        var checadorValido = await db.RrhhChecadores.AsNoTracking()
            .AnyAsync(c => c.Id == logEntry.ChecadorId.Value && c.EmpresaId == logEntry.EmpresaId, httpContext.RequestAborted);

        if (!checadorValido)
        {
            return Results.BadRequest("El checador indicado no pertenece a la empresa.");
        }
    }

    db.RrhhLogsChecador.Add(new RrhhLogChecador
    {
        Id = Guid.NewGuid(),
        EmpresaId = logEntry.EmpresaId,
        ChecadorId = logEntry.ChecadorId,
        FechaUtc = logEntry.FechaUtc == default ? DateTime.UtcNow : logEntry.FechaUtc,
        Nivel = LimitarTexto(logEntry.Nivel, 20, "Information"),
        Mensaje = LimitarTexto(logEntry.Mensaje, 500, "Log de agente"),
        Detalle = LimitarTexto(logEntry.Detalle, 2000),
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    });

    await db.SaveChangesAsync(httpContext.RequestAborted);
    return Results.Ok();
}).AllowAnonymous();

app.MapGet("/api/nomina/recibo/{detalleId:guid}/pdf", async (HttpContext httpContext, Guid detalleId, IDbContextFactory<CrmDbContext> dbFactory, INominaPdfService nominaPdfService, CancellationToken cancellationToken) =>
{
    if (!(httpContext.User.Identity?.IsAuthenticated ?? false))
        return Results.Unauthorized();
    if (!httpContext.User.HasClaim("Capacidad", "nominas.ver"))
        return Results.Forbid();
    if (!Guid.TryParse(httpContext.User.FindFirst("EmpresaId")?.Value, out var empresaIdClaim) || empresaIdClaim == Guid.Empty)
        return Results.Forbid();

    await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
    var empresaIdDetalle = await db.NominaDetalles
        .AsNoTracking()
        .Where(d => d.Id == detalleId)
        .Select(d => (Guid?)d.Nomina.EmpresaId)
        .FirstOrDefaultAsync(cancellationToken);
    if (empresaIdDetalle is null)
        return Results.NotFound();
    if (empresaIdDetalle != empresaIdClaim)
        return Results.Forbid();

    var pdf = await nominaPdfService.GenerateReciboPdfAsync(detalleId, cancellationToken);
    return Results.File(pdf, "application/pdf", $"recibo-nomina-{detalleId:N}.pdf");
}).RequireAuthorization();

app.MapGet("/api/nomina/{nominaId:guid}/recibos/pdf", async (HttpContext httpContext, Guid nominaId, IDbContextFactory<CrmDbContext> dbFactory, INominaPdfService nominaPdfService, CancellationToken cancellationToken) =>
{
    if (!(httpContext.User.Identity?.IsAuthenticated ?? false))
        return Results.Unauthorized();
    if (!httpContext.User.HasClaim("Capacidad", "nominas.ver"))
        return Results.Forbid();
    if (!Guid.TryParse(httpContext.User.FindFirst("EmpresaId")?.Value, out var empresaIdClaim) || empresaIdClaim == Guid.Empty)
        return Results.Forbid();

    await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
    var empresaIdNomina = await db.Nominas
        .AsNoTracking()
        .Where(n => n.Id == nominaId)
        .Select(n => (Guid?)n.EmpresaId)
        .FirstOrDefaultAsync(cancellationToken);
    if (empresaIdNomina is null)
        return Results.NotFound();
    if (empresaIdNomina != empresaIdClaim)
        return Results.Forbid();

    var pdf = await nominaPdfService.GenerateRecibosPdfAsync(nominaId, cancellationToken);
    return Results.File(pdf, "application/pdf", $"recibos-nomina-{nominaId:N}.pdf");
}).RequireAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = (context, report) => WriteHealthResponse(context, report, app.Environment.IsDevelopment())
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = (context, report) => WriteHealthResponse(context, report, app.Environment.IsDevelopment())
});

// Fix one-time: pedidos existentes con TipoPrecio=0 → Contado=1
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

    try
    {
        var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
        if (await db.Database.CanConnectAsync())
        {

            await SeedPlanes(db);

            // Seed Empresa default + Auth: tipos de usuario, capacidades y usuario admin
            var empresaDefault = await SeedEmpresaDefault(db);
            var authServiceForSeed = scope.ServiceProvider.GetRequiredService<IAuthService>();
            await SeedAuth(db, authServiceForSeed, empresaDefault.Id);
            await EnsureBootstrapSuperAdmin(
                db,
                authServiceForSeed,
                builder.Configuration["Bootstrap:SuperAdmin:Email"],
                builder.Configuration["Bootstrap:SuperAdmin:Password"],
                empresaDefault.Id,
                logger);
            await SincronizarSuscripcionesEmpresas(db);
        }
        else
        {
            logger.LogWarning("Database bootstrap skipped because the MySQL connection is not available.");
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Database bootstrap skipped because the MySQL connection is not available.");
    }
}

app.Run();

static async Task<RrhhEstadoAgente> UpsertEstadoAgenteAsync(CrmDbContext db, Guid empresaId, string? nombreAgente, CancellationToken cancellationToken)
{
    var nombreNormalizado = string.IsNullOrWhiteSpace(nombreAgente) ? "Zenith Asistencia Worker" : nombreAgente.Trim();
    var estadoAgente = await db.RrhhEstadosAgente
        .FirstOrDefaultAsync(x => x.EmpresaId == empresaId && x.NombreAgente == nombreNormalizado, cancellationToken);

    if (estadoAgente != null)
    {
        return estadoAgente;
    }

    estadoAgente = new RrhhEstadoAgente
    {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        NombreAgente = nombreNormalizado,
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    };

    db.RrhhEstadosAgente.Add(estadoAgente);
    return estadoAgente;
}

static bool TryAuthenticateAgentRequest(HttpContext httpContext, IConfiguration configuration, Guid empresaId, string? nombreAgente, out AsistenciaAgentConfig? matchedAgent)
{
    matchedAgent = null;
    var requestApiKey = httpContext.Request.Headers["X-Zenith-Worker-Key"].ToString();
    var globalApiKey = configuration["Asistencia:WorkerApiKey"];
    var configuredAgents = configuration.GetSection("Asistencia:Agentes").Get<List<AsistenciaAgentConfig>>() ?? [];

    if (string.IsNullOrWhiteSpace(globalApiKey) && configuredAgents.Count == 0)
    {
        return true;
    }

    if (string.IsNullOrWhiteSpace(requestApiKey))
    {
        return false;
    }

    matchedAgent = configuredAgents
        .Where(a => a.Activo && a.EmpresaId == empresaId && !string.IsNullOrWhiteSpace(a.ApiKey))
        .FirstOrDefault(a =>
            string.Equals(a.ApiKey, requestApiKey, StringComparison.Ordinal)
            && (string.IsNullOrWhiteSpace(a.NombreAgente)
                || string.IsNullOrWhiteSpace(nombreAgente)
                || string.Equals(a.NombreAgente, nombreAgente, StringComparison.OrdinalIgnoreCase)));

    if (matchedAgent != null)
    {
        return true;
    }

    return !string.IsNullOrWhiteSpace(globalApiKey)
        && string.Equals(globalApiKey, requestApiKey, StringComparison.Ordinal);
}

static string? LimitarTexto(string? valor, int maxLength, string? fallback = null)
{
    var texto = string.IsNullOrWhiteSpace(valor) ? fallback : valor.Trim();
    if (string.IsNullOrWhiteSpace(texto))
    {
        return texto;
    }

    return texto.Length <= maxLength ? texto : texto[..maxLength];
}

static string GetRequiredConnectionString(IConfiguration configuration)
{
    var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__ZenithConnection")
        ?? configuration.GetConnectionString("ZenithConnection");

    if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Connection string 'ZenithConnection' not configured. Use environment variables or local secrets.");

    return connectionString;
}

static bool IsJsonEndpoint(PathString path)
    => path.StartsWithSegments("/auth/session", StringComparison.OrdinalIgnoreCase);

static CookieSecurePolicy ParseCookieSecurePolicy(string? value)
    => Enum.TryParse<CookieSecurePolicy>(value, ignoreCase: true, out var parsed)
        ? parsed
        : CookieSecurePolicy.Always;

static Microsoft.AspNetCore.Http.SameSiteMode ParseSameSiteMode(string? value)
    => Enum.TryParse<Microsoft.AspNetCore.Http.SameSiteMode>(value, ignoreCase: true, out var parsed)
        ? parsed
        : Microsoft.AspNetCore.Http.SameSiteMode.Strict;

static AuthenticationProperties CreateAuthenticationProperties()
    => new()
    {
        AllowRefresh = true,
        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60),
        IsPersistent = false
    };

static async Task<ClaimsPrincipal> CreatePrincipalAsync(Usuario usuario, IAuthService authService, IModuloAccesoService moduloAccesoService)
{
    var capacidades = await authService.GetCapacidadesAsync(usuario.TipoUsuarioId);
    var isSuperAdmin = string.Equals(usuario.TipoUsuario.Nombre, "SuperAdmin", StringComparison.OrdinalIgnoreCase);
    var capacidadesFiltradas = await moduloAccesoService.FiltrarCapacidadesAsync(usuario.EmpresaId, capacidades, isSuperAdmin);
    var modulosHabilitados = await moduloAccesoService.ObtenerModulosHabilitadosAsync(usuario.EmpresaId);

    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
        new(ClaimTypes.Email, usuario.Email),
        new(ClaimTypes.Name, usuario.NombreCompleto),
        new(ClaimTypes.Role, usuario.TipoUsuario.Nombre),
        new("TipoUsuarioId", usuario.TipoUsuarioId.ToString()),
        new("EmpresaId", usuario.EmpresaId.ToString()),
        new("RequirePasswordChange", usuario.RequiereCambioPassword.ToString())
    };

    claims.AddRange(capacidadesFiltradas.Select(cap => new Claim("Capacidad", cap)));
    claims.AddRange(modulosHabilitados.Select(modulo => new Claim("Modulo", modulo)));

    return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
}

static Task WriteHealthResponse(HttpContext context, HealthReport report, bool includeDetails)
{
    context.Response.ContentType = "application/json";
    if (includeDetails)
    {
        var payload = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            })
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }

    return context.Response.WriteAsync(JsonSerializer.Serialize(new
    {
        status = report.Status.ToString()
    }));
}

static async Task EnsureBootstrapSuperAdmin(
    CrmDbContext db,
    IAuthService authService,
    string? email,
    string? password,
    Guid empresaId,
    ILogger logger)
{
    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
    {
        return;
    }

    var emailNormalizado = email.Trim().ToLowerInvariant();

    if (password.Length < 8)
    {
        logger.LogWarning("Bootstrap SuperAdmin: la contraseña configurada es demasiado corta (<8). Se omite.");
        return;
    }

    var superTipo = await db.TiposUsuario
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(t => t.EmpresaId == empresaId && t.Nombre == "SuperAdmin");

    if (superTipo == null)
    {
        logger.LogWarning("Bootstrap SuperAdmin: no se encontró el TipoUsuario 'SuperAdmin' para la empresa {EmpresaId}.", empresaId);
        return;
    }

    var usuario = await db.Usuarios
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(u => u.Email == emailNormalizado);

    var hash = authService.HashPassword(password);

    if (usuario == null)
    {
        db.Usuarios.Add(new Usuario
        {
            NombreCompleto = "Bootstrap SuperAdmin",
            Email = emailNormalizado,
            PasswordHash = hash,
            TipoUsuarioId = superTipo.Id,
            EmpresaId = empresaId,
            RequiereCambioPassword = false,
            IsActive = true,
            IntentosFallidos = 0,
            BloqueadoHastaUtc = null,
            CreatedAt = DateTime.UtcNow
        });
        logger.LogInformation("Bootstrap SuperAdmin: se creó el usuario {Email} desde variables de entorno.", emailNormalizado);
    }
    else
    {
        usuario.PasswordHash = hash;
        usuario.TipoUsuarioId = superTipo.Id;
        usuario.EmpresaId = empresaId;
        usuario.IsActive = true;
        usuario.RequiereCambioPassword = false;
        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHastaUtc = null;
        usuario.UpdatedAt = DateTime.UtcNow;
        logger.LogInformation("Bootstrap SuperAdmin: se reseteó el usuario {Email} desde variables de entorno.", emailNormalizado);
    }

    await db.SaveChangesAsync();
}

static async Task<Empresa> SeedEmpresaDefault(CrmDbContext db)
{
    var empresa = await db.Empresas.FirstOrDefaultAsync();
    if (empresa != null)
    {
        if (empresa.Estado != EmpresaEstado.Activa || empresa.IsSuspended || empresa.ActivatedAt == null)
        {
            empresa.Estado = EmpresaEstado.Activa;
            empresa.IsSuspended = false;
            empresa.IsActive = true;
            empresa.ActivatedAt ??= DateTime.UtcNow;
            empresa.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        // Asegurar que AppConfigs existan para esta empresa
        await SeedAppConfigs(db, empresa.Id);
        return empresa;
    }

    empresa = new Empresa
    {
        Codigo = "DEFAULT",
        RazonSocial = "MundoVs",
        NombreComercial = "MundoVs",
        Slogan = "CRM & Producción",
        Estado = EmpresaEstado.Activa,
        ActivatedAt = DateTime.UtcNow,
        IsActive = true,
        IsSuspended = false
    };
    db.Empresas.Add(empresa);
    await db.SaveChangesAsync();

    await SeedAppConfigs(db, empresa.Id);
    return empresa;
}

static async Task SeedPlanes(CrmDbContext db)
{
    var planesDef = new (string Nombre, string Desc, decimal Mensual, decimal Anual, int? LimiteUsuarios, string Modulos, int TrialDays)[]
    {
        ("Base", "Operación esencial para empresas pequeñas", 499m, 4990m, 5, "clientes,productos,pedidos", 14),
        ("Pro", "Incluye compras, RRHH y nóminas", 999m, 9990m, 20, "clientes,productos,pedidos,proveedores,cxp,empleados,nominas", 14),
        ("Enterprise", "Plan avanzado con operación completa y mayor capacidad", 1999m, 19990m, null, "todos", 30)
    };

    foreach (var (nombre, desc, mensual, anual, limiteUsuarios, modulos, trialDays) in planesDef)
    {
        var plan = await db.Planes.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Nombre == nombre);
        if (plan == null)
        {
            db.Planes.Add(new Plan
            {
                Id = Guid.NewGuid(),
                Nombre = nombre,
                Descripcion = desc,
                PrecioMensual = mensual,
                PrecioAnual = anual,
                LimiteUsuarios = limiteUsuarios,
                ModulosIncluidos = modulos,
                TrialDays = trialDays,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            plan.Descripcion = desc;
            plan.PrecioMensual = mensual;
            plan.PrecioAnual = anual;
            plan.LimiteUsuarios = limiteUsuarios;
            plan.ModulosIncluidos = modulos;
            plan.TrialDays = trialDays;
            plan.IsActive = true;
            plan.UpdatedAt = DateTime.UtcNow;
        }
    }

    await db.SaveChangesAsync();
}

static async Task SincronizarSuscripcionesEmpresas(CrmDbContext db)
{
    var hoy = DateTime.UtcNow.Date;
    var suscripcionesActuales = await db.SuscripcionesEmpresa.IgnoreQueryFilters()
        .Include(s => s.Plan)
        .Include(s => s.Empresa)
        .OrderByDescending(s => s.FechaInicio)
        .ThenByDescending(s => s.CreatedAt)
        .ToListAsync();

    var cambios = false;

    foreach (var suscripcion in suscripcionesActuales.GroupBy(s => s.EmpresaId).Select(g => g.First()))
    {
        if (suscripcion.Estado is EstadoSuscripcion.Trial or EstadoSuscripcion.Activa
            && suscripcion.FechaFin.HasValue
            && suscripcion.FechaFin.Value.Date < hoy)
        {
            suscripcion.Estado = EstadoSuscripcion.Vencida;
            suscripcion.RenovacionAutomatica = false;
            suscripcion.UpdatedAt = DateTime.UtcNow;
            cambios = true;
        }

        var empresa = suscripcion.Empresa;
        empresa.PlanActualId = suscripcion.PlanId;
        empresa.MaxUsuarios ??= suscripcion.Plan.LimiteUsuarios;

        switch (suscripcion.Estado)
        {
            case EstadoSuscripcion.Trial:
                empresa.Estado = EmpresaEstado.Demo;
                empresa.TrialEndsAt = suscripcion.FechaFin;
                empresa.IsSuspended = false;
                empresa.IsActive = true;
                break;
            case EstadoSuscripcion.Activa:
                empresa.Estado = EmpresaEstado.Activa;
                empresa.TrialEndsAt = null;
                empresa.IsSuspended = false;
                empresa.IsActive = true;
                empresa.ActivatedAt ??= suscripcion.FechaInicio;
                break;
            case EstadoSuscripcion.Suspendida:
            case EstadoSuscripcion.Vencida:
                if (empresa.Estado != EmpresaEstado.Cancelada)
                    empresa.Estado = EmpresaEstado.Suspendida;
                empresa.IsSuspended = true;
                empresa.IsActive = true;
                break;
            case EstadoSuscripcion.Cancelada:
                empresa.Estado = EmpresaEstado.Cancelada;
                empresa.IsSuspended = true;
                empresa.IsActive = false;
                break;
        }

        empresa.UpdatedAt = DateTime.UtcNow;
        cambios = true;
    }

    if (cambios)
        await db.SaveChangesAsync();
}

static async Task SeedAppConfigs(CrmDbContext db, Guid empresaId)
{
    var configsDef = new (string Clave, string Valor, string Desc)[]
    {
        ("CompanyName", "Empresa", "Nombre de la empresa"),
        ("CompanySlogan", "CRM & Producción", "Slogan o subtítulo"),
        ("GiroEmpresa", "General", "Giro o actividad principal de la empresa"),
        ("Modulo:Calzado", "false", "Habilitar módulo de Calzado"),
        ("Modulo:Serigrafia", "true", "Habilitar módulo de Serigrafía"),
        ("Consecutivo:Producto", "0", "Consecutivo para código de Producto"),
        ("Consecutivo:MateriaPrima", "0", "Consecutivo para código de Materia Prima"),
        ("Consecutivo:Insumo", "0", "Consecutivo para código de Insumo"),
        ("Consecutivo:Cliente", "0", "Consecutivo para código de Cliente"),
    };

    foreach (var (clave, valor, desc) in configsDef)
    {
        if (!await db.AppConfigs.AnyAsync(c => c.EmpresaId == empresaId && c.Clave == clave))
        {
            db.AppConfigs.Add(new AppConfig { EmpresaId = empresaId, Clave = clave, Valor = valor, Descripcion = desc });
        }
    }
    await db.SaveChangesAsync();
}

static async Task SeedModulosAcceso(CrmDbContext db, Guid empresaId)
{
    var existentes = await db.ModulosAcceso.IgnoreQueryFilters().ToDictionaryAsync(m => m.Clave);

    foreach (var moduloDef in ModuloAccesoCatalog.Todos)
    {
        if (!existentes.TryGetValue(moduloDef.Clave, out var modulo))
        {
            modulo = new ModuloAcceso
            {
                Clave = moduloDef.Clave,
                Nombre = moduloDef.Nombre,
                Descripcion = moduloDef.Descripcion,
                Orden = moduloDef.Orden,
                EsGlobal = moduloDef.EsGlobal,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            db.ModulosAcceso.Add(modulo);
            existentes[moduloDef.Clave] = modulo;
        }
        else
        {
            modulo.Nombre = moduloDef.Nombre;
            modulo.Descripcion = moduloDef.Descripcion;
            modulo.Orden = moduloDef.Orden;
            modulo.EsGlobal = moduloDef.EsGlobal;
            modulo.IsActive = true;
            modulo.UpdatedAt = DateTime.UtcNow;
        }
    }

    await db.SaveChangesAsync();

    var modulosEmpresaExistentes = await db.EmpresasModulosAcceso
        .IgnoreQueryFilters()
        .Where(m => m.EmpresaId == empresaId)
        .ToDictionaryAsync(m => m.ModuloAccesoId);

    foreach (var modulo in existentes.Values)
    {
        if (modulosEmpresaExistentes.ContainsKey(modulo.Id))
            continue;

        db.EmpresasModulosAcceso.Add(new EmpresaModuloAcceso
        {
            EmpresaId = empresaId,
            ModuloAccesoId = modulo.Id,
            Habilitado = modulo.Clave is ModuloAccesoCatalog.Plataforma ? false : true,
            Origen = modulo.EsGlobal ? OrigenModuloEmpresa.Sistema : OrigenModuloEmpresa.Plan,
            VigenteDesde = DateTime.UtcNow.Date,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
    }

    await db.SaveChangesAsync();
}

static async Task SeedAuth(CrmDbContext db, IAuthService authService, Guid empresaId)
{
    await SeedModulosAcceso(db, empresaId);

    var modulosPorClave = await db.ModulosAcceso
        .IgnoreQueryFilters()
        .ToDictionaryAsync(m => m.Clave, m => m.Id);

    // ═══ CAPACIDADES ═══
    var caps = new (string Clave, string Nombre, string Modulo, string ModuloClave, string Desc)[]
    {
        ("empresas.ver", "Ver empresas", "Plataforma", ModuloAccesoCatalog.Plataforma, "Acceso a la lista de empresas"),
        ("empresas.editar", "Administrar empresas", "Plataforma", ModuloAccesoCatalog.Plataforma, "Crear y editar empresas"),
        ("clientes.ver", "Ver clientes", "Clientes", ModuloAccesoCatalog.Catalogos, "Acceso a la lista de clientes"),
        ("clientes.editar", "Editar clientes", "Clientes", ModuloAccesoCatalog.Catalogos, "Crear y editar clientes"),
        ("productos.ver", "Ver productos", "Productos", ModuloAccesoCatalog.Catalogos, "Acceso al catálogo de productos"),
        ("productos.editar", "Editar productos", "Productos", ModuloAccesoCatalog.Catalogos, "Crear y editar productos"),
        ("pedidos.ver", "Ver pedidos", "Pedidos", ModuloAccesoCatalog.Manufactura, "Acceso a la lista de pedidos"),
        ("pedidos.crear", "Crear pedidos", "Pedidos", ModuloAccesoCatalog.Manufactura, "Crear nuevos pedidos"),
        ("pedidos.editar", "Editar pedidos", "Pedidos", ModuloAccesoCatalog.Manufactura, "Modificar pedidos existentes"),
        ("pedidos.cancelar", "Cancelar pedidos", "Pedidos", ModuloAccesoCatalog.Manufactura, "Cancelar pedidos"),
        ("cotizaciones.ver", "Ver cotizaciones", "Cotizaciones", ModuloAccesoCatalog.Manufactura, "Acceso a cotizaciones"),
        ("cotizaciones.editar", "Editar cotizaciones", "Cotizaciones", ModuloAccesoCatalog.Manufactura, "Crear y editar cotizaciones"),
        ("serigrafia.ver", "Ver serigrafía", "Serigrafía", ModuloAccesoCatalog.Manufactura, "Acceso al módulo de serigrafía"),
        ("serigrafia.editar", "Editar serigrafía", "Serigrafía", ModuloAccesoCatalog.Manufactura, "Configurar catálogos de serigrafía"),
        ("contabilidad.dashboard", "Ver dashboard", "Contabilidad", ModuloAccesoCatalog.Contabilidad, "Acceso al dashboard"),
        ("contabilidad.estado_resultados", "Estado de resultados", "Contabilidad", ModuloAccesoCatalog.Contabilidad, "Ver estado de resultados"),
        ("contabilidad.cuentas_cobrar", "Cuentas por cobrar", "Contabilidad", ModuloAccesoCatalog.Contabilidad, "Ver cuentas por cobrar"),
        ("facturas.ver", "Ver facturas", "Facturación", ModuloAccesoCatalog.Contabilidad, "Acceso a la lista de facturas"),
        ("facturas.editar", "Editar facturas", "Facturación", ModuloAccesoCatalog.Contabilidad, "Crear, editar y emitir facturas"),
        ("configuracion.ver", "Ver configuración", "Configuración", ModuloAccesoCatalog.Configuracion, "Acceso a configuración del sistema"),
        ("configuracion.editar", "Editar configuración", "Configuración", ModuloAccesoCatalog.Configuracion, "Modificar configuración del sistema"),
        ("usuarios.ver", "Ver usuarios", "Usuarios", ModuloAccesoCatalog.Administracion, "Acceso a gestión de usuarios"),
        ("usuarios.editar", "Editar usuarios", "Usuarios", ModuloAccesoCatalog.Administracion, "Crear y editar usuarios y roles"),
        ("proveedores.ver", "Ver proveedores", "Proveedores", ModuloAccesoCatalog.Catalogos, "Acceso a la lista de proveedores"),
        ("proveedores.editar", "Editar proveedores", "Proveedores", ModuloAccesoCatalog.Catalogos, "Crear y editar proveedores"),
        ("cxp.ver", "Ver cuentas por pagar", "Cuentas por Pagar", ModuloAccesoCatalog.Contabilidad, "Acceso a cuentas por pagar"),
        ("cxp.editar", "Editar cuentas por pagar", "Cuentas por Pagar", ModuloAccesoCatalog.Contabilidad, "Crear facturas/notas y registrar pagos"),
        ("empleados.ver", "Ver empleados", "Empleados", ModuloAccesoCatalog.Rrhh, "Acceso a la lista de empleados"),
        ("empleados.editar", "Editar empleados", "Empleados", ModuloAccesoCatalog.Rrhh, "Crear y editar empleados"),
        ("rrhh.marcaciones.auditoria", "Auditar marcaciones", "Empleados", ModuloAccesoCatalog.Rrhh, "Acceso administrativo al detalle crudo de marcaciones"),
        ("rrhh.tiempoextra.aprobar", "Aprobar tiempo extra", "Nóminas", ModuloAccesoCatalog.Rrhh, "Autoriza pago o banco de horas a partir de asistencias"),
        ("nominas.ver", "Ver nóminas", "Nóminas", ModuloAccesoCatalog.Rrhh, "Acceso al módulo de nóminas"),
        ("nominas.editar", "Editar nóminas", "Nóminas", ModuloAccesoCatalog.Rrhh, "Crear y procesar nóminas"),
        ("inventario.ver", "Ver inventario", "Inventario", ModuloAccesoCatalog.Logistica, "Acceso al módulo unificado de inventario"),
        ("inventario.editar", "Editar inventario", "Inventario", ModuloAccesoCatalog.Logistica, "Alta rápida de materias primas e insumos desde inventario"),
        ("inventario.movimientos", "Registrar movimientos", "Inventario", ModuloAccesoCatalog.Logistica, "Entradas, salidas y ajustes de inventario"),
        ("inventario.reportes", "Ver reportes de inventario", "Inventario", ModuloAccesoCatalog.Logistica, "Consulta de movimientos y costo de inventario"),
    };

    var existentes = await db.Capacidades.AsNoTracking().ToListAsync();
    var existentesDict = existentes.ToDictionary(c => c.Clave, c => c.Id);

    foreach (var (clave, nombre, modulo, moduloClave, desc) in caps)
    {
        if (!modulosPorClave.TryGetValue(moduloClave, out var moduloAccesoId))
            continue;

        var existente = existentes.FirstOrDefault(c => c.Clave == clave);
        if (existente == null)
        {
            db.Capacidades.Add(new Capacidad
            {
                Clave = clave,
                Nombre = nombre,
                Modulo = modulo,
                ModuloAccesoId = moduloAccesoId,
                Descripcion = desc
            });
            continue;
        }

        existente.Nombre = nombre;
        existente.Modulo = modulo;
        existente.ModuloAccesoId = moduloAccesoId;
        existente.Descripcion = desc;
    }
    await db.SaveChangesAsync();

    var capDict = await db.Capacidades.AsNoTracking().ToDictionaryAsync(c => c.Clave, c => c.Id);

    // ═══ TIPOS DE USUARIO ═══
    var tiposData = new (string Nombre, string Desc, bool IsSystem, string[] Caps)[]
    {
        ("SuperAdmin", "Administración global de empresas", true, ["empresas.ver", "empresas.editar"]),
        ("Administrador", "Acceso total al sistema", false, [.. capDict.Keys.Where(k => !k.StartsWith("empresas."))]),
        ("Gerente", "Reportes, pedidos, clientes, cotizaciones", false,
            ["clientes.ver", "clientes.editar", "productos.ver", "productos.editar",
             "pedidos.ver", "pedidos.crear", "pedidos.editar", "pedidos.cancelar",
             "cotizaciones.ver", "cotizaciones.editar", "serigrafia.ver", "serigrafia.editar",
             "contabilidad.dashboard", "contabilidad.estado_resultados", "contabilidad.cuentas_cobrar", "facturas.ver", "facturas.editar",
              "configuracion.ver", "inventario.ver", "inventario.reportes"]),
        ("Ventas", "Clientes, pedidos y cotizaciones", false,
            ["clientes.ver", "clientes.editar", "productos.ver",
             "pedidos.ver", "pedidos.crear", "pedidos.editar",
             "cotizaciones.ver", "cotizaciones.editar", "serigrafia.ver"]),
        ("Producción", "Pedidos y procesos de serigrafía", false,
            ["productos.ver", "pedidos.ver", "cotizaciones.ver", "serigrafia.ver", "serigrafia.editar",
             "inventario.ver", "inventario.editar", "inventario.movimientos", "inventario.reportes"]),
        ("Contador", "Reportes financieros y cobranza", false,
            ["contabilidad.dashboard", "contabilidad.estado_resultados", "contabilidad.cuentas_cobrar", "facturas.ver", "facturas.editar",
             "inventario.ver", "inventario.reportes"]),
    };

    Guid adminTipoId = Guid.Empty;
    Guid superAdminTipoId = Guid.Empty;
    foreach (var (nombre, desc, isSystem, capsKeys) in tiposData)
    {
        var tipo = await db.TiposUsuario.FirstOrDefaultAsync(t => t.EmpresaId == empresaId && t.Nombre == nombre);
        if (tipo == null)
        {
            tipo = new TipoUsuario { Nombre = nombre, Descripcion = desc, IsSystem = isSystem, EmpresaId = empresaId, IsActive = true };
            db.TiposUsuario.Add(tipo);
            await db.SaveChangesAsync();
        }

        if (nombre == "Administrador") adminTipoId = tipo.Id;
        if (nombre == "SuperAdmin") superAdminTipoId = tipo.Id;

        var actuales = await db.TipoUsuarioCapacidades
            .Where(tc => tc.TipoUsuarioId == tipo.Id)
            .Select(tc => tc.Capacidad.Clave)
            .ToListAsync();

        // Enforce least-privilege (cleanup)
        if (nombre == "SuperAdmin")
        {
            var extras = await db.TipoUsuarioCapacidades
                .Include(tc => tc.Capacidad)
                .Where(tc => tc.TipoUsuarioId == tipo.Id && !tc.Capacidad.Clave.StartsWith("empresas."))
                .ToListAsync();
            if (extras.Count != 0)
            {
                db.TipoUsuarioCapacidades.RemoveRange(extras);
                await db.SaveChangesAsync();
                actuales = actuales.Where(c => c.StartsWith("empresas.")).ToList();
}
        }
        else
        {
            var extras = await db.TipoUsuarioCapacidades
                .Include(tc => tc.Capacidad)
                .Where(tc => tc.TipoUsuarioId == tipo.Id && tc.Capacidad.Clave.StartsWith("empresas."))
                .ToListAsync();
            if (extras.Count != 0)
            {
                db.TipoUsuarioCapacidades.RemoveRange(extras);
                await db.SaveChangesAsync();
                actuales = actuales.Where(c => !c.StartsWith("empresas.")).ToList();
            }
        }

        foreach (var key in capsKeys)
        {
            if (actuales.Contains(key)) continue;
            if (capDict.TryGetValue(key, out var capId))
                db.TipoUsuarioCapacidades.Add(new TipoUsuarioCapacidad { TipoUsuarioId = tipo.Id, CapacidadId = capId });
        }
        await db.SaveChangesAsync();
    }

    // ═══ USUARIO ADMIN POR DEFECTO ═══
    if (!await db.Usuarios.AnyAsync())
    {
        db.Usuarios.Add(new Usuario
        {
            NombreCompleto = "Administrador",
            Email = "admin@mundovs.com",
            PasswordHash = authService.HashPassword("Admin123!"),
            TipoUsuarioId = adminTipoId,
            EmpresaId = empresaId,
            RequiereCambioPassword = true,
            IsActive = true
        });
        await db.SaveChangesAsync();
    }

    // ═══ USUARIO SUPERADMIN POR DEFECTO ═══
    if (superAdminTipoId != Guid.Empty && !await db.Usuarios.AnyAsync(u => u.Email == "superadmin@mundovs.com"))
    {
        db.Usuarios.Add(new Usuario
        {
            NombreCompleto = "SuperAdmin",
            Email = "superadmin@mundovs.com",
            PasswordHash = authService.HashPassword("SuperAdmin123!"),
            TipoUsuarioId = superAdminTipoId,
            EmpresaId = empresaId,
            RequiereCambioPassword = true,
            IsActive = true
        });
        await db.SaveChangesAsync();
    }
}

file sealed class AsistenciaAgentConfig
{
    public Guid EmpresaId { get; set; }
    public string? NombreAgente { get; set; }
    public string? ApiKey { get; set; }
    public bool Activo { get; set; } = true;
    public int? IntervaloSegundos { get; set; }
    public bool PermitirLecturaUsuarios { get; set; }
    public bool ModoDiagnostico { get; set; }
}

public sealed record AuthLoginRequest(string Email, string Password);

public sealed record AuthSessionResponse(bool Succeeded, string? Error = null, string? RedirectUrl = null);
