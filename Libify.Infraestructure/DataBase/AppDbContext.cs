using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Libify.Domain.Model;
using Libify.Domain.Model.Base;
using Libify.Domain.Ports;

namespace Libify.Infraestructure.Database
{
    public class AppDbContext : DbContext
    {
        private readonly ITenantContext? _tenant;

        public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext? tenant = null) : base(options)
        {
            _tenant = tenant;
        }

        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Cliente> Cliente { get; set; }
        public DbSet<Servico> Servico { get; set; }
        public DbSet<Proposta> Proposta { get; set; }
        public DbSet<PropostaItem> PropostaItem { get; set; }
        public DbSet<Contrato> Contrato { get; set; }
        public DbSet<Cobranca> Cobranca { get; set; }
        public DbSet<LancamentoFinanceiro> LancamentoFinanceiro { get; set; }
        public DbSet<NotaFiscal> NotaFiscal { get; set; }
        public DbSet<Evento> Evento { get; set; }
        public DbSet<Tarefa> Tarefa { get; set; }
        public DbSet<Post> Post { get; set; }
        public DbSet<Plano> Plano { get; set; }
        public DbSet<AsaasWebhookEvent> AsaasWebhookEvent { get; set; }
        public DbSet<Dispositivo> Dispositivo { get; set; }
        public DbSet<RefreshToken> RefreshToken { get; set; }
        public DbSet<SyncWatermark> SyncWatermark { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tenant vê somente o próprio Usuario (login/refresh usam IgnoreQueryFilters)
            modelBuilder.Entity<Usuario>()
                .HasQueryFilter(e => e.DeletedAt == null && e.Id == _tenant!.UsuarioId);

            // Filtro global de tenant + soft-delete para todas as entidades ITenantOwned
            TenantFilter<Cliente>(modelBuilder);
            TenantFilter<Servico>(modelBuilder);
            TenantFilter<Proposta>(modelBuilder);
            TenantFilter<PropostaItem>(modelBuilder);
            TenantFilter<Contrato>(modelBuilder);
            TenantFilter<Cobranca>(modelBuilder);
            TenantFilter<LancamentoFinanceiro>(modelBuilder);
            TenantFilter<NotaFiscal>(modelBuilder);
            TenantFilter<Evento>(modelBuilder);
            TenantFilter<Tarefa>(modelBuilder);
            TenantFilter<Post>(modelBuilder);
            TenantFilter<Plano>(modelBuilder);
            TenantFilter<Dispositivo>(modelBuilder);
            TenantFilter<RefreshToken>(modelBuilder);
            TenantFilter<SyncWatermark>(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var clr = entityType.ClrType;
                if (!typeof(ModelBase).IsAssignableFrom(clr))
                    continue;

                // Concorrência otimista via xmin (somente PostgreSQL)
                if (Database.IsNpgsql())
                    modelBuilder.Entity(clr)
                        .Property<uint>("xmin")
                        .HasColumnName("xmin")
                        .IsRowVersion()
                        .ValueGeneratedOnAddOrUpdate();

                // Índice por tenant para acelerar o filtro global
                if (typeof(ITenantOwned).IsAssignableFrom(clr))
                    modelBuilder.Entity(clr).HasIndex("UsuarioId");
            }

            modelBuilder.Entity<Dispositivo>()
                .HasIndex(e => new { e.UsuarioId, e.DeviceId }).IsUnique();
            modelBuilder.Entity<RefreshToken>().HasIndex(e => e.TokenHash);
            modelBuilder.Entity<SyncWatermark>()
                .HasIndex(e => new { e.UsuarioId, e.DispositivoId, e.Modulo }).IsUnique();

            modelBuilder.Entity<AsaasWebhookEvent>()
                .HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<AsaasWebhookEvent>()
                .HasIndex(e => e.EventId).IsUnique();
            modelBuilder.Entity<Cobranca>()
                .HasIndex(e => e.AsaasPaymentId);
            modelBuilder.Entity<Plano>()
                .HasIndex(e => e.AsaasSubscriptionId);

            // Tabelas do Outbox/Inbox transacional do MassTransit (entrega confiável + idempotência)
            modelBuilder.AddTransactionalOutboxEntities();

            // Infra de arquitetura por eventos fica isolada em schema próprio; o public guarda só o sistema.
            modelBuilder.Entity<InboxState>().ToTable("InboxState", ArchitectureSchema);
            modelBuilder.Entity<OutboxMessage>().ToTable("OutboxMessage", ArchitectureSchema);
            modelBuilder.Entity<OutboxState>().ToTable("OutboxState", ArchitectureSchema);
        }

        /// <summary>Schema dedicado às tabelas de infraestrutura da arquitetura por eventos.</summary>
        public const string ArchitectureSchema = "Arquitetura";

        private void TenantFilter<T>(ModelBuilder modelBuilder) where T : ModelBase, ITenantOwned
            => modelBuilder.Entity<T>()
                .HasQueryFilter(e => e.DeletedAt == null && e.UsuarioId == _tenant!.UsuarioId);
    }
}
