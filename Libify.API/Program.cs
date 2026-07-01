using System.Text;
using System.Threading.RateLimiting;
using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RedisRateLimiting;
using RedisRateLimiting.AspNetCore;
using Serilog;
using StackExchange.Redis;
using Libify.API.Configuration;
using Libify.API.Middleware;
using Libify.API.Validation;
using Libify.Infraestructure.Configuration;
using Libify.Infraestructure.Messaging;
using Libify.Domain.Ports;
using Libify.Infraestructure.Security;
using Libify.Infraestructure.Services;
using Libify.Infraestructure.Services.Interface;
using Libify.Repository;
using Libify.Repository.Interface;
using Libify.Services;
using Libify.Services.Asaas;
using Libify.Services.Auth;
using Libify.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

// Observabilidade: Serilog (Console sempre; OTLP fora de Development p/ centralização)
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console();
    if (!context.HostingEnvironment.IsDevelopment())
        configuration.WriteTo.OpenTelemetry();
});

// MVC + validação automática (FluentValidation)
builder.Services.AddControllers(options => options.Filters.Add<ValidationActionFilter>());
builder.Services.AddValidatorsFromAssemblyContaining<ClienteDtoValidator>();

// Versionamento de API (/api/v1)
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var jwtScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Informe o token JWT (sem o prefixo 'Bearer').",
        Reference = new Microsoft.OpenApi.Models.OpenApiReference
        {
            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };
    options.AddSecurityDefinition("Bearer", jwtScheme);
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        { jwtScheme, Array.Empty<string>() }
    });
});

// Autenticação JWT (claims sub/sid preservadas)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "";
// HS256 exige chave de no mínimo 256 bits (32 bytes); falha rápido em produção
if (builder.Environment.IsProduction() && Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("Jwt:Key deve ter ao menos 32 bytes (256 bits) em produção.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Autorização: em Produção exige autenticação por padrão; em dev/homolog liberado
if (builder.Environment.IsProduction())
{
    builder.Services.AddAuthorizationBuilder()
        .SetFallbackPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build());
}
else
{
    builder.Services.AddAuthorization();
}

// Infraestrutura (DbContext + tenant) e segurança (JWT/OTP)
// Cache híbrido (L1 memória + L2 Redis): invalidação de revogação propaga entre instâncias
builder.Services.AddLibifyCache(builder.Configuration);
builder.Services.AddMetrics();
builder.Services.AddLibifyPersistence(builder.Configuration);
builder.Services.AddLibifySecurity(builder.Configuration);

// Repositórios e serviços genéricos
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped(typeof(IBaseServices<>), typeof(BaseServices<>));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDispositivoService, DispositivoService>();

// Mensageria (RabbitMQ + Outbox) — a API atua como produtor
builder.Services.AddLibifyMessaging(builder.Configuration);

// Métricas de negócio (OTP/sync)
builder.Services.AddSingleton<Libify.Infraestructure.Observability.LibifyMetrics>();

// Integração white-label Asaas (com resiliência: retry/timeout/circuit breaker)
var asaasConfig = builder.Configuration.GetSection("Asaas").Get<AsaasConfig>() ?? new AsaasConfig();
builder.Services.AddSingleton(asaasConfig);
builder.Services.AddHttpClient<IAsaasClient, AsaasClient>()
    .AddStandardResilienceHandler();
builder.Services.AddSingleton<ISecretProtector, AesSecretProtector>();
builder.Services.AddScoped<IContaAsaasService, ContaAsaasService>();
builder.Services.AddScoped<ICobrancaAsaasService, CobrancaAsaasService>();
builder.Services.AddScoped<IPlanoAssinaturaService, PlanoAssinaturaService>();
builder.Services.AddScoped<IAsaasWebhookProcessor, AsaasWebhookProcessor>();

// AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfiles).Assembly));

// OpenTelemetry (traces + métricas) -> OTLP (Aspire Dashboard em dev)
// Base comum (MassTransit/HttpClient/runtime/meter de negócio/OTLP) na extensão compartilhada
builder.Services.AddLibifyObservability("Libify.API");
// Instrumentação específica de ASP.NET Core (apenas na API)
builder.Services.AddOpenTelemetry()
    .WithTracing(t => t.AddAspNetCoreInstrumentation())
    .WithMetrics(m => m.AddAspNetCoreInstrumentation());

// Conexão Redis compartilhada (rate limiting distribuído + health check); nula em dev sem Redis.
var redisConnection = builder.Configuration.GetConnectionString("Redis");
IConnectionMultiplexer? redisMultiplexer = null;
if (!string.IsNullOrWhiteSpace(redisConnection))
{
    var redisOptions = ConfigurationOptions.Parse(redisConnection);
    redisOptions.AbortOnConnectFail = false;
    redisMultiplexer = ConnectionMultiplexer.Connect(redisOptions);
    builder.Services.AddSingleton(redisMultiplexer);
}

// Rate limiting (global particionado por tenant/IP + política "auth" mais restrita).
// Limites configuráveis via RateLimiting:*; partição por TenantId quando autenticado.
// Com Redis: contadores compartilhados entre instâncias; sem Redis: fallback em memória (dev).
var globalPermit = builder.Configuration.GetValue<int?>("RateLimiting:GlobalPermitLimit") ?? 100;
var authPermit = builder.Configuration.GetValue<int?>("RateLimiting:AuthPermitLimit") ?? 5;
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    if (redisMultiplexer is not null)
    {
        var mux = redisMultiplexer!;
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        {
            var chave = httpContext.User.FindFirst("sub")?.Value
                ?? httpContext.Connection.RemoteIpAddress?.ToString()
                ?? "anon";
            return RedisRateLimitPartition.GetFixedWindowRateLimiter(chave, _ => new RedisFixedWindowRateLimiterOptions
            {
                ConnectionMultiplexerFactory = () => mux,
                PermitLimit = globalPermit,
                Window = TimeSpan.FromMinutes(1)
            });
        });

        options.AddRedisFixedWindowLimiter("auth", opt =>
        {
            opt.ConnectionMultiplexerFactory = () => mux;
            opt.PermitLimit = authPermit;
            opt.Window = TimeSpan.FromMinutes(1);
        });
    }
    else
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        {
            var chave = httpContext.User.FindFirst("sub")?.Value
                ?? httpContext.Connection.RemoteIpAddress?.ToString()
                ?? "anon";
            return RateLimitPartition.GetFixedWindowLimiter(chave, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = globalPermit,
                Window = TimeSpan.FromMinutes(1)
            });
        });

        options.AddFixedWindowLimiter("auth", limiter =>
        {
            limiter.PermitLimit = authPermit;
            limiter.Window = TimeSpan.FromMinutes(1);
            limiter.QueueLimit = 0;
        });
    }
});

// CORS — em produção exige origens explícitas (fail-closed); fora dela, libera para facilitar testes
var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>();
if (builder.Environment.IsProduction() && corsOrigins.Length == 0)
    throw new InvalidOperationException("Cors:Origins deve ser configurado em produção.");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (corsOrigins.Length > 0)
            policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        else
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// Health checks (Postgres + Redis quando configurado + health do bus MassTransit, registrado automaticamente)
var healthChecks = builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection") ?? "", name: "postgres");
if (!string.IsNullOrWhiteSpace(redisConnection))
    healthChecks.AddRedis(redisConnection, name: "redis");

var app = builder.Build();

app.UseSerilogRequestLogging();

// Swagger habilitado em Development e Homologacao (desabilitado em Production)
if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Resolve o tenant a partir do JWT e valida dispositivo revogado
app.UseMiddleware<TenantMiddleware>();

// Headers de segurança
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    await next();
});

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();
app.MapHealthChecks("/health/ready").AllowAnonymous();

app.Run();
