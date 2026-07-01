using FluentAssertions;
using Libify.Domain.Enum;
using Libify.Domain.Helpers;

namespace Libify.Test.Domain
{
  public class AsaasStatusMapperTests
  {
    [Theory]
    [InlineData("PENDING", StatusCobranca.Pendente)]
    [InlineData("RECEIVED", StatusCobranca.Recebida)]
    [InlineData("OVERDUE", StatusCobranca.Vencida)]
    public void MapearPagamento_ConverteStatus(string entrada, StatusCobranca esperado)
      => AsaasStatusMapper.MapearPagamento(entrada).Should().Be(esperado);

    [Fact]
    public void MapearFormaPagamento_Pix()
      => AsaasStatusMapper.MapearFormaPagamento(FormaPagamento.Pix).Should().Be("PIX");

    [Fact]
    public void MapearCicloAssinatura_Mensal()
      => AsaasStatusMapper.MapearCicloAssinatura(TipoPlano.Mensal).Should().Be("MONTHLY");
  }
}
