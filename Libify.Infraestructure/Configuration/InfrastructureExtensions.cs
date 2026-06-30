using System.Diagnostics.CodeAnalysis;
using Libify.Domain.Ports;
using Libify.Infraestructure.Database;
using Libify.Infraestructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Libify.Infraestructure.Configuration
{
    [ExcludeFromCodeCoverage]
    public static class InfrastructureExtensions
    {
        /// <summary>DbContext (PostgreSQL) + contexto de tenant scoped.</summary>
        public static IServiceCollection AddLibifyPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
            services.AddScoped<ITenantContext, TenantContext>();
            return services;
        }

        /// <summary>
        /// Cache híbrido (L1 em memória + L2 Redis quando <c>ConnectionStrings:Redis</c> estiver
        /// configurada). Sem Redis, opera apenas em memória — útil em dev/teste.
        /// </summary>
        public static IServiceCollection AddLibifyCache(this IServiceCollection services, IConfiguration configuration)
        {
            var redis = configuration.GetConnectionString("Redis");
            if (!string.IsNullOrWhiteSpace(redis))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redis;
                    options.InstanceName = "libify:";
                });
            }

            services.AddHybridCache();
            return services;
        }

        /// <summary>JWT (token service) + validação de login social Google.</summary>
        public static IServiceCollection AddLibifySecurity(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
            services.Configure<GoogleAuthOptions>(configuration.GetSection("Google"));
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IGoogleAuthClient, GoogleAuthClient>();
            return services;
        }
    }
}
