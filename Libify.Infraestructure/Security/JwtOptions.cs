namespace Libify.Infraestructure.Security
{
    public class JwtOptions
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = "Libify";
        public string Audience { get; set; } = "Libify";
        public int AccessTokenMinutes { get; set; } = 60;
        public int RefreshTokenDays { get; set; } = 30;
    }
}
