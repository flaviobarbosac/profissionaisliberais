namespace Libify.API.Dto
{
    public class GoogleLoginRequest
    {
        /// <summary>id_token emitido pelo Google Sign-In no cliente.</summary>
        public string IdToken { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string? DeviceNome { get; set; }
        public string? Plataforma { get; set; }
        public string? AppVersion { get; set; }
    }

    public class RefreshRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string? DeviceNome { get; set; }
        public string? Plataforma { get; set; }
        public string? AppVersion { get; set; }
    }

    public class AuthResponse
    {
        public Guid UsuarioId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public Guid DispositivoId { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiraEm { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshExpiraEm { get; set; }
    }

    public class DispositivoDto
    {
        public Guid Id { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string? Nome { get; set; }
        public string? Plataforma { get; set; }
        public string? AppVersion { get; set; }
        public string? Ip { get; set; }
        public DateTime? UltimoAcessoEm { get; set; }
        public bool Revogado { get; set; }
    }
}
