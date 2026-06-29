using Microsoft.EntityFrameworkCore;
using Libify.Domain.Model;

namespace Libify.Infraestructure.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
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
    }
}
