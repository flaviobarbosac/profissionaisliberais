using Libify.Domain.Enum;

namespace Libify.API.Dto
{
    public class CobrancaDto
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public Guid ClienteId { get; set; }
        public Guid? PropostaId { get; set; }
        public FormaPagamento FormaPagamento { get; set; }
        public StatusCobranca Status { get; set; }
        public decimal Valor { get; set; }
        public DateTime Vencimento { get; set; }
        public int Parcelas { get; set; } = 1;
        public string? Descricao { get; set; }
        public string? PixQrCode { get; set; }
        public string? PixCopiaECola { get; set; }
        public string? BoletoUrl { get; set; }
        public string? LinkPagamento { get; set; }
    }
}
