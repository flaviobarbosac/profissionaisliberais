using System.Net;
using FluentAssertions;
using Libify.Infraestructure.Services;
using Libify.Test.Helpers;

namespace Libify.Test.Infrastructure
{
    public class AsaasClientTests
    {
        private static (AsaasClient client, FakeHttpMessageHandler handler) Criar(string body = "{\"id\":\"acc_1\"}")
        {
            var handler = new FakeHttpMessageHandler(body);
            var http = new HttpClient(handler);
            var config = new AsaasConfig { ApiUrl = "https://sandbox.asaas.com/api/v3", PlatformApiKey = "plat-key", TimeoutSeconds = 10 };
            return (new AsaasClient(http, config), handler);
        }

        [Fact]
        public async Task CriarSubconta_UsaPlatformKeyEPostEmAccounts()
        {
            var (client, handler) = Criar("{\"id\":\"acc_1\",\"apiKey\":\"sub_key_1\"}");

            var retorno = await client.CriarSubcontaAsync(new AsaasSubcontaRequest("Loja", "loja@x.com", "12345678000199"));

            retorno.Id.Should().Be("acc_1");
            retorno.ApiKey.Should().Be("sub_key_1");
            handler.UltimaRequisicao!.Method.Should().Be(HttpMethod.Post);
            handler.UltimaRequisicao.RequestUri!.AbsolutePath.Should().EndWith("/accounts");
            handler.UltimaRequisicao.Headers.GetValues("access_token").Should().Contain("plat-key");
            handler.UltimoCorpo.Should().Contain("Loja");
        }

        [Fact]
        public async Task CriarCobranca_UsaApiKeyDaSubconta()
        {
            var (client, handler) = Criar("{\"id\":\"pay_1\",\"status\":\"PENDING\",\"value\":100.0}");

            var retorno = await client.CriarCobrancaAsync("sub-key",
                new AsaasCobrancaRequest("cus_1", "PIX", 100m, "2026-07-01"));

            retorno.Id.Should().Be("pay_1");
            retorno.Status.Should().Be("PENDING");
            handler.UltimaRequisicao!.RequestUri!.AbsolutePath.Should().EndWith("/payments");
            handler.UltimaRequisicao.Headers.GetValues("access_token").Should().Contain("sub-key");
        }

        [Fact]
        public async Task ObterPixQrCode_UsaGetNaRotaCorreta()
        {
            var (client, handler) = Criar("{\"payload\":\"00020126...\"}");

            var retorno = await client.ObterPixQrCodeAsync("sub-key", "pay_99");

            retorno.Payload.Should().StartWith("00020126");
            handler.UltimaRequisicao!.Method.Should().Be(HttpMethod.Get);
            handler.UltimaRequisicao.RequestUri!.AbsolutePath.Should().EndWith("/payments/pay_99/pixQrCode");
        }

        [Fact]
        public async Task RespostaDeErro_LancaExcecao()
        {
            var handler = new FakeHttpMessageHandler("erro", HttpStatusCode.BadRequest);
            var client = new AsaasClient(new HttpClient(handler), new AsaasConfig { ApiUrl = "https://sandbox.asaas.com/api/v3" });

            var act = async () => await client.CriarClienteAsync("k", new AsaasClienteRequest("Fulano", "12345678900"));

            await act.Should().ThrowAsync<AsaasApiException>();
        }
    }
}
