# Libify — Infraestrutura AWS

## Conta AWS (dev)

| Campo | Valor |
|-------|-------|
| Account ID | `635198174869` |
| Região | `sa-east-1` |
| IAM User | `flavio.barbosa` |
| Profile local | `libify-dev` |

Credenciais ficam em `%USERPROFILE%\.aws\credentials` (fora do git).

## Preparar sessão

```powershell
cd D:\Projetos\LibiFy\profissionaisliberais
. .\infra\scripts\use-libify-dev.ps1
```

## Verificar ambiente

```powershell
.\infra\scripts\check-prerequisites.ps1
```

## Ferramentas instaladas

| Ferramenta | Status |
|------------|--------|
| AWS CLI 2.x | Instalado |
| Terraform 1.15.x | Instalado |
| Docker Desktop | OK (compose `libify-dev`) |
| .NET 10 | OK |
| Node.js | OK |
| Git | OK |
| Ansible | Opcional (WSL) — fase deploy |

## Status do deploy (dev)

| Componente | Status |
|------------|--------|
| Terraform (VPC, RDS, EC2, ECR, Route53, SSM) | OK |
| Migrations RDS | OK |
| API + Caddy + RabbitMQ + Redis + Aspire | OK |
| Worker | OK |
| **Registro.br** | Pendente (pode pular por enquanto) |
| Amplify front | Deploy manual OK — GitHub CI/CD pendente (PAT) |

## Acesso via IP (sem DNS)

Use estes endpoints ate delegar NS no Registro.br:

| URL | Uso |
|-----|-----|
| `http://54.232.84.74/health` | Health check API |
| `http://54.232.84.74/swagger` | Swagger |
| `http://54.232.84.74/api/v1/...` | Endpoints REST |

IP: **54.232.84.74** | Instancia: `i-09b83e8513a37faa4`

**Front:** Amplify e HTTPS; a API no IP e HTTP — o browser bloqueia mixed content. Ate o DNS:
- testar API/Swagger pelo IP acima, ou
- rodar o front local: `VITE_API_BASE_URL=http://54.232.84.74/api/v1 npm run dev`

URL Amplify (quando conectar repo): `https://develop.d10vcq31qfr2rm.amplifyapp.com`

## Amplify (front)

| Item | Valor |
|------|-------|
| App ID | `d10vcq31qfr2rm` |
| Repo | https://github.com/flaviobarbosac/prosiffionaisliberais-front |
| Branch | `develop` |
| URL temporaria | https://develop.d10vcq31qfr2rm.amplifyapp.com |

### Conectar GitHub (CI/CD automatico)

Requer **GitHub PAT** (permissoes `repo` + `admin:repo_hook`) e [AWS Amplify GitHub App](https://github.com/apps/aws-amplify-sa-east-1) instalado na conta `flaviobarbosac`.

```powershell
$env:GITHUB_ACCESS_TOKEN = "ghp_..."
.\infra\scripts\connect-amplify-github.ps1
```

Ou via Terraform (`infra/terraform/terraform.tfvars`):

```hcl
github_access_token = "ghp_..."
```

```powershell
cd infra/terraform; terraform apply
```

### Deploy manual (sem GitHub)

```powershell
.\infra\scripts\deploy-amplify-manual.ps1
```

Backend (API/Worker) ja vem do repo https://github.com/flaviobarbosac/profissionaisliberais via imagens ECR — nao usa Amplify.

## Proximos passos

1. **Registro.br** — delegar NS (quando quiser dominio)
2. Amplify — conectar repo Git, branch `develop`
3. SSM — JWT real + secrets Asaas
4. Rotacionar Access Key exposta no chat

## DNS Registro.br (pendente)

```
ns-1526.awsdns-62.org
ns-1672.awsdns-17.co.uk
ns-240.awsdns-30.com
ns-969.awsdns-57.net
```

Depois: `https://api.dev.libify.com.br` | `https://aspire.dev.libify.com.br` | `https://www.dev.libify.com.br`

## Segurança

- **Rotacionar a Access Key** exposta no chat após concluir o setup.
- Nunca commitar `*.tfvars` com secrets (ver `infra/.gitignore`).
