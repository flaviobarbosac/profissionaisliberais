# Libify — Infraestrutura AWS (Desenvolvimento)

Documento de referência com as decisões acordadas e o plano de preparação do ambiente.

## Decisões arquiteturais

| Item | Decisão |
|------|---------|
| **Região** | `sa-east-1` (São Paulo) |
| **Banco** | Amazon RDS PostgreSQL (não RedeHost) |
| **Backend** | `Libify.API` + `Libify.Worker` (containers .NET 10) |
| **Mensageria** | RabbitMQ (MassTransit — SQS ainda não implementado no código) |
| **Cache / rate limit** | Redis **omitido em dev** (fallback em memória já existe no código) |
| **Frontend** | React/Vite — hospedado na AWS |
| **Domínio** | Domínio público disponível |
| **Custo** | Arquitetura enxuta (sem NAT Gateway, ALB, Amazon MQ, ElastiCache) |

## Arquitetura alvo (dev barato)

```
Internet
   │
   ├── api.dev.<dominio>  ──► EC2 (Docker: API + Worker + RabbitMQ) :443/:8080
   ├── app.dev.<dominio>  ──► Amplify Hosting ou S3 + CloudFront (front Vite)
   │
   └── RDS PostgreSQL (subnet privada ou pública restrita por SG)
        └── db.t4g.micro, single-AZ, ~20 GB gp3

SSM Parameter Store ──► secrets (JWT, Asaas, connection string)
ECR ──► imagens libify-api e libify-worker
Route 53 (ou DNS externo) ──► registros A/CNAME
ACM ──► certificado TLS (sa-east-1 para ALB/EC2; us-east-1 se CloudFront no front)
```

### Por que EC2 e não ECS + ALB neste momento

| Serviço evitado | Custo aprox. dev | Motivo |
|-----------------|------------------|--------|
| NAT Gateway | ~US$ 32/mês | Subnets públicas + SG restritivo |
| Application Load Balancer | ~US$ 16+/mês | Nginx/Caddy no EC2 em dev |
| Amazon MQ | ~US$ 18+/mês | RabbitMQ no mesmo EC2 via Docker |
| ElastiCache Redis | ~US$ 12+/mês | Código funciona sem Redis em dev |

**Estimativa total dev:** ~US$ 25–45/mês (RDS + EC2 + tráfego mínimo + Amplify free tier).

## Variáveis de ambiente (ECS/EC2)

```
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Host=<rds>;Port=5432;Database=libify_dev;...
ConnectionStrings__RabbitMq=amqp://guest:guest@localhost:5672
Jwt__Key=<ssm>
Asaas__PlatformApiKey=<ssm>
Asaas__WebhookBaseUrl=https://api.dev.<dominio>
Cors__Origins__0=https://app.dev.<dominio>
```

Front (build-time):

```
VITE_API_BASE_URL=https://api.dev.<dominio>/api/v1
```

## Plugins AWS no Cursor

- **aws-knowledge:** documentação e boas práticas (não provisiona).
- **Amplify MCP:** offline no ambiente atual — corrigir em Cursor Settings se necessário.
- Provisionamento real: **Terraform + AWS CLI** (ou Ansible pós-provisionamento).

---

## Respostas às perguntas (2025-06-30)

### 1. Consigo gerar script em Terraform?

**Sim.** Terraform é a opção recomendada para este projeto:
- VPC, subnets, security groups
- RDS PostgreSQL
- EC2 + IAM role + user-data
- ECR, SSM, Route 53, ACM
- Amplify app ou S3 + CloudFront para o front

Estrutura prevista: pasta `infra/terraform/` com módulos ou stacks separados (`network`, `data`, `compute`, `front`).

### 2. Consigo criar script em Ansible?

**Sim.** Ansible complementa o Terraform (não substitui):
- Instalar Docker / Docker Compose no EC2
- Pull das imagens ECR
- Subir `docker-compose` de produção dev
- Configurar Nginx/Caddy + TLS
- Rodar migrations EF (`dotnet ef database update`)
- Health check pós-deploy

Estrutura prevista: `infra/ansible/` com `inventory`, `playbooks/deploy.yml`, `roles/`.

### 3. Consigo rodar e criar a infraestrutura?

**Sim, parcialmente automatizado**, desde que:
- AWS CLI autenticado na conta correta
- Terraform instalado
- Permissões IAM suficientes
- Domínio e zona DNS informados (Route 53 ou provedor externo)
- Aprovação explícita antes de `terraform apply` (gera custo na AWS)

**Passos manuais inevitáveis:**
- Configurar credenciais AWS na máquina
- Apontar DNS (se zona não estiver na Route 53)
- Validar certificado ACM (DNS ou e-mail)
- Primeiro push de imagens para ECR
- Secrets sensíveis (Asaas, JWT) — preferir SSM, não commitar

### 4. O que precisa estar instalado na máquina?

| Ferramenta | Versão mínima | Obrigatório |
|------------|---------------|-------------|
| **AWS CLI v2** | 2.x | Sim |
| **Terraform** | ≥ 1.5 | Sim |
| **Docker Desktop** | atual | Sim (build/push imagens) |
| **Git** | atual | Sim |
| **.NET SDK 10** | 10.x | Sim (migrations locais) |
| **Ansible** | ≥ 2.14 | Recomendado (deploy/config EC2) |
| **Node.js** | ≥ 20 | Sim (build front) |
| **Session Manager plugin** | — | Opcional (acesso EC2 sem SSH aberto) |

**Credenciais AWS:** perfil configurado (`aws configure` ou SSO) com permissões para VPC, EC2, RDS, ECR, SSM, Route53, ACM, Amplify/S3/CloudFront.

---

## Plano de preparação do ambiente

### Fase 0 — Informações necessárias (antes de codar IaC)

- [ ] Nome do domínio (ex.: `libify.com.br`)
- [ ] Subdomínios desejados (ex.: `api.dev.`, `app.dev.`)
- [ ] DNS na Route 53 ou externo (Cloudflare, Registro.br, etc.)
- [ ] ID da conta AWS e confirmação de billing
- [ ] Chaves Asaas sandbox (dev)
- [ ] Repositório Git do front e back (URLs para Amplify)

### Fase 1 — Instalar ferramentas locais (Windows)

```powershell
# Verificar o que já existe
aws --version
terraform --version
docker --version
dotnet --version
node --version

# Instalar (exemplos via winget)
winget install Amazon.AWSCLI
winget install Hashicorp.Terraform
# Docker Desktop: https://www.docker.com/products/docker-desktop/
# Ansible (WSL recomendado): pip install ansible no Ubuntu WSL
```

Validar acesso AWS:

```powershell
aws sts get-caller-identity
aws ec2 describe-regions --region-names sa-east-1
```

### Fase 2 — Bootstrap remoto (uma vez por conta/região)

```powershell
# Bucket/state lock para Terraform (será criado pelo Terraform ou manualmente)
# ECR repos serão criados pelo Terraform
aws ecr get-login-password --region sa-east-1
```

### Fase 3 — Criar IaC no repositório

- [ ] `infra/terraform/` — VPC, RDS, EC2, ECR, SSM, Route53, ACM, Amplify
- [ ] `infra/ansible/` — deploy pós-provisionamento
- [ ] `infra/docker/docker-compose.aws-dev.yml` — API + Worker + RabbitMQ + proxy
- [ ] `.env.example` / documentação de variáveis
- [ ] Backend Terraform (S3 + DynamoDB lock) — recomendado antes do apply em equipe

### Fase 4 — Build e push das imagens

```powershell
cd D:\Projetos\LibiFy\profissionaisliberais
aws ecr get-login-password --region sa-east-1 | docker login --username AWS --password-stdin <account>.dkr.ecr.sa-east-1.amazonaws.com
docker build -f Libify.API/Dockerfile -t libify-api .
docker build -f Libify.Worker/Dockerfile -t libify-worker .
# tag + push após ECR existir
```

### Fase 5 — Provisionar infra (Terraform)

```powershell
cd infra/terraform
terraform init
terraform plan -var-file=dev.tfvars
# revisar custos e recursos
terraform apply -var-file=dev.tfvars
```

### Fase 6 — Configurar EC2 (Ansible ou user-data)

- [ ] Docker Compose no EC2
- [ ] Nginx/Caddy com TLS (ACM no ALB ou Let's Encrypt no EC2)
- [ ] Variáveis vindas do SSM Parameter Store
- [ ] Migrations: `PROFLIB_CONNECTION` + `dotnet ef database update`

### Fase 7 — Front (Amplify)

- [ ] Conectar repo Git ao Amplify
- [ ] Build spec: `npm ci && npm run build`
- [ ] Env: `VITE_API_BASE_URL=https://api.dev.<dominio>/api/v1`
- [ ] Domínio customizado + certificado

### Fase 8 — DNS e validação

- [ ] Registros Route 53 / CNAME externo
- [ ] `https://api.dev.<dominio>/health` → 200
- [ ] `https://api.dev.<dominio>/swagger` (Development/Homologacao)
- [ ] Login no front apontando para API
- [ ] Webhook Asaas apontando para URL pública da API

### Fase 9 — Operacional dev

- [ ] Alarme básico CloudWatch (RDS storage, EC2 status)
- [ ] Tag `Environment=dev`, `Project=libify` em todos os recursos
- [ ] Política de parar EC2 fora do horário (opcional, economia)

---

## Ordem de execução recomendada

1. Fase 0 + Fase 1 (você)
2. Fase 3 (agente cria Terraform + Ansible no repo)
3. Fase 2 + Fase 5 (`terraform apply`)
4. Fase 4 (build/push imagens)
5. Fase 6 (Ansible deploy)
6. Fase 7 + 8 (front + DNS + testes)
7. Fase 9 (opcional)

---

## Próximo passo

Informar o **domínio** e se o **DNS está na Route 53**. Com isso, o agente pode gerar `infra/terraform/` e `infra/ansible/` na próxima iteração.
