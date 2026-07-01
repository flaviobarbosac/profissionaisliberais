#Requires -Version 5.1
# Deploy no EC2 via SSM (copia compose + sobe stack)
param(
    [string]$InstanceId = "",
    [string]$Profile = "libify-dev",
    [string]$Region = "sa-east-1"
)

$ErrorActionPreference = "Stop"
$env:AWS_PROFILE = $Profile
$env:AWS_REGION = $Region

if (-not $InstanceId) {
    Push-Location "$PSScriptRoot\..\terraform"
    $InstanceId = terraform output -raw ec2_instance_id 2>$null
    Pop-Location
    if (-not $InstanceId) { throw "Informe -InstanceId ou rode terraform apply antes." }
}

Write-Host "Deploy na instancia $InstanceId ..." -ForegroundColor Cyan

$dockerDir = Join-Path $PSScriptRoot "..\docker"
$compose = Get-Content (Join-Path $dockerDir "docker-compose.aws.yml") -Raw
$caddy = Get-Content (Join-Path $dockerDir "Caddyfile") -Raw

# Envia arquivos via SSM
$commands = @(
    "mkdir -p /opt/libify",
    "cat > /opt/libify/docker-compose.yml << 'COMPOSE_EOF'",
    $compose,
    "COMPOSE_EOF",
    "cat > /opt/libify/Caddyfile << 'CADDY_EOF'",
    $caddy,
    "CADDY_EOF",
    @'
cd /opt/libify
export AWS_REGION=sa-east-1
export PROJECT=libify
export ENVIRONMENT=dev
export API_FQDN=$(aws ssm get-parameter --name /libify/dev/asaas/webhook_base_url --query Parameter.Value --output text | sed 's|https://||')
export ASPIRE_FQDN=aspire.dev.libify.com.br
export ECR_API=$(aws sts get-caller-identity --query Account --output text).dkr.ecr.sa-east-1.amazonaws.com/libify-dev-api
export ECR_WORKER=$(aws sts get-caller-identity --query Account --output text).dkr.ecr.sa-east-1.amazonaws.com/libify-dev-worker
export DB_CONNECTION=$(aws ssm get-parameter --name /libify/dev/db/connection_string --with-decryption --query Parameter.Value --output text)
export JWT_KEY=$(aws ssm get-parameter --name /libify/dev/jwt/key --with-decryption --query Parameter.Value --output text)
export ASAAS_WEBHOOK_BASE_URL=$(aws ssm get-parameter --name /libify/dev/asaas/webhook_base_url --query Parameter.Value --output text)
export CORS_ORIGIN=$(aws ssm get-parameter --name /libify/dev/cors/origin --query Parameter.Value --output text)
aws ecr get-login-password --region sa-east-1 | docker login --username AWS --password-stdin $(aws sts get-caller-identity --query Account --output text).dkr.ecr.sa-east-1.amazonaws.com
docker compose pull || true
docker compose up -d
'@
)

# SSM Run Command com script inline simplificado
$script = @'
#!/bin/bash
set -e
mkdir -p /opt/libify
cd /opt/libify
aws s3 cp s3://PLACEHOLDER || true
'@

Write-Host "Use SSM Session Manager para concluir deploy manualmente:" -ForegroundColor Yellow
Write-Host "  aws ssm start-session --target $InstanceId --profile $Profile" -ForegroundColor Yellow
Write-Host "Copie /opt/libify/docker-compose.yml e Caddyfile do repo infra/docker/" -ForegroundColor Yellow
Write-Host "Ou aguarde playbook Ansible completo." -ForegroundColor Yellow
