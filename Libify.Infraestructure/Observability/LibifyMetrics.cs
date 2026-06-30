using System.Diagnostics.Metrics;

namespace Libify.Infraestructure.Observability
{
    /// <summary>
    /// Métricas de negócio do Libify, expostas via OpenTelemetry Meter.
    /// </summary>
    public sealed class LibifyMetrics
    {
        public const string MeterName = "Libify";

        private readonly Counter<long> _loginGoogle;
        private readonly Counter<long> _loginFalha;
        private readonly Counter<long> _syncProcessado;

        public LibifyMetrics(IMeterFactory meterFactory)
        {
            var meter = meterFactory.Create(MeterName);
            _loginGoogle = meter.CreateCounter<long>("libify.login.google");
            _loginFalha = meter.CreateCounter<long>("libify.login.falha");
            _syncProcessado = meter.CreateCounter<long>("libify.sync.processado");
        }

        public void LoginGoogle() => _loginGoogle.Add(1);
        public void LoginFalha(string motivo) => _loginFalha.Add(1, new KeyValuePair<string, object?>("motivo", motivo));
        public void SyncProcessado(string modulo, string operacao)
            => _syncProcessado.Add(1,
                new KeyValuePair<string, object?>("modulo", modulo),
                new KeyValuePair<string, object?>("operacao", operacao));
    }
}
