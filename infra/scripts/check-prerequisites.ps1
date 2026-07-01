#Requires -Version 5.1
$ErrorActionPreference = "Stop"

. "$PSScriptRoot\use-libify-dev.ps1"

Write-Host "`n=== Libify - verificacao de pre-requisitos ===`n" -ForegroundColor Cyan

function Test-Command($name) {
    $cmd = Get-Command $name -ErrorAction SilentlyContinue
    if ($cmd) {
        Write-Host "(OK) $name -> $($cmd.Source)" -ForegroundColor Green
        return $true
    }
    Write-Host "(FALTA) $name nao encontrado no PATH" -ForegroundColor Red
    return $false
}

$ok = $true
$ok = (Test-Command aws) -and $ok
$ok = (Test-Command terraform) -and $ok
$ok = (Test-Command docker) -and $ok
$ok = (Test-Command dotnet) -and $ok
$ok = (Test-Command node) -and $ok
$ok = (Test-Command git) -and $ok

Write-Host "`n--- Versoes ---" -ForegroundColor Cyan
if (Get-Command aws -ErrorAction SilentlyContinue) { aws --version }
if (Get-Command terraform -ErrorAction SilentlyContinue) { terraform version }
if (Get-Command docker -ErrorAction SilentlyContinue) { docker --version }
if (Get-Command dotnet -ErrorAction SilentlyContinue) { Write-Host "dotnet $(dotnet --version)" }
if (Get-Command node -ErrorAction SilentlyContinue) { Write-Host "node $(node --version)" }

Write-Host "`n--- AWS (profile libify-dev) ---" -ForegroundColor Cyan
try {
    $identity = aws sts get-caller-identity | ConvertFrom-Json
    Write-Host "(OK) Account: $($identity.Account)" -ForegroundColor Green
    Write-Host "(OK) User:    $($identity.Arn)" -ForegroundColor Green
    $region = aws ec2 describe-regions --region-names sa-east-1 --query "Regions[0].RegionName" --output text 2>$null
    if ($region -eq "sa-east-1") {
        Write-Host "(OK) Regiao sa-east-1 acessivel" -ForegroundColor Green
    }
} catch {
    Write-Host "(ERRO) Credenciais AWS invalidas ou perfil libify-dev ausente" -ForegroundColor Red
    Write-Host 'Configure em %USERPROFILE%\.aws\credentials (secao libify-dev)' -ForegroundColor Yellow
    $ok = $false
}

Write-Host "`n--- Docker libify-dev ---" -ForegroundColor Cyan
try {
    $compose = docker compose ls --format json 2>$null | ConvertFrom-Json
    $libify = $compose | Where-Object { $_.Name -eq "libify-dev" }
    if ($libify) {
        Write-Host "(OK) Compose libify-dev ativo" -ForegroundColor Green
    } else {
        Write-Host "(AVISO) Compose libify-dev nao encontrado. Suba com:" -ForegroundColor Yellow
        Write-Host "  docker compose -f docker-compose.dev.yml up -d" -ForegroundColor Yellow
    }
} catch {
    Write-Host "(AVISO) Nao foi possivel listar compose projects" -ForegroundColor Yellow
}

Write-Host ""
if ($ok) {
    Write-Host "Ambiente pronto para codar/aplicar infra Terraform." -ForegroundColor Green
    exit 0
}
Write-Host "Corrija os itens FALTA/ERRO antes do terraform apply." -ForegroundColor Red
exit 1
