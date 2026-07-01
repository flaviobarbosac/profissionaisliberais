using System.Security.Cryptography;
using System.Text;
using Libify.Domain.Ports;
using Libify.Infraestructure.Services;

namespace Libify.Infraestructure.Security
{
  /// <summary>Criptografia AES-GCM para segredos (API Keys Asaas) em repouso.</summary>
  public class AesSecretProtector : ISecretProtector
  {
    private readonly byte[] _key;

    public AesSecretProtector(AsaasConfig config)
    {
      var material = config.EncryptionKey ?? config.PlatformApiKey;
      if (string.IsNullOrWhiteSpace(material) || Encoding.UTF8.GetByteCount(material) < 32)
        material = "libify-dev-fallback-key-32bytes!!";

      _key = SHA256.HashData(Encoding.UTF8.GetBytes(material));
    }

    public string Protect(string plainText)
    {
      ArgumentException.ThrowIfNullOrEmpty(plainText);
      var nonce = RandomNumberGenerator.GetBytes(12);
      var plain = Encoding.UTF8.GetBytes(plainText);
      var cipher = new byte[plain.Length];
      var tag = new byte[16];

      using var aes = new AesGcm(_key, 16);
      aes.Encrypt(nonce, plain, cipher, tag);

      var payload = new byte[nonce.Length + tag.Length + cipher.Length];
      Buffer.BlockCopy(nonce, 0, payload, 0, nonce.Length);
      Buffer.BlockCopy(tag, 0, payload, nonce.Length, tag.Length);
      Buffer.BlockCopy(cipher, 0, payload, nonce.Length + tag.Length, cipher.Length);
      return Convert.ToBase64String(payload);
    }

    public string Unprotect(string protectedText)
    {
      ArgumentException.ThrowIfNullOrEmpty(protectedText);
      var payload = Convert.FromBase64String(protectedText);
      var nonce = payload.AsSpan(0, 12);
      var tag = payload.AsSpan(12, 16);
      var cipher = payload.AsSpan(28);

      var plain = new byte[cipher.Length];
      using var aes = new AesGcm(_key, 16);
      aes.Decrypt(nonce, cipher, tag, plain);
      return Encoding.UTF8.GetString(plain);
    }
  }
}
