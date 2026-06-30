namespace Libify.Services.Auth
{
    /// <summary>
    /// Convenção única da chave de cache do status de revogação de dispositivo,
    /// compartilhada entre o middleware de tenant (leitura) e o serviço (invalidação).
    /// </summary>
    public static class DispositivoRevocationCache
    {
        public static string Key(Guid dispositivoId) => $"disp-revogado:{dispositivoId}";
    }
}
