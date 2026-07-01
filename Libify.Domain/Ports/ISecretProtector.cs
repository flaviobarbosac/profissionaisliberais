namespace Libify.Domain.Ports
{
    /// <summary>Protege segredos em repouso (ex.: API Key da subconta Asaas).</summary>
    public interface ISecretProtector
    {
        string Protect(string plainText);
        string Unprotect(string protectedText);
    }
}
