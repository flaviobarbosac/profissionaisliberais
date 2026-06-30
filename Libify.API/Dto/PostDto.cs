namespace Libify.API.Dto
{
    public class PostDto
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string? FotoAntesUrl { get; set; }
        public string? FotoDepoisUrl { get; set; }
        public string? Legenda { get; set; }
        public string? Hashtags { get; set; }
        public bool Publicado { get; set; }
        public DateTime? PublicadoEm { get; set; }
    }
}
