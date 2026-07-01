# Cloudflare — DNS dev (temporario)

Ate delegar NS no Registro.br, use o Cloudflare para apontar subdominios dev ao EC2.

## Script (recomendado)

1. Crie token: https://dash.cloudflare.com/profile/api-tokens → **Edit zone DNS** → zone `libify.com.br`
2. Execute:

```powershell
$env:CLOUDFLARE_API_TOKEN = "seu-token"
.\infra\scripts\cloudflare-dns-dev.ps1
```

## Painel (manual)

1. https://dash.cloudflare.com → **libify.com.br** → **DNS** → **Records**
2. Crie ou edite:

| Type | Name | Content | Proxy |
|------|------|---------|-------|
| A | `api.dev` | `54.232.84.74` | Proxied (laranja) |
| A | `aspire.dev` | `54.232.84.74` | Proxied (laranja) |

3. **SSL/TLS** → Overview → modo **Flexible** (HTTPS no browser → HTTP no EC2:80)

## Validar

```powershell
curl https://api.dev.libify.com.br/health   # Healthy
```

Front Amplify ja usa `VITE_API_BASE_URL=https://api.dev.libify.com.br/api/v1`.

## Amanha (Registro.br)

Delegar NS para Route53 (Terraform) e remover registros dev duplicados no Cloudflare se a zona sair de la.
