using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Mignow.Atc.Api.Middleware;
using Mignow.Atc.Api.Realtime;
using Mignow.Atc.Application.Ports.Execution;
using Mignow.Atc.Infra.Database.Context;
using Mignow.Atc.Infra.Database.Seeding;
using Mignow.Atc.Infra.IoC;
using Serilog;

namespace Mignow.Atc.Api.Bootstrap;

/// <summary>
/// Bootstrap da API (blueprint §7.3): auth por ambiente (Entra fora de Development/Local/Testing; usuário de
/// desenvolvimento dentro), Serilog, Swagger com nullable fiel e uma tag por controller, CORS por configuração, i18n
/// pelo Accept-Language, SignalR (<c>/hubs/runs</c>), worker da fila e migração + seed no startup.
/// </summary>
public static class AtcApi
{
    public const string Title = "MigATC API";

    public static bool IsLocalLike(IHostEnvironment env) => env.IsDevelopment() || env.IsEnvironment("Local") || env.IsEnvironment("Testing");

    public static WebApplicationBuilder AddAtcApi(this WebApplicationBuilder builder)
    {
        var entra = builder.Configuration.GetSection(EntraOptions.Section).Get<EntraOptions>() ?? new EntraOptions();
        var localLike = IsLocalLike(builder.Environment);
        if (!localLike && !entra.IsConfigured)
            throw new InvalidOperationException("Entra:TenantId e Entra:ClientId são obrigatórios fora de Development/Local (fail-fast).");
        // Sem Entra em ambiente local: as requisições viram o usuário de desenvolvimento (dev@migatc.local).
        builder.Configuration["Auth:Dev:Enabled"] = (localLike && !entra.IsConfigured).ToString();

        builder.Host.UseSerilog((ctx, sp, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration).ReadFrom.Services(sp).Enrich.FromLogContext()
            .WriteTo.Console());

        builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        builder.Services.AddSignalR().AddJsonProtocol(o => o.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(o =>
        {
            o.SwaggerDoc("v1", new OpenApiInfo { Title = Title, Version = "v1" });
            o.SupportNonNullableReferenceTypes();
            o.NonNullableReferenceTypesAsRequired();
            // operationId estável para o Orval (um client por tag): <Controller>_<Ação>.
            o.CustomOperationIds(api => api.ActionDescriptor.RouteValues.TryGetValue("controller", out var c) && api.ActionDescriptor.RouteValues.TryGetValue("action", out var a)
                ? $"{c}_{a}" : null);
        });

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
        {
            if (!entra.IsConfigured)
                return;
            o.Authority = $"https://login.microsoftonline.com/{entra.TenantId}/v2.0";
            o.MapInboundClaims = false;
            o.TokenValidationParameters.ValidAudiences = [entra.ClientId, $"api://{entra.ClientId}"];
            o.Events = new JwtBearerEvents
            {
                // SignalR manda o token na query string (WebSocket não tem cabeçalho Authorization).
                OnMessageReceived = ctx =>
                {
                    var token = ctx.Request.Query["access_token"];
                    if (!string.IsNullOrEmpty(token) && ctx.HttpContext.Request.Path.StartsWithSegments(RunHub.Path))
                        ctx.Token = token;
                    return Task.CompletedTask;
                },
            };
        });
        builder.Services.AddAuthorization(o =>
        {
            // Só existe o papel Consultor = qualquer autenticado (decisão 20): deny-by-default quando há Entra.
            if (entra.IsConfigured)
                o.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        });

        builder.Services.Configure<RequestLocalizationOptions>(o =>
        {
            o.SetDefaultCulture("en-US");
            o.AddSupportedCultures("en-US", "pt-BR", "es-ES");
            o.AddSupportedUICultures("en-US", "pt-BR", "es-ES");
            o.ApplyCurrentCultureToResponseHeaders = true;
        });

        var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (corsOrigins.Length > 0)
            builder.Services.AddCors(o => o.AddPolicy("Front", p => p.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

        builder.Services.AddDiConfiguration(builder.Configuration);
        builder.Services.AddSingleton<IRunNotifier, SignalRRunNotifier>();
        builder.Services.AddRunWorker();
        return builder;
    }

    public static async Task<WebApplication> UseAtcApiAsync(this WebApplication app)
    {
        // Migração + seed em runtime (único ponto autorizado).
        if (!app.Configuration.GetValue<bool>("Database:SkipStartup"))
        {
            await using var scope = app.Services.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<DatabaseMigrator>().MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedAsync();
        }

        app.UseSerilogRequestLogging();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseRequestLocalization();
        if (!app.Environment.IsProduction())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        if (!app.Environment.IsEnvironment("Testing"))
            app.UseHttpsRedirection();
        var corsOrigins = app.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (corsOrigins.Length > 0)
            app.UseCors("Front");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHub<RunHub>(RunHub.Path);
        return app;
    }
}
