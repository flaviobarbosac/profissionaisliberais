#Requires -Version 5.1
<#
.SYNOPSIS
  Conecta app Amplify existente ao repositorio GitHub (branch develop).
.DESCRIPTION
  Requer GitHub PAT com permissoes repo + admin:repo_hook.
  Instalar Amplify GitHub App: https://github.com/apps/aws-amplify-sa-east-1
  Token: https://github.com/settings/tokens (classic) ou fine-grained com Contents read + Webhooks write.

  $env:GITHUB_ACCESS_TOKEN = "ghp_..."
  .\infra\scripts\connect-amplify-github.ps1
#>
param(
    [string]$Profile = "libify-dev",
    [string]$Region = "sa-east-1",
    [string]$AppId = "",
    [string]$Repository = "https://github.com/flaviobarbosac/prosiffionaisliberais-front",
    [string]$Branch = "develop",
    [string]$AccessToken = $env:GITHUB_ACCESS_TOKEN
)

$ErrorActionPreference = "Stop"
$env:AWS_PROFILE = $Profile
$env:AWS_REGION = $Region

if (-not $AccessToken) {
    Write-Host "Defina GITHUB_ACCESS_TOKEN (PAT GitHub com repo + admin:repo_hook)." -ForegroundColor Red
    Write-Host "Exemplo: `$env:GITHUB_ACCESS_TOKEN = 'ghp_...'; .\infra\scripts\connect-amplify-github.ps1"
    exit 1
}

if (-not $AppId) {
    Push-Location "$PSScriptRoot\..\terraform"
    $AppId = terraform output -raw amplify_app_id
    Pop-Location
}

Write-Host "Conectando Amplify $AppId -> $Repository ($Branch)..." -ForegroundColor Cyan

aws amplify update-app `
    --app-id $AppId `
    --repository $Repository `
    --access-token $AccessToken `
    --enable-branch-auto-build `
    --output json | Out-Null

aws amplify update-branch `
    --app-id $AppId `
    --branch-name $Branch `
    --enable-auto-build `
    --output json | Out-Null

Write-Host "Iniciando build..." -ForegroundColor Cyan
$job = aws amplify start-job `
    --app-id $AppId `
    --branch-name $Branch `
    --job-type RELEASE `
    --output json | ConvertFrom-Json

Write-Host "Job: $($job.jobSummary.jobId) | Status: $($job.jobSummary.status)"
Write-Host "URL: https://${Branch}.${AppId}.amplifyapp.com" -ForegroundColor Green
