using Libify.Domain.Enum;

namespace Libify.API.Dto
{
  public class EmitirCobrancaDto
  {
    public Guid ClienteId { get; set; }
    public decimal Valor { get; set; }
    public DateTime Vencimento { get; set; }
    public FormaPagamento FormaPagamento { get; set; } = FormaPagamento.Pix;
    public Guid? PropostaId { get; set; }
    public int Parcelas { get; set; } = 1;
    public string? Descricao { get; set; }
  }

  public class AssinarPlanoDto
  {
    public TipoPlano Tipo { get; set; }
    public FormaPagamento FormaPagamento { get; set; } = FormaPagamento.Pix;
  }
}
