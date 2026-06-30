using FluentAssertions;
using Libify.Domain.Helpers;

namespace Libify.Test.Domain
{
    public class DateTimeHelperTests
    {
        [Fact]
        public void UtcNow_AplicaOffsetBrasil()
        {
            var esperado = DateTime.UtcNow.AddHours(-3);

            DateTimeHelper.UtcNow.Should().BeCloseTo(esperado, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void Now_RetornaValorRecente()
        {
            var esperado = DateTime.Now.AddHours(-3);

            DateTimeHelper.Now.Should().BeCloseTo(esperado, TimeSpan.FromSeconds(5));
        }
    }
}
